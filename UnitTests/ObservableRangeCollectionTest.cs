using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading;
using NUnit.Framework;
using ObservableRangeCollection;
using static System.Collections.Specialized.NotifyCollectionChangedAction;
using static NUnit.Framework.Assert;
using static ObservableRangeCollection.ObservableRangeCollectionBase<UnitTests.TestEntity>;

namespace UnitTests
{
    public abstract class ObservableRangeCollectionContext
    {
        private ObservableRangeCollection<TestEntity> _collection;

        [SetUp]
        public void ObservableRangeCollectionContextSetup()
        {
            _collection = new ObservableRangeCollection<TestEntity>();
        }

        private void AssertCollectionSizeIs( int expectedSize )
        {
            AreEqual( expectedSize, _collection.Count );
        }

        private void AddAndRaiseEvents( params TestEntity[] range )
        {
            _collection.AddAndRaiseEvents( new List<TestEntity>( range ) );
        }

        private void AddRange( params TestEntity[] range )
        {
            _collection.AddRange( range );
        }

        private void ReplaceRange( params TestEntity[] range )
        {
            _collection.ReplaceRange( range );
        }

        [TestFixture]
        public class ObservableRangeCollectionTest : ObservableRangeCollectionContext
        {
            private void AssertCollectionContains( TestEntity entity )
            {
                if ( entity == null ) throw new ArgumentNullException( nameof( entity ) );

                True( _collection.Contains( entity ) );
            }

            [Test]
            public void ObservableRangeCollection_ShouldBeInstanceOfObservableCollection()
            {
                IsInstanceOf<ObservableCollection<TestEntity>>( _collection );
            }

            [Test]
            public void WhenAddingNullRange_AddRangeThrowsNullRange()
            {
                Throws<NullRange>( () => AddRange( null ) );
            }

            [Test]
            public void AddRangeShouldCall_AddAndRaiseEvents()
            {
                var stubCollection = new ObservableRangeCollectionBaseStub();

                var testEntities = new[] { new TestEntity() };
                stubCollection.AddRange( testEntities );

                True( stubCollection.IsAddAndRaiseEventsCalled );
                AreEqual( testEntities, stubCollection.ToAddItems );
            }

            [Test]
            public void WhenAddingEmptyRange_CollectionShouldNotChange()
            {
                AddAndRaiseEvents();

                AssertCollectionSizeIs( 0 );
            }

            [Test]
            public void WhenRangeOneIsAdded_CollectionShouldContainRangeOne()
            {
                var one = new[] { new TestEntity() };

                AddAndRaiseEvents( one );

                foreach ( var entity in one ) AssertCollectionContains( entity );
            }

            [Test]
            public void WhenRangeOneAndTwoAreAdded_CollectionShouldContainRangeOneAndTwo()
            {
                var one = new[] { new TestEntity() };
                var two = new[] { new TestEntity(), new TestEntity() };

                AddAndRaiseEvents( one );
                AddAndRaiseEvents( two );

                AssertCollectionSizeIs( 3 );
                foreach ( var entity in one ) AssertCollectionContains( entity );
                foreach ( var entity in two ) AssertCollectionContains( entity );
            }

            [Test]
            public void ReplaceItemsShouldClearRangeOneAndCallAddAndRaiseEventsToAddRangeTwo()
            {
                var rangeOne = new TestEntity();
                var stubCollection = new ObservableRangeCollectionBaseStub { rangeOne };

                var rangeTwo = new List<TestEntity> { new TestEntity(), new TestEntity() };
                stubCollection.ReplaceItems( rangeTwo );

                False( stubCollection.Contains( rangeOne ) );
                True( stubCollection.IsAddAndRaiseEventsCalled );
                AreEqual( rangeTwo, stubCollection.ToAddItems );
                AreEqual( 0, stubCollection.CountMock, "Clear() got called before AddAndRaiseEvents()!" );
            }

            [Test]
            public void WhenRangeIsNull_ReplaceRangeShouldThrowNullRangeWithoutChangingCollection()
            {
                var oldRange = new TestEntity();
                _collection.Add( oldRange );

                Throws<NullRange>( () => ReplaceRange( null ) );
                AreEqual( oldRange, _collection[0] );
            }

            [Test]
            public void ReplaceRangeShouldCallReplaceItems()
            {
                var stubCollection = new ObservableRangeCollectionBaseStub { new TestEntity() };

                var testEntities = new List<TestEntity> { new TestEntity(), new TestEntity() };
                stubCollection.ReplaceRange( testEntities );

                True( stubCollection.IsReplaceItemsCalled );
                AreEqual( testEntities, stubCollection.ToAddItems );
            }

            [Test]
            public void WhenItemIsNull_ReplaceThrowNullItem()
            {
                var oldRange = new TestEntity();
                _collection.Add( oldRange );

                Throws<NullItem>( () => _collection.Replace( null ) );
                AreEqual( oldRange, _collection[0] );
            }

            [Test]
            public void ReplaceShouldCallReplaceItems()
            {
                var item = new TestEntity();
                var stubCollection = new ObservableRangeCollectionBaseStub();

                stubCollection.Replace( item );

                True( stubCollection.IsReplaceItemsCalled );
                AreEqual( item, stubCollection.ReplacedItem );
            }
        }

        public class ObservableRangeCollectionEventContext : ObservableRangeCollectionContext
        {
            private readonly AutoResetEvent _eventNotifier = new AutoResetEvent( false );

            [SetUp]
            public void ObservableRangeCollectionEventContextSetup()
            {
                _eventNotifier.Reset();
            }

            private void AssertAnEventRaised()
            {
                True( _eventNotifier.WaitOne( 1500 ) );
            }

            private void AssertNoEventRaised()
            {
                False( _eventNotifier.WaitOne( 1500 ) );
            }

            [TestFixture]
            public class ObservableRangeCollectionChangedEventsTest : ObservableRangeCollectionEventContext
            {
                private NotifyCollectionChangedEventArgs _eventArgs;
                private int _collectionCountWhenEventRaised;

                [SetUp]
                public void ObservableRangeCollectionEventsTestSetup()
                {
                    _eventArgs = null;
                    _collectionCountWhenEventRaised = 0;
                    _collection.CollectionChanged += OnCollectionChanged();

                    NotifyCollectionChangedEventHandler OnCollectionChanged()
                    {
                        return ( sender, args ) =>
                        {
                            _collectionCountWhenEventRaised = _collection.Count;
                            _eventArgs = args;
                            _eventNotifier.Set();
                        };
                    }
                }

                private void AssertCollectionChangedEventActionIs(
                    NotifyCollectionChangedAction collectionChangedAction )
                {
                    AreEqual( collectionChangedAction, _eventArgs.Action );
                }

                [Test]
                public void WhenAddingRangeOneToEmptyCollection_ShouldRaiseCollectionChangedWithResetAction()
                {
                    var one = new[] { new TestEntity() };

                    AddAndRaiseEvents( one );

                    AssertAnEventRaised();
                    AssertCollectionChangedEventActionIs( Reset );
                    AssertCollectionSizeIs( _collectionCountWhenEventRaised );
                }

                [Test]
                public void WhenAddingRangeOneToNonEmptyCollection_ShouldRaiseCollectionChangedWithAddAction()
                {
                    AddAndRaiseEvents( new TestEntity(), new TestEntity() );
                    var one = new[] { new TestEntity() };

                    AddAndRaiseEvents( one );

                    AssertAnEventRaised();
                    AssertCollectionChangedEventActionIs( Add );
                    AssertCollectionSizeIs( _collectionCountWhenEventRaised );
                    IsNull( _eventArgs.OldItems );
                    AreEqual( 2, _eventArgs.NewStartingIndex );
                    for ( var i = 0; i < one.Length; i++ )
                        AreEqual( one[i], _eventArgs.NewItems[i] );
                }

                [Test]
                public void WhenAddingEmptyRange_NoEventsShouldBeRaised()
                {
                    AddAndRaiseEvents();

                    AssertNoEventRaised();
                }

                [Test]
                public void WhenAddingRange_ShouldOnlyRaiseOneCollectionChangedEvent()
                {
                    var eventRaisesCount = 0;
                    _collection.CollectionChanged += ( sender, args ) =>
                    {
                        ++eventRaisesCount;

                        _eventNotifier.Set();
                    };

                    AddAndRaiseEvents( new TestEntity(), new TestEntity(), new TestEntity() );

                    AssertAnEventRaised();
                    AreEqual( 1, eventRaisesCount );
                }

                [Test]
                public void WhenCollectionChangedHasMultiSubsAndIsBeingModified_AddRangeThrowsInvalidOperation()
                {
                    _collection.CollectionChanged += OnCollectionChanged;
                    _collection.CollectionChanged += ( sender, args ) => { };

                    AddRange( new TestEntity(), new TestEntity(), new TestEntity() );

                    void OnCollectionChanged( object sender, NotifyCollectionChangedEventArgs args )
                    {
                        _collection.CollectionChanged -= OnCollectionChanged;

                        Throws<InvalidOperationException>( () => AddRange( new TestEntity() ) );
                    }
                }

                [Test]
                public void WhenCollectionChangedHasMultiSubsAndIsBeingModified_ReplaceRangeThrowsInvalidOperation()
                {
                    _collection.CollectionChanged += OnCollectionChanged;
                    _collection.CollectionChanged += ( sender, args ) => { };

                    ReplaceRange( new TestEntity(), new TestEntity(), new TestEntity() );

                    void OnCollectionChanged( object sender, NotifyCollectionChangedEventArgs args )
                    {
                        _collection.CollectionChanged -= OnCollectionChanged;

                        Throws<InvalidOperationException>( () => ReplaceRange( new TestEntity() ) );
                    }
                }

                [Test]
                public void WhenCollectionChangedHasMultiSubsAndIsBeingModified_ReplaceThrowsInvalidOperation()
                {
                    _collection.CollectionChanged += OnCollectionChanged;
                    _collection.CollectionChanged += ( sender, args ) => { };

                    _collection.Replace( new TestEntity() );

                    void OnCollectionChanged( object sender, NotifyCollectionChangedEventArgs args )
                    {
                        _collection.CollectionChanged -= OnCollectionChanged;

                        Throws<InvalidOperationException>( () => _collection.Replace( new TestEntity() ) );
                    }
                }
            }

            [TestFixture]
            public class ObservableRangeCollectionPropertyChangedEventTest : ObservableRangeCollectionEventContext
            {
                private List<string> _propertiesNames;

                [SetUp]
                public void ObservableRangeCollectionPropertyChangedEventSetup()
                {
                    _propertiesNames = new List<string>();
                    ( (INotifyPropertyChanged) _collection ).PropertyChanged += ( sender, args ) =>
                    {
                        if ( args != null ) _propertiesNames.Add( args.PropertyName );
                        _eventNotifier.Set();
                    };
                }

                [Test]
                public void WhenAddingRange_ShouldRaisePropertyChanged()
                {
                    AddAndRaiseEvents( new TestEntity() );

                    AssertAnEventRaised();
                    AreEqual( 2, _propertiesNames.Count );
                    AreEqual( "Count", _propertiesNames[0] );
                    AreEqual( "Items", _propertiesNames[1] );
                }

                [Test]
                public void WhenAddingEmptyRange_NoEventsShouldBeRaised()
                {
                    AddAndRaiseEvents();

                    AssertNoEventRaised();
                }
            }
        }

        private class ObservableRangeCollectionBaseStub : ObservableRangeCollection<TestEntity>
        {
            public List<TestEntity> ToAddItems { get; private set; }

            public int CountMock { get; private set; } = -1;

            protected internal override void AddAndRaiseEvents( List<TestEntity> toAddItems )
            {
                CountMock = Count;
                ToAddItems = toAddItems;
                IsAddAndRaiseEventsCalled = true;
            }

            protected internal override void ReplaceItems( List<TestEntity> items )
            {
                base.ReplaceItems( items );
                IsReplaceItemsCalled = true;
                ReplacedItem = items[0];
            }

            public bool IsAddAndRaiseEventsCalled { get; private set; }

            public bool IsReplaceItemsCalled { get; private set; }

            public TestEntity ReplacedItem { get; private set; }
        }
    }
}
# MVVM Utils

[![Build status](https://dev.azure.com/yahya-tinani/mvvm-utils/_apis/build/status/mvvm-utils%20CI)](https://dev.azure.com/yahya-tinani/mvvm-utils/_build/latest?definitionId=1)![Release status](https://vsrm.dev.azure.com/yahya-tinani/_apis/public/Release/badge/f481c3b8-48a1-4acc-8604-db8291e96afa/2/2)[![NuGet](https://img.shields.io/nuget/v/MvvmUtils.svg?label=NuGet)](https://www.nuget.org/packages/MvvmUtils/)![](https://img.shields.io/azure-devops/coverage/yahya-tinani/mvvm-utils/1)![](https://img.shields.io/nuget/dt/MvvmUtils.svg)![](https://img.shields.io/github/license/whyrising/mvvm-utils.svg)

MVVM Utils is intended to save developers' time by providing a bunch of handy classes that replace some of the repetitive code that we may have to write in a MVVM application.

### ObservableRangeCollection

It's an ObservableCollection that can add, remove and replace a group of items but in a better optimized way, by working with ranges, so instead of notifying the UI every time an item is added or removed, It notifies the UI only once, after the intended operation is done.

API:

- **AddRange** : If the collection is empty, It raises OnCollectionChanged  with NotifyCollectionChangedAction.Reset, otherwise, It raises OnCollectionChanged with NotifyCollectionChangedAction.Add along with the added items and the starting index.
- **RemoveRange** : It raises OnCollectionChanged with NotifyCollectionChangedAction.Reset.
- **RemoveRangeWithRemoveAction** : It checks if the removed range is consecutive, if so, the startingIndex is determined, otherwise, the starting index is set to -1, then it raises OnCollectionChanged with NotifyCollectionChangedAction.Remove along with the starting index and the removed items.
- **ReplaceRange**
- **Replace**
- **GetRange**: returns the requested range based on a given starting **index** and the **count** of the elements of this range.

N.B.: this ObservableCollection is more of a remake, inspired by [jamesmontemagno](https://github.com/jamesmontemagno/mvvm-helpers/tree/master/MvvmHelpers) :   ), I tried to rewrite it my way, and hopefully did some improvements XD.

## Wanna Join Me?

You can report bugs, ask for more features, suggestions, optimizations or even send some pull requests, you'll know what to do :  ).
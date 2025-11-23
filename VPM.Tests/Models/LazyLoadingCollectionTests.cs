using Xunit;
using VPM.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace VPM.Tests.Models
{
    public class LazyLoadingCollectionTests
    {
        [Fact]
        public void Constructor_InitializesEmpty()
        {
            var collection = new LazyLoadingCollection<int>();

            Assert.Empty(collection);
            Assert.Equal(0, collection.TotalCount);
            Assert.Equal(0, collection.LoadedCount);
            Assert.True(collection.IsFullyLoaded);
        }

        [Fact]
        public void Constructor_CustomParameters_SetsCorrectly()
        {
            var collection = new LazyLoadingCollection<int>(500, 250);

            Assert.NotNull(collection);
        }

        [Fact]
        public void SetAllItems_LoadsInitialSubset()
        {
            var collection = new LazyLoadingCollection<int>(10, 5);
            var items = Enumerable.Range(1, 100);

            collection.SetAllItems(items);

            Assert.Equal(100, collection.TotalCount);
            Assert.Equal(10, collection.LoadedCount);
            Assert.False(collection.IsFullyLoaded);
        }

        [Fact]
        public void SetAllItems_NullItems_DoesNothing()
        {
            var collection = new LazyLoadingCollection<int>();

            collection.SetAllItems(null);

            Assert.Equal(0, collection.TotalCount);
        }

        [Fact]
        public void SetAllItems_EmptyItems_InitializesEmpty()
        {
            var collection = new LazyLoadingCollection<int>();

            collection.SetAllItems(new List<int>());

            Assert.Equal(0, collection.TotalCount);
            Assert.True(collection.IsFullyLoaded);
        }

        [Fact]
        public void SetAllItems_FewerThanInitialLoad_LoadsAll()
        {
            var collection = new LazyLoadingCollection<int>(100, 50);
            var items = Enumerable.Range(1, 50);

            collection.SetAllItems(items);

            Assert.Equal(50, collection.TotalCount);
            Assert.Equal(50, collection.LoadedCount);
            Assert.True(collection.IsFullyLoaded);
        }

        [Fact]
        public void LoadMoreItems_LoadsNextBatch()
        {
            var collection = new LazyLoadingCollection<int>(10, 5);
            var items = Enumerable.Range(1, 100);
            collection.SetAllItems(items);

            collection.LoadMoreItems();

            Assert.Equal(15, collection.LoadedCount);
            Assert.False(collection.IsFullyLoaded);
        }

        [Fact]
        public void LoadMoreItems_NearEnd_LoadsRemaining()
        {
            var collection = new LazyLoadingCollection<int>(90, 20);
            var items = Enumerable.Range(1, 100);
            collection.SetAllItems(items);

            collection.LoadMoreItems();

            Assert.Equal(100, collection.LoadedCount);
            Assert.True(collection.IsFullyLoaded);
        }

        [Fact]
        public void LoadMoreItems_AlreadyFullyLoaded_DoesNothing()
        {
            var collection = new LazyLoadingCollection<int>(50, 25);
            var items = Enumerable.Range(1, 50);
            collection.SetAllItems(items);

            var beforeCount = collection.LoadedCount;
            collection.LoadMoreItems();

            Assert.Equal(beforeCount, collection.LoadedCount);
        }

        [Fact]
        public void LoadAllItems_LoadsAllRemaining()
        {
            var collection = new LazyLoadingCollection<int>(10, 5);
            var items = Enumerable.Range(1, 100);
            collection.SetAllItems(items);

            collection.LoadAllItems();

            Assert.Equal(100, collection.LoadedCount);
            Assert.True(collection.IsFullyLoaded);
        }

        [Fact]
        public void LoadAllItems_AlreadyLoaded_DoesNothing()
        {
            var collection = new LazyLoadingCollection<int>(50, 25);
            var items = Enumerable.Range(1, 50);
            collection.SetAllItems(items);

            collection.LoadAllItems();

            Assert.Equal(50, collection.LoadedCount);
            Assert.True(collection.IsFullyLoaded);
        }

        [Fact]
        public void AddItems_AddsToTotalAndLoadsVisiblePortion()
        {
            var collection = new LazyLoadingCollection<int>(10, 5);
            var initialItems = Enumerable.Range(1, 20);
            collection.SetAllItems(initialItems);

            var newItems = Enumerable.Range(21, 10);
            collection.AddItems(newItems);

            Assert.Equal(30, collection.TotalCount);
        }

        [Fact]
        public void AddItems_NullItems_DoesNothing()
        {
            var collection = new LazyLoadingCollection<int>(10, 5);
            var items = Enumerable.Range(1, 20);
            collection.SetAllItems(items);

            var beforeCount = collection.TotalCount;
            collection.AddItems(null);

            Assert.Equal(beforeCount, collection.TotalCount);
        }

        [Fact]
        public void AddItems_EmptyItems_DoesNothing()
        {
            var collection = new LazyLoadingCollection<int>(10, 5);
            var items = Enumerable.Range(1, 20);
            collection.SetAllItems(items);

            var beforeCount = collection.TotalCount;
            collection.AddItems(new List<int>());

            Assert.Equal(beforeCount, collection.TotalCount);
        }

        [Fact]
        public void IsFullyLoaded_InitialState_ReturnsFalse()
        {
            var collection = new LazyLoadingCollection<int>(10, 5);
            var items = Enumerable.Range(1, 100);
            collection.SetAllItems(items);

            Assert.False(collection.IsFullyLoaded);
        }

        [Fact]
        public void IsFullyLoaded_AfterLoadAll_ReturnsTrue()
        {
            var collection = new LazyLoadingCollection<int>(10, 5);
            var items = Enumerable.Range(1, 100);
            collection.SetAllItems(items);

            collection.LoadAllItems();

            Assert.True(collection.IsFullyLoaded);
        }

        [Fact]
        public void SetAllItems_FiresCollectionChangedEvent()
        {
            var collection = new LazyLoadingCollection<int>(10, 5);
            bool eventRaised = false;

            collection.CollectionChanged += (s, e) =>
            {
                eventRaised = true;
                Assert.Equal(NotifyCollectionChangedAction.Reset, e.Action);
            };

            collection.SetAllItems(Enumerable.Range(1, 100));

            Assert.True(eventRaised);
        }

        [Fact]
        public void LoadMoreItems_FiresCollectionChangedEvent()
        {
            var collection = new LazyLoadingCollection<int>(10, 5);
            collection.SetAllItems(Enumerable.Range(1, 100));

            bool eventRaised = false;
            collection.CollectionChanged += (s, e) => eventRaised = true;

            collection.LoadMoreItems();

            Assert.True(eventRaised);
        }

        [Fact]
        public void SetAllItems_UpdatesTotalCountProperty()
        {
            var collection = new LazyLoadingCollection<int>(10, 5);
            bool propertyChanged = false;
            string changedProperty = null;

            ((INotifyPropertyChanged)collection).PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(collection.TotalCount))
                {
                    propertyChanged = true;
                    changedProperty = e.PropertyName;
                }
            };

            collection.SetAllItems(Enumerable.Range(1, 100));

            Assert.True(propertyChanged);
            Assert.Equal(nameof(collection.TotalCount), changedProperty);
        }

        [Fact]
        public void LoadedCount_ReflectsCurrentlyLoadedItems()
        {
            var collection = new LazyLoadingCollection<int>(10, 5);
            var items = Enumerable.Range(1, 100);
            collection.SetAllItems(items);

            Assert.Equal(10, collection.LoadedCount);

            collection.LoadMoreItems();
            Assert.Equal(15, collection.LoadedCount);
        }

        [Fact]
        public void SetAllItems_PreservesOrder()
        {
            var collection = new LazyLoadingCollection<int>(10, 5);
            var items = new[] { 5, 3, 8, 1, 9, 2, 7, 4, 6, 10 };

            collection.SetAllItems(items);

            Assert.Equal(items.Take(10), collection.ToArray());
        }

        [Fact]
        public async Task LoadAllItemsAsync_LoadsAllWithChunking()
        {
            var collection = new LazyLoadingCollection<int>(10, 5);
            var items = Enumerable.Range(1, 100);
            collection.SetAllItems(items);

            await collection.LoadAllItemsAsync(chunkSize: 20, delayMs: 1);

            Assert.Equal(100, collection.LoadedCount);
            Assert.True(collection.IsFullyLoaded);
        }

        [Fact]
        public async Task LoadAllItemsAsync_AlreadyLoaded_DoesNothing()
        {
            var collection = new LazyLoadingCollection<int>(50, 25);
            var items = Enumerable.Range(1, 50);
            collection.SetAllItems(items);

            var beforeCount = collection.LoadedCount;
            await collection.LoadAllItemsAsync();

            Assert.Equal(beforeCount, collection.LoadedCount);
        }

        [Fact]
        public void TotalCount_ReflectsAllAvailableItems()
        {
            var collection = new LazyLoadingCollection<int>(10, 5);
            var items = Enumerable.Range(1, 1000);

            collection.SetAllItems(items);

            Assert.Equal(1000, collection.TotalCount);
            Assert.Equal(10, collection.LoadedCount);
        }

        [Fact]
        public void LoadMoreItems_MultipleCallsProgressivelyLoad()
        {
            var collection = new LazyLoadingCollection<int>(10, 5);
            var items = Enumerable.Range(1, 30);
            collection.SetAllItems(items);

            Assert.Equal(10, collection.LoadedCount);

            collection.LoadMoreItems();
            Assert.Equal(15, collection.LoadedCount);

            collection.LoadMoreItems();
            Assert.Equal(20, collection.LoadedCount);

            collection.LoadMoreItems();
            Assert.Equal(25, collection.LoadedCount);

            collection.LoadMoreItems();
            Assert.Equal(30, collection.LoadedCount);
            Assert.True(collection.IsFullyLoaded);
        }
    }
}

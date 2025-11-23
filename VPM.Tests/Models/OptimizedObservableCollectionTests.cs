using Xunit;
using VPM.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace VPM.Tests.Models
{
    public class OptimizedObservableCollectionTests
    {
        [Fact]
        public void Constructor_InitializesEmpty()
        {
            var collection = new OptimizedObservableCollection<int>();

            Assert.Empty(collection);
        }

        [Fact]
        public void AddRange_NullItems_DoesNothing()
        {
            var collection = new OptimizedObservableCollection<int>();

            collection.AddRange(null);

            Assert.Empty(collection);
        }

        [Fact]
        public void AddRange_EmptyItems_DoesNothing()
        {
            var collection = new OptimizedObservableCollection<int>();

            collection.AddRange(new List<int>());

            Assert.Empty(collection);
        }

        [Fact]
        public void AddRange_WithItems_AddsAllItems()
        {
            var collection = new OptimizedObservableCollection<int>();
            var items = new[] { 1, 2, 3, 4, 5 };

            collection.AddRange(items);

            Assert.Equal(5, collection.Count);
            Assert.Equal(items, collection);
        }

        [Fact]
        public void AddRange_FiresSingleNotification()
        {
            var collection = new OptimizedObservableCollection<int>();
            int collectionChangedCount = 0;
            int propertyChangedCount = 0;

            collection.CollectionChanged += (s, e) => collectionChangedCount++;
            ((INotifyPropertyChanged)collection).PropertyChanged += (s, e) => propertyChangedCount++;

            collection.AddRange(new[] { 1, 2, 3, 4, 5 });

            Assert.Equal(1, collectionChangedCount);
            Assert.Equal(2, propertyChangedCount);
        }

        [Fact]
        public void AddRange_ToExistingCollection_AppendsItems()
        {
            var collection = new OptimizedObservableCollection<int> { 1, 2, 3 };

            collection.AddRange(new[] { 4, 5, 6 });

            Assert.Equal(6, collection.Count);
            Assert.Equal(new[] { 1, 2, 3, 4, 5, 6 }, collection);
        }

        [Fact]
        public void ReplaceAll_NullItems_DoesNothing()
        {
            var collection = new OptimizedObservableCollection<int> { 1, 2, 3 };

            collection.ReplaceAll(null);

            Assert.Equal(3, collection.Count);
        }

        [Fact]
        public void ReplaceAll_EmptyItems_ClearsCollection()
        {
            var collection = new OptimizedObservableCollection<int> { 1, 2, 3 };

            collection.ReplaceAll(new List<int>());

            Assert.Empty(collection);
        }

        [Fact]
        public void ReplaceAll_WithItems_ReplacesAllItems()
        {
            var collection = new OptimizedObservableCollection<int> { 1, 2, 3 };
            var newItems = new[] { 10, 20, 30, 40 };

            collection.ReplaceAll(newItems);

            Assert.Equal(4, collection.Count);
            Assert.Equal(newItems, collection);
        }

        [Fact]
        public void ReplaceAll_FiresSingleNotification()
        {
            var collection = new OptimizedObservableCollection<int> { 1, 2, 3 };
            int collectionChangedCount = 0;
            int propertyChangedCount = 0;

            collection.CollectionChanged += (s, e) => collectionChangedCount++;
            ((INotifyPropertyChanged)collection).PropertyChanged += (s, e) => propertyChangedCount++;

            collection.ReplaceAll(new[] { 10, 20 });

            Assert.Equal(1, collectionChangedCount);
            Assert.Equal(2, propertyChangedCount);
        }

        [Fact]
        public void Clear_EmptyCollection_DoesNothing()
        {
            var collection = new OptimizedObservableCollection<int>();

            collection.Clear();

            Assert.Empty(collection);
        }

        [Fact]
        public void Clear_WithItems_RemovesAllItems()
        {
            var collection = new OptimizedObservableCollection<int> { 1, 2, 3, 4, 5 };

            collection.Clear();

            Assert.Empty(collection);
        }

        [Fact]
        public void Clear_FiresSingleNotification()
        {
            var collection = new OptimizedObservableCollection<int> { 1, 2, 3 };
            int collectionChangedCount = 0;
            int propertyChangedCount = 0;

            collection.CollectionChanged += (s, e) => collectionChangedCount++;
            ((INotifyPropertyChanged)collection).PropertyChanged += (s, e) => propertyChangedCount++;

            collection.Clear();

            Assert.Equal(1, collectionChangedCount);
            Assert.Equal(2, propertyChangedCount);
        }

        [Fact]
        public void Add_SingleItem_FiresNormalNotification()
        {
            var collection = new OptimizedObservableCollection<int>();
            int collectionChangedCount = 0;

            collection.CollectionChanged += (s, e) =>
            {
                collectionChangedCount++;
                Assert.Equal(NotifyCollectionChangedAction.Add, e.Action);
            };

            collection.Add(1);

            Assert.Equal(1, collectionChangedCount);
            Assert.Single(collection);
        }

        [Fact]
        public void Remove_SingleItem_FiresNormalNotification()
        {
            var collection = new OptimizedObservableCollection<int> { 1, 2, 3 };
            int collectionChangedCount = 0;

            collection.CollectionChanged += (s, e) =>
            {
                collectionChangedCount++;
                Assert.Equal(NotifyCollectionChangedAction.Remove, e.Action);
            };

            collection.Remove(2);

            Assert.Equal(1, collectionChangedCount);
            Assert.Equal(2, collection.Count);
        }

        [Fact]
        public void AddRange_LargeCollection_PerformanceTest()
        {
            var collection = new OptimizedObservableCollection<int>();
            var items = Enumerable.Range(1, 10000);

            var startTime = DateTime.UtcNow;
            collection.AddRange(items);
            var duration = DateTime.UtcNow - startTime;

            Assert.Equal(10000, collection.Count);
            Assert.True(duration.TotalSeconds < 1, "AddRange should complete within 1 second");
        }

        [Fact]
        public void ReplaceAll_PreservesOrder()
        {
            var collection = new OptimizedObservableCollection<int>();
            var items = new[] { 5, 3, 8, 1, 9, 2 };

            collection.ReplaceAll(items);

            Assert.Equal(items, collection.ToArray());
        }

        [Fact]
        public void AddRange_PreservesOrder()
        {
            var collection = new OptimizedObservableCollection<int> { 1, 2 };
            var items = new[] { 5, 3, 8 };

            collection.AddRange(items);

            Assert.Equal(new[] { 1, 2, 5, 3, 8 }, collection.ToArray());
        }

        [Fact]
        public void CollectionChanged_Reset_ActionForBulkOperations()
        {
            var collection = new OptimizedObservableCollection<int>();
            NotifyCollectionChangedAction? capturedAction = null;

            collection.CollectionChanged += (s, e) => capturedAction = e.Action;

            collection.AddRange(new[] { 1, 2, 3 });

            Assert.Equal(NotifyCollectionChangedAction.Reset, capturedAction);
        }

        [Fact]
        public void PropertyChanged_CountAndIndexer_ForBulkOperations()
        {
            var collection = new OptimizedObservableCollection<int>();
            var changedProperties = new List<string>();

            ((INotifyPropertyChanged)collection).PropertyChanged += (s, e) =>
            {
                changedProperties.Add(e.PropertyName);
            };

            collection.AddRange(new[] { 1, 2, 3 });

            Assert.Contains("Count", changedProperties);
            Assert.Contains("Item[]", changedProperties);
        }

        [Fact]
        public void AddRange_WithEnumerableNotList_HandlesCorrectly()
        {
            var collection = new OptimizedObservableCollection<int>();
            var items = Enumerable.Range(1, 5);

            collection.AddRange(items);

            Assert.Equal(5, collection.Count);
            Assert.Equal(new[] { 1, 2, 3, 4, 5 }, collection);
        }

        [Fact]
        public void ReplaceAll_WithEnumerableNotList_HandlesCorrectly()
        {
            var collection = new OptimizedObservableCollection<int> { 1, 2, 3 };
            var items = Enumerable.Range(10, 5);

            collection.ReplaceAll(items);

            Assert.Equal(5, collection.Count);
            Assert.Equal(new[] { 10, 11, 12, 13, 14 }, collection);
        }
    }
}

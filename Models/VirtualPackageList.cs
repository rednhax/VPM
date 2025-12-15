using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace VPM.Models
{
    /// <summary>
    /// A virtualizing list that creates PackageItems on demand to reduce memory usage.
    /// Instead of storing 20k PackageItem objects, it stores 20k string keys and creates/caches items as needed.
    /// </summary>
    public class VirtualPackageList : IList<PackageItem>, IList, INotifyCollectionChanged, INotifyPropertyChanged
    {
        private readonly List<string> _keys = new List<string>();
        private readonly Func<string, PackageItem> _itemFactory;
        // Use a dictionary for caching. WeakReference allows GC to collect unused items.
        // Key is the index in the list.
        private readonly Dictionary<int, WeakReference<PackageItem>> _cache = new Dictionary<int, WeakReference<PackageItem>>();
        
        // Keep a strong reference to recently used items to prevent flickering/recreation during scrolling
        private readonly LinkedList<PackageItem> _lruCache = new LinkedList<PackageItem>();
        private const int LRU_CAPACITY = 200;

        public VirtualPackageList(Func<string, PackageItem> itemFactory)
        {
            _itemFactory = itemFactory ?? throw new ArgumentNullException(nameof(itemFactory));
        }

        public void SetKeys(IEnumerable<string> keys)
        {
            _keys.Clear();
            if (keys != null)
            {
                _keys.AddRange(keys);
            }
            _cache.Clear();
            _lruCache.Clear();
            
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            OnPropertyChanged(nameof(Count));
            OnPropertyChanged("Item[]");
        }

        public PackageItem this[int index]
        {
            get
            {
                if (index < 0 || index >= _keys.Count) return null;

                PackageItem item = null;
                if (_cache.TryGetValue(index, out var weakRef))
                {
                    weakRef.TryGetTarget(out item);
                }

                if (item == null)
                {
                    var key = _keys[index];
                    item = _itemFactory(key);
                    if (item != null)
                    {
                        _cache[index] = new WeakReference<PackageItem>(item);
                    }
                }

                if (item != null)
                {
                    // Update LRU
                    if (_lruCache.Count >= LRU_CAPACITY)
                    {
                        _lruCache.RemoveLast();
                    }
                    _lruCache.AddFirst(item);
                }

                return item;
            }
            set => throw new NotSupportedException("Setting items by index is not supported.");
        }

        object IList.this[int index]
        {
            get => this[index];
            set => throw new NotSupportedException();
        }

        public int Count => _keys.Count;
        public bool IsReadOnly => false;
        public bool IsFixedSize => false;
        public object SyncRoot => ((ICollection)_keys).SyncRoot;
        public bool IsSynchronized => ((ICollection)_keys).IsSynchronized;

        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnCollectionChanged(NotifyCollectionChangedEventArgs e) => CollectionChanged?.Invoke(this, e);
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public void Add(PackageItem item)
        {
            if (item == null) return;
            _keys.Add(item.MetadataKey);
            int index = _keys.Count - 1;
            _cache[index] = new WeakReference<PackageItem>(item);
            
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
            OnPropertyChanged(nameof(Count));
            OnPropertyChanged("Item[]");
        }

        public int Add(object value)
        {
            Add((PackageItem)value);
            return Count - 1;
        }

        public void Clear()
        {
            _keys.Clear();
            _cache.Clear();
            _lruCache.Clear();
            
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            OnPropertyChanged(nameof(Count));
            OnPropertyChanged("Item[]");
        }

        public bool Contains(PackageItem item)
        {
            if (item == null) return false;
            return _keys.Contains(item.MetadataKey);
        }

        public bool Contains(object value) => Contains(value as PackageItem);

        public void CopyTo(PackageItem[] array, int arrayIndex)
        {
            for (int i = 0; i < Count; i++)
            {
                array[arrayIndex + i] = this[i];
            }
        }

        public void CopyTo(Array array, int index)
        {
            for (int i = 0; i < Count; i++)
            {
                array.SetValue(this[i], index + i);
            }
        }

        public IEnumerator<PackageItem> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int IndexOf(PackageItem item)
        {
            if (item == null) return -1;
            // This is slow (O(n)), but unavoidable if we only have the item
            // We assume item.MetadataKey is available
            return _keys.IndexOf(item.MetadataKey);
        }

        public int IndexOf(object value) => IndexOf(value as PackageItem);

        public void Insert(int index, PackageItem item)
        {
            if (item == null) return;
            _keys.Insert(index, item.MetadataKey);
            
            // Shift cache keys? No, cache is invalid now for indices >= index
            // Clearing cache is safer/easier, or we could shift keys
            _cache.Clear(); 
            
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
            OnPropertyChanged(nameof(Count));
            OnPropertyChanged("Item[]");
        }

        public void Insert(int index, object value) => Insert(index, (PackageItem)value);

        public bool Remove(PackageItem item)
        {
            if (item == null) return false;
            int index = IndexOf(item);
            if (index >= 0)
            {
                RemoveAt(index);
                return true;
            }
            return false;
        }

        public void Remove(object value) => Remove(value as PackageItem);

        public void RemoveAt(int index)
        {
            if (index < 0 || index >= Count) return;
            
            var item = this[index]; // Get item for notification
            _keys.RemoveAt(index);
            _cache.Clear(); // Invalidate cache
            
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
            OnPropertyChanged(nameof(Count));
            OnPropertyChanged("Item[]");
        }
    }
}

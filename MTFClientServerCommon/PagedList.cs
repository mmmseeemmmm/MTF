using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace MTFClientServerCommon
{

    public abstract class PagedList<T> : IList<T>, IList, INotifyCollectionChanged 
        where T : class
    {
        private int pageSize;
        private Dictionary<int, T> data = new Dictionary<int, T>();
        private int count;
        private int lastLoadedPageNumber = -1;
        private object dataOperationLock = new object();

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public PagedList(int pageSize)
        {
            this.pageSize = pageSize;
        }

        public int IndexOf(T item)
        {
            if (data.Values.Any(d => d == item))
            {
                return data.FirstOrDefault(k => k.Value == item).Key;
            }

            return -1;
        }

        public void Insert(int index, T item)
        {
            //invalid index raise ArgumentOutOfRangeException
            if (index > count || index < 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            lock (dataOperationLock)
            {
                if (index != count)
                {
                    var itemsToUpdate = data.Keys.Where(k => k >= index).OrderByDescending(k => k);
                    foreach (int indx in itemsToUpdate)
                    {
                        data[indx + 1] = data[indx];
                        data.Remove(indx);
                    }
                }

                data[index] = item;
                count++;
            }
        }

        public void RemoveAt(int index)
        {
            object removedItem = null;
            //invalid index raise ArgumentOutOfRangeException
            if (index > count || index < 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (data.ContainsKey(index))
            {
                var itemsToUpdate = data.Keys.Where(k => k >= index && k < count - 1).OrderBy(k => k);
                removedItem = data[index];
                foreach (int indx in itemsToUpdate)
                {
                    data[indx] = data[indx + 1];
                }
                count--;
                data.Remove(count);
            }
            else
            {
                //data isn't loaded
                throw new Exception("Item to delete is in page witch isn't loaded -> deleting these items isn't possible");
            }
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removedItem, index));
        }

        public T this[int index]
        {
            get
            {
                lock (dataOperationLock)
                {
                    if (data.ContainsKey(index))
                    {
                        return data[index];
                    }

                    //data isn't loaded, load it and return
                    loadInternalData(index / pageSize);

                    return data[index];
                }
            }
            set
            {
                //invalid index raise ArgumentOutOfRangeException
                if (index > count || index < 0)
                {
                    throw new ArgumentOutOfRangeException();
                }

                if (data.ContainsKey(index))
                {
                    data[index] = value;
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, value, index));
                }
            }
        }

        public void Add(T item)
        {
            Insert(count, item);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, count - 1));
            cleanInternalData();
        }

        public void Clear()
        {
            data.Clear();
            count = 0;
            ClearSavedData();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public bool Contains(T item)
        {
            return IndexOf(item) > 0;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public int Count
        {
            get { return count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(T item)
        {
            var indx = IndexOf(item);
            if (indx < 0)
            {
                return false;
            }
            RemoveAt(indx);

            return true;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new PagedListEnumerator<T>(this);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return new PagedListEnumerator<T>(this);
        }

        public int Add(object value)
        {
            Add((T)value);
            return count - 1;
        }

        public bool Contains(object value)
        {
            return Contains((T)value);
        }

        public int IndexOf(object value)
        {
            if (value == null)
            {
                return -1;
            }
            return IndexOf((T)value);
        }

        public void Insert(int index, object value)
        {
            Insert(index, (T)value);
        }

        public bool IsFixedSize
        {
            get { return false; }
        }

        public void Remove(object value)
        {
            Remove((T)value);
        }

        object IList.this[int index]
        {
            get
            {
                return this[index];
            }
            set
            {
                this[index] = (T)value;
            }
        }

        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        public bool IsSynchronized
        {
            get { return false; }
        }

        public object SyncRoot
        {
            get { return this; }
        }

        protected abstract void SaveData(int pageNumber, T[] data);

        protected abstract T[] LoadData(int pageNumber);

        protected abstract void ClearSavedData();

        protected void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (CollectionChanged != null)
            {
                CollectionChanged(this, e);
            }
        }

        protected bool IsPageLoaded(int pageNumber)
        {
            return data.ContainsKey(pageNumber * pageSize);
        }

        protected T[] GetPageDataOnlyIfAvailable(int pageNumber)
        {
            return data.Where(x => x.Key >= pageSize * pageNumber && x.Key < (pageSize * pageNumber) + pageSize).OrderBy(o => o.Key).Select(x => x.Value).ToArray();
        }

        protected void SetCount(int count)
        {
            this.count = count;
        }

        protected int PageSize => pageSize;

        private void cleanInternalData()
        {
            int dontCleanPageNumber = lastLoadedPageNumber;
            if (data.Count >= 3 * pageSize)
            {
                int minIndex = data.Keys.Min();
                if (dontCleanPageNumber != -1)
                {
                    var pagesAfter = data.Keys.Where(k => k > (dontCleanPageNumber + 1) * pageSize);
                    if (pagesAfter.Count() > 0)
                    {
                        minIndex = pagesAfter.Min();
                    }
                }
                int pageNumber = minIndex / pageSize;

                var dataPairs = data.Where(x => x.Key >= minIndex && x.Key < minIndex + pageSize).OrderBy(x => x.Key);
                var indexes = dataPairs.Select(x => x.Key);
                SaveData(pageNumber, dataPairs.Select(x => x.Value).ToArray());
                foreach (int i in indexes)
                {
                    data.Remove(i);
                }
            }
        }

        private void loadInternalData(int pageNumber)
        {
            var loadedData = LoadData(pageNumber);
            int i = 0;
            foreach (var d in loadedData)
            {
                data[(pageNumber * pageSize) + i] = d;
                i++;
            }
            lastLoadedPageNumber = pageNumber;

            cleanInternalData();
        }
    }


    public class PagedListEnumerator<T> : IEnumerator<T>, IEnumerator
        where T : class
    {
        private int currentIndex = -1;
        private int count;
        private PagedList<T> list;

        public PagedListEnumerator(PagedList<T> list)
        {
            this.count = list.Count;
            this.list = list;
        }

        public T Current
        {
            get { return list[currentIndex]; }
        }

        public void Dispose()
        {
        }

        object IEnumerator.Current
        {
            get { return Current; }
        }

        public bool MoveNext()
        {
            currentIndex++;
            return currentIndex < count;
        }

        public void Reset()
        {
            currentIndex = -1;
        }
    }
}

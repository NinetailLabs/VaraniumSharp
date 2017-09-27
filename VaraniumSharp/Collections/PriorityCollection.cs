using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using VaraniumSharp.Interfaces.Collections;

namespace VaraniumSharp.Collections
{
    /// <summary>
    ///     Collection that enforces and monitors priority of items that are added to the collection
    /// </summary>
    /// <typeparam name="T">Type of object added to the collection</typeparam>
    public class PriorityCollection<T> : IProducerConsumerCollection<T> where T : IPriorityItem
    {
        #region Properties

        /// <summary>
        ///     Number of items in the collection
        /// </summary>
        public int Count
        {
            get
            {
                lock (SyncRoot)
                {
                    return _items.Count;
                }
            }
        }

        /// <summary>
        ///     Indicate if the collection is Synchronized (thread-safe)
        /// </summary>
        public bool IsSynchronized => true;

        /// <summary>
        ///     Object used to synchronize access to the collection
        /// </summary>
        public object SyncRoot { get; } = new object();

        #endregion

        #region Public Methods

        /// <summary>
        ///     Copy the collection to an <see cref="Array" />
        /// </summary>
        /// <exception cref="ArgumentException">Target array is smaller than the number of items to inser</exception>
        /// <exception cref="ArgumentOutOfRangeException">The index is below the lower bound of the array</exception>
        /// <param name="array">Array that the collection should be copied to</param>
        /// <param name="index">Index to start inserting at in the target array</param>
        public void CopyTo(Array array, int index)
        {
            lock (SyncRoot)
            {
                if (array.Length - index < _items.Count)
                {
                    throw new ArgumentException("Target array is too small to copy items");
                }

                if (index < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(index), index, "Index is outside the bound of the array");
                }

                for (var r = 0; r < _items.Count; r++)
                {
                    array.SetValue(_items[r], r + index);
                }
            }
        }

        /// <summary>
        ///     Copy the collection to an array
        /// </summary>
        /// <param name="array">The array to copy the values to</param>
        /// <param name="index">The index to start inserting at</param>
        public void CopyTo(T[] array, int index)
        {
            CopyTo((Array)array, index);
        }

        /// <summary>
        ///     Get typed enumerator for the collection
        /// </summary>
        /// <returns>Enumerator for the collection</returns>
        public IEnumerator<T> GetEnumerator()
        {
            lock (SyncRoot)
            {
                return _items.GetEnumerator();
            }
        }

        /// <summary>
        ///     Convert the collection to a shallow copied array
        /// </summary>
        /// <returns>Array containing references to the items</returns>
        public T[] ToArray()
        {
            lock (SyncRoot)
            {
                return _items.ToArray();
            }
        }

        /// <summary>
        ///     Try to add an item to the collection.
        ///     If the item has a null priority it will be added to the end of the collection and a priority value will be assigned
        ///     to it.
        /// </summary>
        /// <param name="item">The item to add</param>
        /// <returns>True means the item has been successfully added</returns>
        public bool TryAdd(T item)
        {
            lock (SyncRoot)
            {
                if (item.Priority == null)
                {
                    item.Priority = _items.LastOrDefault()?.Priority ?? 0;
                }

                var priorityIndex = _items.FindLastIndex(x => x.Priority <= item.Priority);
                _items.Insert(priorityIndex + 1, item);
                item.PriorityChanged += ItemOnPriorityChanged;
                return true;
            }
        }

        /// <summary>
        ///     Try to take an item from the collection.
        ///     <remarks>The item with the highest priority (lowest int value) will always be taken</remarks>
        /// </summary>
        /// <param name="item">Out parameter the item will be assigned to</param>
        /// <returns>True if an item could be found in the array</returns>
        public bool TryTake(out T item)
        {
            lock (SyncRoot)
            {
                item = _items.FirstOrDefault();
                if (item != null)
                {
                    _items.Remove(item);
                    item.PriorityChanged -= ItemOnPriorityChanged;
                }
                return item != null;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Get enumerator for the collection
        /// </summary>
        /// <returns>Enumerator for the collection</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            lock (SyncRoot)
            {
                return GetEnumerator();
            }
        }

        /// <summary>
        ///     Occurs when a collection item's priority has been updated which require rearrangement of the list
        /// </summary>
        /// <param name="sender">Item with changed priority</param>
        /// <param name="args">This will be empty</param>
        private void ItemOnPriorityChanged(object sender, EventArgs args)
        {
            lock (SyncRoot)
            {
                var typedSender = (T)sender;

                _items.Remove(typedSender);
                var priorityIndex = _items.FindLastIndex(x => x.Priority <= typedSender.Priority);
                _items.Insert(priorityIndex + 1, typedSender);
            }
        }

        #endregion

        #region Variables

        /// <summary>
        ///     Collection that is used as the base for storing items we manage
        /// </summary>
        private readonly List<T> _items = new List<T>();

        #endregion
    }
}
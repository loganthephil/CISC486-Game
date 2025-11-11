using System;
using System.Collections;
using System.Collections.Generic;

namespace DroneStrikers.Core
{
    public class DropOutStack<T> : IReadOnlyCollection<T>, ICollection
    {
        /// <summary>
        ///     The number of items currently in the stack.
        /// </summary>
        public int Count { get; private set; }
        public bool IsSynchronized { get; } // Always false because I don't care about thread safety
        public object SyncRoot { get; }

        private T[] _items;
        private int _top;

        /// <summary>
        ///     Represents a variable size last-in-first-out (LIFO) collection of instances of the same specified type with a fixed capacity.
        ///     If the stack exceeds its capacity, the oldest item is "dropped out" to make room for the new item.
        /// </summary>
        /// <param name="capacity"> The maximum number of items the stack can hold.</param>
        public DropOutStack(int capacity)
        {
            _items = new T[capacity];
            Count = 0;
            IsSynchronized = false;
            SyncRoot = new object();
        }

        /// <summary>
        ///     Removes all items from the stack.
        /// </summary>
        public void Clear()
        {
            _items = new T[_items.Length];
            Count = 0;
            _top = 0;
        }

        /// <summary>
        ///     Push a new item onto the stack. If the stack is at capacity, the oldest item is removed.
        /// </summary>
        /// <param name="item"> The item to push onto the stack.</param>
        public void Push(T item)
        {
            _items[_top] = item;
            _top = (_top + 1) % _items.Length; // Wrap around if we reach the end
            if (Count < _items.Length) Count++; // Only increase count if we haven't reached capacity
        }

        /// <summary>
        ///     Removes an item from the top of the stack and returns it.
        /// </summary>
        /// <returns> The item removed from the top of the stack.</returns>
        /// <exception cref="InvalidOperationException"> Thrown if the stack is empty.</exception>
        public T Pop()
        {
            if (Count == 0) throw new InvalidOperationException("Stack is empty.");

            _top = (_top - 1 + _items.Length) % _items.Length; // Move top back, wrapping around if necessary
            T item = _items[_top];
            _items[_top] = default; // Clear the reference
            Count--;
            return item;
        }

        /// <summary>
        ///     Returns the item at the top of the stack without removing it.
        /// </summary>
        /// <returns> The item at the top of the stack.</returns>
        /// <exception cref="InvalidOperationException"> Thrown if the stack is empty.</exception>
        public T Peek()
        {
            if (Count == 0) throw new InvalidOperationException("Stack is empty.");
            return _items[(_top - 1 + _items.Length) % _items.Length];
        }

        /// <summary>
        ///     Attempts to pop an item from the stack. Returns true if successful, false if the stack is empty.
        ///     The popped item is returned via the out parameter.
        /// </summary>
        /// <param name="item"> The item popped from the stack, or default if the stack is empty.</param>
        /// <returns> True if an item was popped, false if the stack was empty.</returns>
        public bool TryPop(out T item)
        {
            if (Count == 0)
            {
                item = default;
                return false;
            }

            item = Pop();
            return true;
        }

        /// <summary>
        ///     Attempts to peek at the item at the top of the stack without removing it.
        ///     The peeked item is returned via the out parameter.
        /// </summary>
        /// <param name="item"> The item at the top of the stack, or default if the stack is empty.</param>
        /// <returns> True if an item was peeked, false if the stack was empty.</returns>
        public bool TryPeek(out T item)
        {
            if (Count == 0)
            {
                item = default;
                return false;
            }

            item = Peek();
            return true;
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
            {
                int index = (_top - 1 - i + _items.Length) % _items.Length;
                yield return _items[index];
            }
        }

        // Get an enumerator that iterates over the stack from top to bottom
        public IEnumerator GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
            {
                int index = (_top - 1 - i + _items.Length) % _items.Length;
                yield return _items[index];
            }
        }

        public void CopyTo(Array array, int index)
        {
            if (array == null) throw new ArgumentNullException(nameof(array));
            if (array.Rank != 1) throw new ArgumentException("Array must be one-dimensional.", nameof(array));
            if (index < 0) throw new ArgumentOutOfRangeException(nameof(index), "Index must be non-negative.");
            if (array.Length - index < Count) throw new ArgumentException("The destination array has insufficient space.");

            for (int i = 0; i < Count; i++)
            {
                int sourceIndex = (_top - 1 - i + _items.Length) % _items.Length;
                array.SetValue(_items[sourceIndex], index + i);
            }
        }
    }
}
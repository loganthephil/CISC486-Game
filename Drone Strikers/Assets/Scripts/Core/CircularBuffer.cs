using System;
using System.Collections;
using System.Collections.Generic;

namespace DroneStrikers.Core
{
    public class CircularBuffer<T> : IReadOnlyCollection<T>, ICollection
    {
        public int Count { get; private set; }
        public bool IsSynchronized { get; }
        public object SyncRoot { get; }

        private readonly T[] _buffer;
        private int _start;
        private int _end;

        public CircularBuffer(int capacity) : this(capacity, new T[] { }) { }

        public CircularBuffer(int capacity, T[] items)
        {
            if (capacity <= 0) throw new ArgumentException("Cannot create CircularBuffer with negative or zero capacity.");
            if (items == null) throw new ArgumentNullException(nameof(items));
            if (items.Length > capacity) throw new ArgumentException("Initial items exceed buffer capacity.");

            _buffer = new T[capacity];

            Array.Copy(items, _buffer, items.Length);
            Count = items.Length;

            _start = 0;
            _end = Count == capacity ? 0 : Count;

            IsSynchronized = false;
            SyncRoot = new object();
        }

        /// <summary>
        ///     The maximum capacity of the circular buffer.
        /// </summary>
        public int Capacity => _buffer.Length;

        public bool IsFull => Count == Capacity;
        public bool IsEmpty => Count == 0;

        /// <summary>
        ///     The item at the front of the circular buffer.
        /// </summary>
        /// <returns>The item at the front of the buffer.</returns>
        public T Front()
        {
            ThrowIfEmpty();
            return _buffer[_start];
        }

        /// <summary>
        ///     The item at the back of the circular buffer.
        /// </summary>
        /// <returns> The item at the back of the buffer.</returns>
        public T Back()
        {
            ThrowIfEmpty();
            return _buffer[(_end != 0 ? _end : Capacity) - 1];
        }

        /// <summary>
        ///     Index access to the circular buffer.
        ///     Does not loop around when adding items.
        ///     Valid indices are 0 to Count - 1.
        /// </summary>
        /// <param name="index"> The index of the item to access.</param>
        /// <returns> The item at the specified index.</returns>
        /// <exception cref="IndexOutOfRangeException">Thrown when the index is out of range.</exception>
        public T this[int index]
        {
            get
            {
                if (IsEmpty || index >= Count) throw new IndexOutOfRangeException();
                return _buffer[InternalIndex(index)];
            }
            set
            {
                if (IsEmpty || index >= Count) throw new IndexOutOfRangeException();
                _buffer[InternalIndex(index)] = value;
            }
        }

        /// <summary>
        ///     Pushes an item to the back of the circular buffer.
        /// </summary>
        /// <param name="item"> The item to push to the back of the buffer.</param>
        public void PushBack(T item)
        {
            if (IsFull)
            {
                _buffer[_end] = item;
                Increment(ref _end);
                _start = _end;
            }
            else
            {
                _buffer[_end] = item;
                Increment(ref _end);
                Count++;
            }
        }

        /// <summary>
        ///     Pushes an item to the front of the circular buffer.
        /// </summary>
        /// <param name="item"> The item to push to the front of the buffer.</param>
        public void PushFront(T item)
        {
            if (IsFull)
            {
                Decrement(ref _start);
                _end = _start;
                _buffer[_start] = item;
            }
            else
            {
                Decrement(ref _start);
                _buffer[_start] = item;
                Count++;
            }
        }

        /// <summary>
        ///     Removes and returns the item at the back of the circular buffer.
        /// </summary>
        /// <returns> The item removed from the back of the buffer.</returns>
        public T PopBack()
        {
            T item = Back();
            Decrement(ref _end);
            _buffer[_end] = default;
            Count--;
            return item;
        }


        /// <summary>
        ///     Removes and returns the item at the front of the circular buffer.
        /// </summary>
        /// <returns> The item removed from the front of the buffer.</returns>
        public T PopFront()
        {
            T item = Front();
            _buffer[_start] = default;
            Increment(ref _start);
            Count--;
            return item;
        }

        /// <summary>
        ///     Clears the contents of the circular buffer.
        /// </summary>
        public void Clear()
        {
            _start = 0;
            _end = 0;
            Count = 0;
            Array.Clear(_buffer, 0, _buffer.Length);
        }

        public void CopyTo(Array array, int index)
        {
            if (array == null) throw new ArgumentNullException(nameof(array));
            if (array.Rank != 1) throw new ArgumentException("Array must be one-dimensional.");
            if (index < 0) throw new ArgumentOutOfRangeException(nameof(index), "Index cannot be negative.");
            if (array.Length - index < Count) throw new ArgumentException("The destination array has insufficient space.");

            IList<ArraySegment<T>> segments = ToArraySegments();

            foreach (ArraySegment<T> segment in segments)
            {
                Array.Copy(segment.Array ?? Array.Empty<T>(), segment.Offset, array, index, segment.Count);
                index += segment.Count;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            IList<ArraySegment<T>> segments = ToArraySegments();

            foreach (ArraySegment<T> segment in segments)
            {
                for (int i = 0; i < segment.Count; i++)
                {
                    if (segment.Array != null) yield return segment.Array[segment.Offset + i];
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IList<ArraySegment<T>> ToArraySegments()
        {
            return new[] { ArrayOne(), ArrayTwo() };
        }

        private void Increment(ref int index)
        {
            if (++index == Capacity) index = 0;
        }

        private void Decrement(ref int index)
        {
            if (index == 0) index = Capacity;
            index--;
        }

        private int InternalIndex(int index) => _start + (index < Capacity - _start ? index : index - Capacity);

        private void ThrowIfEmpty()
        {
            if (IsEmpty) throw new InvalidOperationException("CircularBuffer is empty.");
        }

        private ArraySegment<T> ArrayOne()
        {
            if (IsEmpty) return new ArraySegment<T>(new T[0]);

            if (_start < _end) return new ArraySegment<T>(_buffer, _start, _end - _start);

            return new ArraySegment<T>(_buffer, _start, _buffer.Length - _start);
        }

        private ArraySegment<T> ArrayTwo()
        {
            if (IsEmpty) return new ArraySegment<T>(new T[0]);

            if (_start < _end) return new ArraySegment<T>(_buffer, _end, 0);

            return new ArraySegment<T>(_buffer, 0, _end);
        }
    }
}
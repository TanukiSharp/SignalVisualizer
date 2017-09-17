using System;
using System.Collections.Generic;
using System.Threading;

namespace SignalVisualizer
{
    public class RingBuffer<T> : IEnumerable<T>
    {
        public int Length => buffer.Length;
        private int position;
        private T[] buffer;

        private int iterateLength;

        private readonly ReaderWriterLockSlim syncLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

        public RingBuffer(int length)
        {
            if (length <= 0)
                throw new ArgumentOutOfRangeException(nameof(length), length, $"Argument '{nameof(length)}' must be strictly greater then zero.");

            buffer = new T[length];
        }

        public void Clear()
        {
            syncLock.EnterWriteLock();

            try
            {
                Array.Clear(buffer, 0, buffer.Length);
                iterateLength = 0;
            }
            finally
            {
                syncLock.ExitWriteLock();
            }
        }

        public void Write(T data)
        {
            syncLock.EnterWriteLock();

            try
            {
                if (position < buffer.Length)
                {
                    buffer[position] = data;
                    position++;
                }
                else
                {
                    buffer[0] = data;
                    position = 1;
                }

                iterateLength++;

                if (iterateLength > buffer.Length)
                    iterateLength = buffer.Length;
            }
            finally
            {
                syncLock.ExitWriteLock();
            }
        }

        public void Write(T[] data)
        {
            Write(data, 0, data.Length);
        }

        public void Write(T[] data, int offset, int count)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (count < 0)
                throw new ArgumentOutOfRangeException($"Argument '{nameof(count)}' must be greater than or equal to zero.");
            if (offset < 0)
                throw new ArgumentOutOfRangeException($"Argument '{nameof(offset)}' must be greater than or equal to zero.");
            if (count + offset > data.Length)
                throw new ArgumentOutOfRangeException($"Argument '{nameof(offset)}' and '{nameof(count)}' make bigger capacity than '{nameof(data)}' array.");

            if (count == 0)
                return;

            syncLock.EnterWriteLock();

            try
            {
                WriteMultipleInternal(data, offset, count);
            }
            finally
            {
                syncLock.ExitWriteLock();
            }
        }

        private void WriteMultipleInternal(T[] data, int offset, int count)
        {
            int lengthToCopy = count;
            int localOffset = offset;

            if (count > buffer.Length)
            {
                lengthToCopy = buffer.Length;
                localOffset += count - lengthToCopy;
            }

            int writeLength = Math.Min(lengthToCopy, buffer.Length - position);
            Array.Copy(data, localOffset, buffer, position, writeLength);
            localOffset += writeLength;

            iterateLength += writeLength;
            if (iterateLength > buffer.Length)
                iterateLength = buffer.Length;

            position += writeLength;

            if (writeLength == lengthToCopy)
                return;

            int remainToCopy = lengthToCopy - writeLength;
            Array.Copy(data, localOffset, buffer, 0, remainToCopy);

            iterateLength += remainToCopy;
            if (iterateLength > buffer.Length)
                iterateLength = buffer.Length;

            position = remainToCopy;
        }

        public IEnumerator<T> GetEnumerator()
        {
            syncLock.EnterReadLock();

            try
            {
                int index = position;
                if (index >= buffer.Length)
                    index = 0;

                for (int iterationCount = 0; iterationCount < iterateLength; iterationCount++)
                {
                    yield return buffer[index++];
                    if (index >= buffer.Length)
                        index = 0;
                }
            }
            finally
            {
                syncLock.ExitReadLock();
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private int reverseEnumerateIterationCount = -1;
        private int reverseEnumerateIndex = -1;

        public void BeginReverseEnumerate()
        {
            syncLock.EnterReadLock();

            reverseEnumerateIndex = position - 1;
            if (reverseEnumerateIndex < 0)
                reverseEnumerateIndex = buffer.Length - 1;

            reverseEnumerateIterationCount = 0;
        }

        public bool GetReverseEnumerateValue(out T value)
        {
            if (reverseEnumerateIterationCount < iterateLength)
            {
                value = buffer[reverseEnumerateIndex];

                reverseEnumerateIndex--;
                if (reverseEnumerateIndex < 0)
                    reverseEnumerateIndex = buffer.Length - 1;

                reverseEnumerateIterationCount++;

                return true;
            }

            value = default(T);
            return false;
        }

        public void EndReverseEnumerate()
        {
            reverseEnumerateIterationCount = -1;
            reverseEnumerateIndex = -1;

            syncLock.ExitReadLock();
        }

        public IEnumerable<T> ReverseEnumerate()
        {
            syncLock.EnterReadLock();

            try
            {
                int index = position - 1;
                if (index < 0)
                    index = buffer.Length - 1;

                for (int iterationCount = 0; iterationCount < iterateLength/* - 1*/; iterationCount++)
                {
                    yield return buffer[index];
                    index--;
                    if (index < 0)
                        index = buffer.Length - 1;
                }
            }
            finally
            {
                syncLock.ExitReadLock();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentTerminal.App.Views.NativeFrontend.Utility
{
    internal class ArrayReader<T>
    {
        private readonly T[] _array;

        public int Offset { get; private set; }
        public int RemainingLength { get; private set; }

        public ArrayReader(T[] array)
        {
            _array = array;
            Offset = 0;
            RemainingLength = array.Length;
        }

        public ArrayReader(T[] array, int offset, int count)
        {
            _array = array;
            Offset = offset;
            RemainingLength = count;
        }

        public ArrayReader(ArraySegment<T> segment)
        {
            _array = segment.Array;
            Offset = segment.Offset;
            RemainingLength = segment.Count;
        }

        public bool TryRead(out T element)
        {
            if (RemainingLength > 0)
            {
                element = _array[Offset];
                Offset++;
                RemainingLength--;
                return true;
            }
            else
            {
                element = default(T);
                return false;
            }
        }

        public bool TryPeek(out T element)
        {
            if (RemainingLength > 0)
            {
                element = _array[Offset];
                return true;
            }
            else
            {
                element = default(T);
                return false;
            }
        }

        public T Read()
        {
            T element;
            if (!TryRead(out element))
            {
                throw new IndexOutOfRangeException();
            }
            return element;
        }

        public T Peek()
        {
            T element;
            if (!TryPeek(out element))
            {
                throw new IndexOutOfRangeException();
            }
            return element;
        }
    }
}

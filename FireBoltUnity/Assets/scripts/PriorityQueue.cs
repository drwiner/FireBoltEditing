using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.scripts
{
    //code stolen from Bardic...need to encapsulate and extract library of shared utils
    public class PriorityQueue<T>
    {
        private readonly List<T> heap;
        private readonly IComparer<T> comparer;

        private class Comparer : IComparer<T>
        {
            private readonly Func<T, T, int> comparer;

            public Comparer(Func<T, T, int> comparer)
            {
                this.comparer = comparer;
            }

            public int Compare(T x, T y)
            {
                return this.comparer(x, y);
            }
        }

        public int Count { get { return heap.Count; } }

        public bool Empty { get { return this.Count == 0; } }

        public PriorityQueue(IComparer<T> comparer)
        {
            this.comparer = comparer;
            this.heap = new List<T>();
        }

        public PriorityQueue(Func<T, T, int> comparer)
            : this(new Comparer(comparer))
        {
            // Nothing needed here
        }

        private int Compare(int left, int right)
        {
            return this.comparer.Compare(heap[left], heap[right]);
        }

        private void Swap(int i, int j)
        {
            T tmp = heap[i];
            heap[i] = heap[j];
            heap[j] = tmp;
        }

        public void Clear()
        {
            this.heap.Clear();
        }

        public void Add(T item)
        {
            heap.Add(item);
            // Recurse up heap until we know child is in right place
            for (int child = heap.Count - 1, parent = (child - 1) / 2;
                child > 0 && this.Compare(child, parent) < 0;
                child = parent, parent = (parent - 1) / 2)
            {
                this.Swap(child, parent);
            }
        }

        public T Peek()
        {
            return heap[0];
        }

        public T Pop()
        {
            int lasti = heap.Count - 1;
            T top = this.Peek();
            heap[0] = heap[lasti];
            heap.RemoveAt(lasti);
            --lasti;

            // Recurse down heap until element is in right place
            for (int parent = 0, child = parent * 2 + 1; child <= lasti; parent = child, child = child * 2 + 1)
            {
                int right = child + 1;
                if (right <= lasti && this.Compare(right, child) < 0)
                {
                    child = right;
                }
                if (this.Compare(parent, child) <= 0)
                {
                    break;
                }
                else
                {
                    this.Swap(child, parent);
                }
            }

            return top;
        }
    }
}

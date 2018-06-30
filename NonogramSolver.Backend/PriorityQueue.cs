using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NonogramSolver.Backend
{
    // A priority queue that is implemented as a min heap
    // For example, if 'TKey' is 'int', then dequeing all elements will output in ascending order
    // If two keys are identical, there is no guarantee about which order they will be retrieved
    public class PriorityQueue<TKey, TValue>
    {
        private class Node
        {
            public TKey key;
            public TValue value;
        }

        private readonly List<Node> tree;
        private readonly IComparer<TKey> keyComparer;

        private enum HeapifyCheck
        {
            None,
            Left,
            Right
        }

        public PriorityQueue()
        {
            tree = new List<Node>();
            keyComparer = Comparer<TKey>.Default;
        }

        public PriorityQueue(Comparison<TKey> comparer)
        {
            tree = new List<Node>();
            keyComparer = Comparer<TKey>.Create(comparer);
        }

        public PriorityQueue(IComparer<TKey> comparer)
        {
            tree = new List<Node>();
            keyComparer = comparer;
        }

        public int Count => tree.Count;

        public void Enqueue(TKey key, TValue value)
        {
            Node node = new Node();
            node.key = key;
            node.value = value;

            // Push the new node onto the end of the tree, then HeapifyUp to fix the tree
            int index = tree.Count;
            tree.Add(node);
            HeapifyUp(index);
        }

        public TValue Dequeue()
        {
            return DequeueWorker().value;
        }

        public TValue Dequeue(out TKey key)
        {
            Node node = DequeueWorker();
            key = node.key;
            return node.value;
        }

        private Node DequeueWorker()
        {
            if (tree.Count == 0)
                throw new InvalidOperationException("Cannot Dequeue: PriorityQueue is empty.");

            // We want to remove the first node in the tree, our invariant is that this is
            // always the minimum value. How we do this is:
            // 1. Swap the first / last element
            // 2. Pop the last element (which is our min value)
            // 3. HeapifyDown from the first element (previously last) to fix the tree
            
            int lastIndex = tree.Count - 1;

            Swap(0, lastIndex);

            Node minNode = tree[lastIndex];
            tree.RemoveAt(lastIndex);

            HeapifyDown();

            return minNode;
        }

        private void HeapifyDown()
        {
            if (tree.Count == 0)
                return;

            int current = 0;
            int left, right;
            while (true)
            {
                left = GetLeftChild(current);
                right = GetRightChild(current);

                HeapifyCheck checkResult = CheckHeapifyDown(current, left, right);
                if (checkResult == HeapifyCheck.None)
                {
                    break;
                }
                else if (checkResult == HeapifyCheck.Left)
                {
                    Swap(current, left);
                    current = left;
                }
                else
                {
                    Debug.Assert(checkResult == HeapifyCheck.Right);
                    Swap(current, right);
                    current = right;
                }
            }
        }

        private void HeapifyUp(int index)
        {
            Debug.Assert(index < tree.Count);

            int current = index;
            int parent;
            while (current != 0)
            {
                parent = GetParent(current);
                TKey currentKey = tree[current].key;
                TKey parentKey = tree[parent].key;

                // If current >= parent, no need to swap
                if (keyComparer.Compare(currentKey, parentKey) >= 0)
                    break;

                Swap(current, parent);
                current = parent;
            }
        }

        private int GetLeftChild(int current)
        {
            return current * 2 + 1;
        }

        private int GetRightChild(int current)
        {
            return current * 2 + 2;
        }

        private int GetParent(int current)
        {
            return (current - 1) / 2;
        }

        private void Swap(int first, int second)
        {
            if (first == second)
                return;

            Node a = tree[first];
            tree[first] = tree[second];
            tree[second] = a;
        }

        private HeapifyCheck CheckHeapifyDown(int current, int left, int right)
        {
            TKey currentKey = tree[current].key;
            bool currentVsLeft = false; // true if current > left
            bool currentVsRight = false; // true if current > right

            if (left < tree.Count)
            {
                TKey lKey = tree[left].key;
                currentVsLeft = keyComparer.Compare(currentKey, lKey) > 0;
            }

            if (right < tree.Count)
            {
                TKey rKey = tree[right].key;
                currentVsRight = keyComparer.Compare(currentKey, rKey) > 0;
            }

            if (currentVsLeft && currentVsRight)
            {
                // current > left, current > right
                // This is the hard case, we need to know if left > right

                TKey lKey = tree[left].key;
                TKey rKey = tree[right].key;
                return keyComparer.Compare(lKey, rKey) > 0 ? HeapifyCheck.Right : HeapifyCheck.Left;
            }
            else if (currentVsLeft)
            {
                // right > current > left
                return HeapifyCheck.Left;
            }
            else if (currentVsRight)
            {
                // left > current > right
                return HeapifyCheck.Right;
            }
            else
            {
                // current < left, current < right
                return HeapifyCheck.None;
            }
        }
    }
}

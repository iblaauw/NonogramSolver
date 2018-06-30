using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NonogramSolver.Backend;

namespace NonogramSolver.Tests
{
    [TestClass]
    class PriorityQueueTest
    {
        private class TestObj
        {
            public int Value { get; set; }
        }

        private void Push(PriorityQueue<int, TestObj> queue, int val)
        {
            queue.Enqueue(val, new TestObj() { Value = val });
        }

        private void Verify(PriorityQueue<int, TestObj> queue, int correct)
        {
            var obj = queue.Dequeue();
            Assert.True(obj.Value == correct);
        }

        private void VerifyAll(PriorityQueue<int, TestObj> queue, ICollection<int> answers)
        {
            Assert.True(queue.Count == answers.Count);
            foreach (int correct in answers)
            {
                Verify(queue, correct);
            }
            Assert.True(queue.Count == 0);
        }

        private void VerifyFull(params int[] values)
        {
            PriorityQueue<int, TestObj> queue = new PriorityQueue<int, TestObj>();
            foreach (int val in values)
            {
                Push(queue, val);
            }

            Assert.True(queue.Count == values.Length);

            Array.Sort(values);

            VerifyAll(queue, values);
        }

        [TestMethod]
        public void Basic()
        {
            VerifyFull(1, 3, 5, 2, 4);
        }

        [TestMethod]
        public void Basic2()
        {
            VerifyFull(0, 2, 4, 6, 8);
        }

        [TestMethod]
        public void BasicEnqueueDequeue()
        {
            PriorityQueue<int, TestObj> queue = new PriorityQueue<int, TestObj>();

            Assert.True(queue.Count == 0);
            queue.Enqueue(1, new TestObj() { Value = 2 });
            Assert.True(queue.Count == 1);

            var obj = queue.Dequeue(out int key);
            Assert.True(obj.Value == 2);
            Assert.True(key == 1);
            Assert.True(queue.Count == 0);
        }

        [TestMethod]
        public void ThrowOnEmpty()
        {
            PriorityQueue<int, TestObj> queue = new PriorityQueue<int, TestObj>();
            Push(queue, 1);
            Push(queue, -1);

            queue.Dequeue();
            queue.Dequeue();

            bool success = false;
            try
            {
                queue.Dequeue();
            }
            catch (Exception)
            {
                success = true;
            }

            Assert.True(success);
        }

        [TestMethod]
        public void Complex()
        {
            PriorityQueue<int, TestObj> queue = new PriorityQueue<int, TestObj>();
            Push(queue, 3);
            Push(queue, 0);
            Push(queue, 1);
            Push(queue, 4);

            Verify(queue, 0);
            Verify(queue, 1);

            Push(queue, 7);
            Push(queue, 2);
            Push(queue, 5);

            Verify(queue, 2);
            Verify(queue, 3);

            Push(queue, 6);

            VerifyAll(queue, new[] { 4, 5, 6, 7 });
        }

        [TestMethod]
        public void Reversed()
        {
            PriorityQueue<int, TestObj> queue = new PriorityQueue<int, TestObj>((a,b) => b - a);

            Push(queue, 17);
            Push(queue, 5);
            Push(queue, 9);
            Push(queue, 1);
            Push(queue, 2);
            Push(queue, 6);

            VerifyAll(queue, new[] { 17, 9, 6, 5, 2, 1 });
        }

        [TestMethod]
        public void DoubleTake()
        {
            PriorityQueue<int, TestObj> queue = new PriorityQueue<int, TestObj>();
            var correct = new[] { 0, 1, 2, 3, 4, 5 };

            Push(queue, 0);
            Push(queue, 3);
            Push(queue, 2);
            Push(queue, 1);
            Push(queue, 4);
            Push(queue, 5);

            VerifyAll(queue, correct);

            Push(queue, 5);
            Push(queue, 4);
            Push(queue, 3);
            Push(queue, 2);
            Push(queue, 1);
            Push(queue, 0);
            
            VerifyAll(queue, correct);
        }
    }
}

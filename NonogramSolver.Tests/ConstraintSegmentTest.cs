using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NonogramSolver.Backend;

namespace NonogramSolver.Tests
{
    [TestClass]
    class ConstraintSegmentTest
    {
        private class SegmentDescription
        {
            public SegmentDescription(int start, int end, uint color, uint number, bool isGap)
            {
                StartIndex = start;
                EndIndex = end;
                Color = color;
                Number = number;
                IsGap = isGap;
            }

            public int StartIndex { get; set; }
            public int EndIndex { get; set; }
            public uint Color { get; set; }
            public uint Number { get; set; }
            public bool IsGap { get; set; }

            public bool Match(ConstraintSegment segment)
            {
                return segment.StartIndex == StartIndex
                    && segment.EndIndex == EndIndex
                    && segment.Constraint.color == Color
                    && segment.Constraint.number == Number
                    && segment.IsGap == IsGap;
            }
        }

        [TestMethod]
        public void Basic()
        {
            List<Constraint> constraints = new List<Constraint>
            {
                new Constraint(1,1),
                new Constraint(2,2),
                new Constraint(3,3),
            };

            ConstraintSegment.CreateChain(constraints, out ConstraintSegment begin, out ConstraintSegment end);

            List<int> startIndices = new List<int> { 0, 1, 3 };
            List<int> endIndices = new List<int> { 1, 3, 6 };
            //List<bool> isGap = new List<bool> { false, false, false };

            int index = 0;
            ConstraintSegment current = begin;
            while (!Object.ReferenceEquals(current, end))
            {
                Assert.True(current.IsGap == false);
                Assert.True(current.StartIndex == startIndices[index]);
                Assert.True(current.EndIndex == endIndices[index]);
                Assert.True(current.Constraint == constraints[index]);

                index++;
                current = current.Next;
            }
        }

        [TestMethod]
        public void Gaps()
        {
            List<Constraint> constraints = new List<Constraint>
            {
                new Constraint(1,2),
                new Constraint(1,1),
                new Constraint(1,2),
            };

            List<SegmentDescription> descriptions = new List<SegmentDescription>
            {
                new SegmentDescription(0,2,1,2,false),
                new SegmentDescription(2,3,0,1,true),
                new SegmentDescription(3,4,1,1,false),
                new SegmentDescription(4,5,0,1,true),
                new SegmentDescription(5,7,1,2,false),
            };

            AssertMatches(constraints, descriptions);
        }

        [TestMethod]
        public void Mixed()
        {
            List<Constraint> constraints = new List<Constraint>
            {
                new Constraint(1,1),
                new Constraint(1,1),
                new Constraint(2,2),
                new Constraint(1,3),
                new Constraint(3,1),
            };

            List<SegmentDescription> descriptions = new List<SegmentDescription>
            {
                new SegmentDescription(0,1,1,1,false),
                new SegmentDescription(1,2,0,1,true),
                new SegmentDescription(2,3,1,1,false),
                new SegmentDescription(3,5,2,2,false),
                new SegmentDescription(5,8,1,3,false),
                new SegmentDescription(8,9,3,1,false),
            };

            AssertMatches(constraints, descriptions);
        }

        [TestMethod]
        public void SimpleBump()
        {
            List<Constraint> constraints = new List<Constraint>
            {
                new Constraint(1,1),
                new Constraint(2,1),
            };

            List<SegmentDescription> descriptions = new List<SegmentDescription>
            {
                new SegmentDescription(0,1,1,1,false),
                new SegmentDescription(1,2,0,1,true),
                new SegmentDescription(2,3,2,1,false),
            };

            ConstraintSegment.CreateChain(constraints, out ConstraintSegment begin, out ConstraintSegment end);
            bool success = end.Bump(7);
            Assert.True(success);
            AssertMatches(begin, descriptions);
        }

        [TestMethod]
        public void GappedBump()
        {
            List<Constraint> constraints = new List<Constraint>
            {
                new Constraint(1,1),
                new Constraint(1,1),
            };

            List<SegmentDescription> descriptions1 = new List<SegmentDescription>
            {
                new SegmentDescription(0,1,1,1,false),
                new SegmentDescription(1,3,0,2,true),
                new SegmentDescription(3,4,1,1,false),
            };

            ConstraintSegment.CreateChain(constraints, out ConstraintSegment begin, out ConstraintSegment end);

            bool success = end.Bump(4);
            Assert.True(success);
            AssertMatches(begin, descriptions1);
        }

        [TestMethod]
        public void GappedBump2()
        {
            // A continuation of GappedBump(), thist test just bumps twice

            List<Constraint> constraints = new List<Constraint>
            {
                new Constraint(1,1),
                new Constraint(1,1),
            };

            List<SegmentDescription> descriptions2 = new List<SegmentDescription>
            {
                new SegmentDescription(1,2,1,1,false),
                new SegmentDescription(2,3,0,1,true),
                new SegmentDescription(3,4,1,1,false),
            };

            ConstraintSegment.CreateChain(constraints, out ConstraintSegment begin, out ConstraintSegment end);

            end.Bump(4);

            bool success = end.Bump(4);
            Assert.True(success);
            AssertMatches(begin, descriptions2);

            success = end.Bump(4);
            Assert.False(success);
        }

        private void AssertMatches(IEnumerable<Constraint> constraints, IEnumerable<SegmentDescription> segments)
        {
            ConstraintSegment.CreateChain(constraints, out ConstraintSegment begin, out ConstraintSegment end);
            AssertMatches(begin, segments);
        }

        private void AssertMatches(ConstraintSegment begin, IEnumerable<SegmentDescription> segments)
        {
            var segmentSequence = ExtractChainSequence(begin);
            var matches = segmentSequence.Zip(segments, (c, d) => d.Match(c));
            Assert.True(matches.All(m => m));
        }

        private IEnumerable<ConstraintSegment> ExtractChainSequence(ConstraintSegment begin)
        {
            ConstraintSegment current = begin;
            while (current != null)
            {
                yield return current;
                current = current.Next;
            }
        }
    }
}

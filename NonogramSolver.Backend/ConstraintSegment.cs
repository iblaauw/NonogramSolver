using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace NonogramSolver.Backend
{
    public class ConstraintSegment
    {
        private ConstraintSegment(Constraint constraint, int startIndex)
        {
            Next = null;
            Prev = null;
            Constraint = constraint;
            StartIndex = startIndex;
        }

        public ConstraintSegment Next { get; private set; }
        public ConstraintSegment Prev { get; private set; }
        public Constraint Constraint { get; private set; }

        public int StartIndex { get; private set; }
        public int EndIndex => StartIndex + (int)Constraint.number;

        public bool IsGap => Constraint.color == ColorSpace.Empty;

        public static void CreateChain(IEnumerable<Constraint> constraints, out ConstraintSegment begin, out ConstraintSegment end)
        {
            begin = null;
            end = null;

            ConstraintSegment current = null;
            foreach (Constraint constr in constraints)
            {
                if (current == null)
                {
                    current = new ConstraintSegment(constr, 0);
                    begin = current;
                }
                else
                {
                    current = current.CreateNext(constr);
                }
            }

            end = current;
        }

        public ConstraintSegment CreateNext(Constraint constraint)
        {
            Debug.Assert(Next == null);

            if (constraint.color == this.Constraint.color)
            {
                // We need to create a constraint side-by-side with the same color -> introduce a gap
                Constraint newConstraint = new Constraint { color = ColorSpace.Empty, number = 1 };
                ConstraintSegment gapSegment = CreateNext(newConstraint);
                Debug.Assert(gapSegment.IsGap);
                return gapSegment.CreateNext(constraint);
            }

            var newSegment = new ConstraintSegment(constraint, EndIndex);
            InsertAfterSelf(newSegment);
            return newSegment;
        }

        public bool Bump(int maxIndex)
        {
            if (IsGap)
            {
                Debug.Assert(maxIndex == EndIndex);
                if (IsNecessaryGap())
                {
                    maxIndex = EndIndex - 1;
                }
                return Prev.Bump(maxIndex);
            }

            if (EndIndex >= maxIndex)
            {
                Debug.Assert(EndIndex == maxIndex, "Overlapping ConstraintSegments!");
                if (Prev == null)
                    return false; // Nothing before, and can't move current segment

                return Prev.Bump(StartIndex);
            }
            else //if (EndIndex < maxIndex)
            {
                StartIndex++;
                if (Next != null)
                {
                    Debug.Assert(Next.IsGap);
                    Next.ShrinkForBump();
                }

                if (Prev != null)
                {
                    Prev.GrowForBump();
                }

                ResetFolowing();
                return true;
            }
        }

        public void Validate(int maxIndex)
        {
            // TODO: use more than asserts...
            Debug.Assert(EndIndex <= maxIndex);
            if (Next != null)
            {
                Debug.Assert(Constraint.color != Next.Constraint.color);
                Debug.Assert(EndIndex == Next.StartIndex);
                Next.Validate(maxIndex);
            }
        }

        private void RemoveSelf()
        {
            if (Prev != null)
            {
                Prev.Next = Next;
            }

            if (Next != null)
            {
                Next.Prev = Prev;
            }
        }

        private void InsertAfterSelf(ConstraintSegment segment)
        {
            segment.Prev = this;

            // Order is important
            segment.Next = Next;
            if (Next != null)
            {
                Next.Prev = segment;
            }
            Next = segment;
        }

        private void ShrinkForBump()
        {
            // Our Prev has just moved up by 1
            Debug.Assert(IsGap);
            StartIndex++;
            var constr = Constraint;
            constr.number--;
            Constraint = constr;
            Debug.Assert(StartIndex == Prev.EndIndex);

            if (Constraint.number == 0)
            {
                Debug.Assert(!IsNecessaryGap(), "Necessary gap shrunk to 0!");
                // This gap has size reduced to 0: delete ourselves
                // TODO: maybe consider keeping as a "cache' for Bumps in the future?
                Debug.Assert(Prev != null && Next != null, "Gap found not in middle of segments");
                RemoveSelf();
            }
        }

        private void GrowForBump()
        {
            // Our Next has just moved up by 1
            if (IsGap)
            {
                // Just grow the gap
                var con = Constraint; // This copies
                con.number++;
                Constraint = con;
                Debug.Assert(EndIndex == Next.StartIndex);
                return;
            }

            // We need to introduce a new gap object gap
            var constraint = new Constraint { color = ColorSpace.Empty, number = 1 };
            ConstraintSegment gap = new ConstraintSegment(constraint, EndIndex);
            InsertAfterSelf(gap);
        }

        private void ResetFolowing()
        {
            if (Next == null)
                return;

            if (Next.IsGap)
            {
                if (Next.IsNecessaryGap())
                {
                    // Keep gap but reset its size to 1
                    var constr = Next.Constraint;
                    constr.number = 1;
                    Next.Constraint = constr;
                }
                else
                {
                    // Delete gap
                    Next.RemoveSelf();
                }
            }

            Next.StartIndex = EndIndex;
            Next.ResetFolowing();
        }

        private bool IsNecessaryGap()
        {
            if (!IsGap)
                return false;

            Debug.Assert(Prev != null && Next != null);

            return Prev.Constraint.color == Next.Constraint.color;
        }
    }
}

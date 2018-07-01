using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NonogramSolver.Backend
{
    internal class AssignmentGenerator
    {
        private readonly IBoardView boardView;
        private readonly IConstraintSet constraints;

        private GapNode firstNode;
        private GapNode lastNode;

        public AssignmentGenerator(IBoardView boardView, IConstraintSet constraints)
        {
            this.boardView = boardView;
            this.constraints = constraints;
        }

        public bool MoveNext()
        {
            if (lastNode == null)
            {
                Build(out firstNode, out lastNode);

                // Right after building, it may be invalid,
                // because we haven't ever checked anything
                // Reset will force the whole chain to verify
                return firstNode.Reset(boardView);
            }

            ColorNode currentNode = lastNode.PrevNode;
            while (currentNode != null)
            {
                bool success = currentNode.Bump(boardView);
                if (success)
                    return true;

                currentNode = currentNode.PrevNode.PrevNode;
            }

            return false;
        }

        public IReadOnlyList<uint> GetAssignment()
        {
            if (firstNode == null)
                throw new InvalidOperationException("MoveNext must be called before GetAssignment");

            uint[] colors = new uint[boardView.Count];
            firstNode.ExtractAssignment(colors);
            return colors;
        }

        private void Build(out GapNode firstGap, out GapNode lastGap)
        {
            firstGap = new GapNode(null);
            GapNode prevGap = firstGap;
            for (int i = 0; i < constraints.Count; i++)
            {
                int min = boardView.ConstraintState.minValues[i];
                int max = boardView.ConstraintState.maxValues[i];
                ColorNode colorNode = new ColorNode(prevGap, constraints[i], min, max + 1);
                prevGap.SetNext(colorNode);

                prevGap = new GapNode(colorNode);
                colorNode.SetNext(prevGap);
            }

            prevGap.MarkAsEnd(boardView.Count);
            lastGap = prevGap;
        }

        private class ColorNode
        {
            private readonly int length;
            private readonly uint color;
            private readonly int min;
            private readonly int max;

            public ColorNode(GapNode prev, Constraint constraint, int minValue, int maxValue)
            {
                length = (int)constraint.number;
                color = constraint.color;
                min = minValue;
                max = maxValue;

                PrevNode = prev;
                NextNode = null;

                Start = min;
            }

            public GapNode PrevNode { get; private set; }
            public GapNode NextNode { get; private set; }

            public int Start { get; private set; }
            public int End => Start + length;

            public uint Color => color;

            public void SetNext(GapNode gap)
            {
                Debug.Assert(NextNode == null);
                Debug.Assert(gap != null);
                NextNode = gap;
            }

            public bool Bump(IBoardView boardView)
            {
                Debug.Assert(NextNode != null);

                return MoveWorker(Start + 1, boardView);
            }

            public bool Reset(IBoardView boardView)
            {
                Debug.Assert(NextNode != null);

                int start = Math.Max(min, PrevNode.MinEnd);

                return MoveWorker(start, boardView);
            }

            public bool Verify(IBoardView boardView)
            {
                for (int i = Start; i < End; i++)
                {
                    bool valid = boardView[i].HasColor(color);
                    if (!valid)
                        return false;
                }

                return true;
            }

            public void ExtractAssignment(IList<uint> colors)
            {
                for (int i = Start; i < End; i++)
                {
                    colors[i] = color;
                }

                NextNode.ExtractAssignment(colors);
            }

            private bool MoveWorker(int minStartVal, IBoardView boardView)
            {
                Start = minStartVal;

                while (End <= max)
                {
                    if (Verify(boardView))
                    {
                        if (PrevNode.VerifyNextMoved(boardView))
                        {
                            if (NextNode.Reset(boardView))
                            {
                                return true;
                            }
                        }
                    }

                    Start++;
                }

                return false;
            }
        }

        private class GapNode
        {
            public GapNode(ColorNode prev)
            {
                PrevNode = prev;
                NextNode = null;

                Start = PrevNode != null ? PrevNode.End : 0;
                End = Start;
            }

            public ColorNode PrevNode { get; private set; }
            public ColorNode NextNode { get; private set; }

            public bool IsRequired { get; private set; }

            public int Start { get; private set; }
            public int End { get; private set; }
            public int MinEnd => IsRequired ? Start + 1 : Start;
            public int Length => End - Start;

            public void SetNext(ColorNode color)
            {
                Debug.Assert(NextNode == null);
                Debug.Assert(Start == End);
                Debug.Assert(color != null);

                if (PrevNode != null)
                {
                    if (PrevNode.Color == color.Color)
                    {
                        IsRequired = true;
                    }
                }
                NextNode = color;

                Debug.Assert(NextNode.Start >= MinEnd);
                End = NextNode.Start;
            }

            public void MarkAsEnd(int boardSize)
            {
                Debug.Assert(NextNode == null);
                End = boardSize;
            }

            public bool Reset(IBoardView boardView)
            {
                Start = PrevNode != null ? PrevNode.End : 0;

                if (NextNode == null)
                {
                    return Verify(boardView);
                }
                else
                {
                    End = MinEnd;
                }

                return NextNode.Reset(boardView);
            }

            public bool Verify(IBoardView boardView)
            {
                for (int i = Start; i < End; i++)
                {
                    bool valid = boardView[i].HasColor(ColorSpace.Empty);
                    if (!valid)
                        return false;
                }

                return true;
            }

            public bool VerifyNextMoved(IBoardView boardView)
            {
                int newEnd = NextNode.Start;
                int newLength = newEnd - Start;
                int minLength = IsRequired ? 1 : 0;

                if (newLength < minLength)
                    return false;

                End = newEnd;
                return Verify(boardView);
            }

            public void ExtractAssignment(IList<uint> colors)
            {
                for (int i = Start; i < End; i++)
                {
                    colors[i] = ColorSpace.Empty;
                }

                if (NextNode != null)
                {
                    NextNode.ExtractAssignment(colors);
                }
            }
        }
    }
}

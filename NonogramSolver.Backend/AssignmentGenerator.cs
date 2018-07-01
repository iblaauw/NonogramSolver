using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NonogramSolver.Backend
{
    internal class AssignmentGenerator
    {
        private readonly IBoardView boardView;
        private readonly IConstraintSet constraints;

        private int[] currentStarts;
        private int[] ends;
        private bool[] requiresGapAfter;

        public AssignmentGenerator(IBoardView boardView, IConstraintSet constraints)
        {
            this.boardView = boardView;
            this.constraints = constraints;
            currentStarts = (int[])boardView.ConstraintState.minValues.Clone();
            CalculateEndsAndGaps();
        }

        public Assignment Current
        {
            get
            {
                return new Assignment(currentStarts, constraints, boardView.Count);
            }
        }

        public bool Init()
        {
            if (constraints.Count > 0)
            {
                return DoSetStartPointAtLeast(0, boardView.ConstraintState.minValues[0]);
            }

            return true;
        }

        public bool MoveNext()
        {
            return GoToNextValidStartPoints();
        }

        private bool GoToNextValidStartPoints()
        {
            for (int constraintIndex = currentStarts.Length - 1; constraintIndex >= 0; constraintIndex--)
            {
                bool success = BumpStartPoint(constraintIndex);
                if (success)
                    return true;
            }
            return false;
        }

        private bool BumpStartPoint(int constraintIndex)
        {
            int start = currentStarts[constraintIndex];
            start++;
            return DoSetStartPointAtLeast(constraintIndex, start);
        }

        // Sets the given constraint to start at least at the given value, may actually be set to something later
        private bool DoSetStartPointAtLeast(int constraintIndex, int value)
        {
            int end = ends[constraintIndex];
            int initialStartValue = Math.Max(value, boardView.ConstraintState.minValues[constraintIndex]);

            for (int newStart = initialStartValue; newStart < end; newStart++)
            {
                if (IsStartPointValidForBoard(newStart, constraintIndex))
                {
                    // A valid spot is found for the current constraint!

                    if (constraintIndex < constraints.Count - 1)
                    {
                        // Now we need to reset any constraints that happen after the current one
                        int selfEnd = newStart + (int)constraints[constraintIndex].number;
                        if (requiresGapAfter[constraintIndex])
                        {
                            selfEnd++;
                        }

                        if (!DoSetStartPointAtLeast(constraintIndex + 1, selfEnd))
                        {
                            // This spot is valid for the current constraint... but not for our preceding constraints
                            // Don't use this position
                            continue;
                        }
                    }

                    currentStarts[constraintIndex] = newStart;
                    return true;
                }
            }

            return false;
        }

        // If we put the given constraint at the given startPoint, is that valid for the current board state?
        private bool IsStartPointValidForBoard(int startPoint, int constraintIndex)
        {
            var constraint = constraints[constraintIndex];
            int end = startPoint + (int)constraint.number;
            for (int i = startPoint; i < end; i++)
            {
                bool valid = boardView[i].HasColor(constraint.color);
                if (!valid)
                    return false;
            }

            return true;
        }

        private bool IsGapValidForBoard(int start, int end)
        {
            for (int i = start; i < end; i++)
            {
                bool valid = boardView[i].HasColor(ColorSpace.Empty);
                if (!valid)
                    return false;
            }

            return true;
        }

        private void CalculateEndsAndGaps()
        {
            ends = new int[currentStarts.Length];
            requiresGapAfter = new bool[currentStarts.Length];

            for (int i = 0; i < ends.Length; i++)
            {
                ends[i] = boardView.ConstraintState.maxValues[i] - (int)constraints[i].number + 2; // This is +2 because maxValues is inclusive
            }

            for (int i = 0; i < requiresGapAfter.Length - 1; i++)
            {
                var color = constraints[i].color;
                var nextColor = constraints[i + 1].color;
                requiresGapAfter[i] = color == nextColor;
            }
        }

        public class Assignment
        {
            private readonly IReadOnlyList<int> startPoints;
            private readonly IConstraintSet constraints;
            private readonly int boardSize;

            public Assignment(IReadOnlyList<int> startPoints, IConstraintSet constraints, int boardSize)
            {
                this.startPoints = startPoints;
                this.constraints = constraints;
                this.boardSize = boardSize;
            }

            public IReadOnlyList<uint> ExtractColors()
            {
                uint[] colors = new uint[boardSize];

                int constraintIndex = 0;
                int i = 0; 
                while (constraintIndex < startPoints.Count)
                {
                    int start = startPoints[constraintIndex];
                    Fill(colors, i, start, ColorSpace.Empty);

                    var constraint = constraints[constraintIndex];
                    int end = start + (int)constraint.number;
                    Fill(colors, start, end, constraint.color);

                    i = end;
                    constraintIndex++;
                }

                Fill(colors, i, boardSize, ColorSpace.Empty);

                return colors;
            }

            // TODO: make an extension method / util somewhere?
            private static void Fill(uint[] colors, int start, int end, uint color)
            {
                for (int i = start; i < end; i++)
                {
                    colors[i] = color;
                }
            }
        }

    }
}

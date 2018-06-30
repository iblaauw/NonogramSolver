﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NonogramSolver.Backend
{
    class ConstraintState
    {
        public ConstraintState(int boardSize, int numConstraints)
        {
            minValues = Enumerable.Repeat(0, numConstraints).ToArray();
            maxValues = Enumerable.Repeat(boardSize-1, numConstraints).ToArray();
        }

        private ConstraintState(int[] mins, int[] maxs)
        {
            minValues = (int[])mins.Clone();
            maxValues = (int[])maxs.Clone();
        }

        public readonly int[] minValues;
        public readonly int[] maxValues;

        public ConstraintState Clone()
        {
            return new ConstraintState(minValues, maxValues);
        }
    }

    class Constrainer
    {
        private readonly IConstraintSet constraintSet;
        private readonly int boardSize;

        public Constrainer(int index, bool isRow, int size, IConstraintSet constraints)
        {
            constraintSet = constraints;
            boardSize = size;
            Index = index;
            IsRow = isRow;
        }

        public int Index { get; private set; }
        public bool IsRow { get; private set; }
        public bool IsDirty { get; set; } = false;

        public ConstrainResult ConstrainBoard(IBoardView boardView, ConstraintState state)
        {
            var result = VerifyValid(boardView);
            if (result == ConstrainResult.NoSolution)
                return ConstrainResult.NoSolution;

            AdjustInitialMins(state);
            AdjustInitialMaxs(state);
            CalculateMin(boardView, state);
            CalculateMax(boardView, state);

            // TODO: roll these into CalculateMin/Max
            AdjustInitialMins(state);
            AdjustInitialMaxs(state);
            return Emit(boardView, state);
        }

        private void AdjustInitialMins(ConstraintState state)
        {
            Debug.Assert(state.minValues.Length > 0);
            int prevMin = state.minValues[0];
            for (int i = 1; i < state.minValues.Length; i++)
            {
                int forcedGapOffset = constraintSet[i - 1].color == constraintSet[i].color ? 1 : 0;
                long lowMargin = prevMin + constraintSet[i - 1].number + forcedGapOffset;
                if (state.minValues[i] < lowMargin)
                {
                    state.minValues[i] = (int)lowMargin;
                }

                prevMin = state.minValues[i];
            }
        }

        private void AdjustInitialMaxs(ConstraintState state)
        {
            Debug.Assert(state.maxValues.Length > 0);
            int lastIndex = state.maxValues.Length - 1;
            int prevMax = state.maxValues[lastIndex];
            for (int i = lastIndex-1; i >= 0; i--)
            {
                int forcedGapOffset = constraintSet[i + 1].color == constraintSet[i].color ? 1 : 0;
                long highMargin = prevMax - constraintSet[i + 1].number - forcedGapOffset;
                if (state.maxValues[i] > highMargin)
                {
                    state.maxValues[i] = (int)highMargin;
                }

                prevMax = state.maxValues[i];
            }
        }

        private void CalculateMin(IBoardView boardView, ConstraintState state)
        {
            for (int constrIndex = 0; constrIndex < state.minValues.Length; constrIndex++)
            {
                Constraint constraint = constraintSet[constrIndex];
                int max = state.maxValues[constrIndex];
                int startPoint = state.minValues[constrIndex];
                for (int boardIndex = startPoint; boardIndex <= max; boardIndex++)
                {
                    if (!boardView[boardIndex].HasColor(constraint.color))
                    {
                        startPoint = boardIndex + 1;
                    }
                    else if (boardIndex - startPoint + 1 >= constraint.number)
                        break;
                }

                state.minValues[constrIndex] = startPoint;
            }
        }

        private void CalculateMax(IBoardView boardView, ConstraintState state)
        {
            for (int constrIndex = state.minValues.Length - 1; constrIndex >= 0; constrIndex--)
            {
                Constraint constraint = constraintSet[constrIndex];
                int min = state.minValues[constrIndex];
                int startPoint = state.maxValues[constrIndex];
                for (int boardIndex = startPoint; boardIndex >= min; boardIndex--)
                {
                    if (!boardView[boardIndex].HasColor(constraint.color))
                    {
                        startPoint = boardIndex - 1;
                    }
                    else if (startPoint - boardIndex + 1 >= constraint.number)
                        break;
                }

                state.maxValues[constrIndex] = startPoint;
            }
        }

        private ConstrainResult Emit(IBoardView boardView, ConstraintState state)
        {
            ColorSet[] colors = new ColorSet[boardView.Count];

            for (int boardIndex = 0; boardIndex < boardView.Count; boardIndex++)
            {
                colors[boardIndex] = ColorSet.CreateSingle(ColorSpace.Empty);

                for (int constraintIndex = 0; constraintIndex < state.minValues.Length; constraintIndex++)
                {
                    int min = state.minValues[constraintIndex];
                    int max = state.maxValues[constraintIndex];

                    if (boardIndex >= min && boardIndex <= max)
                    {
                        Constraint constraint = constraintSet[constraintIndex];

                        // Check for a guaranteed region
                        long offset = (max - min + 1) - constraint.number;
                        if (boardIndex >= min + offset && boardIndex <= max - offset)
                        {
                            // If this is a guaranteed region, then we know it can't be any other possible color, 
                            colors[boardIndex] = ColorSet.CreateSingle(constraint.color);
                            break;
                        }

                        colors[boardIndex] = colors[boardIndex].AddColor(constraint.color);
                    }
                }
            }

            return boardView.IntersectAll(colors);
        }

        private ConstrainResult VerifyValid(IBoardView boardView)
        {
            int constraintIndex = 0;
            int colorCount = 0;
            for (int i = 0; i < boardView.Count; i++)
            {
                ColorSet color = boardView[i];
                if (!color.IsSingle())
                    return ConstrainResult.Success;

                uint currentColor = color.GetSingleColor();
                if (currentColor == 0)
                {
                    if (colorCount != 0)
                    {
                        return ConstrainResult.NoSolution;
                    }
                }
                else
                {
                    if (constraintIndex >= constraintSet.Count)
                        return ConstrainResult.NoSolution;

                    if (currentColor == constraintSet[constraintIndex].color)
                    {
                        colorCount++;
                        if (colorCount == constraintSet[constraintIndex].number)
                        {
                            constraintIndex++;
                            colorCount = 0;
                        }
                    }
                    else
                    {
                        return ConstrainResult.NoSolution;
                    }
                }
            }

            return ConstrainResult.Success;
        }

    }
}
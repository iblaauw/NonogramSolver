using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace NonogramSolver.Backend
{
    internal class ConstraintList
    {
        private readonly IConstraintSet constraintSet;
        private readonly int boardSize;

        public ConstraintList(int index, bool isRow, int size, IConstraintSet constraints)
        {
            constraintSet = constraints;
            boardSize = size;
            Index = index;
            IsRow = isRow;
        }

        public int Index { get; private set; }
        public bool IsRow { get; private set; }
        public bool IsDirty { get; set; } = false;

        public ConstrainResult ConstrainBoard(IBoardView boardView)
        {
            ConstraintSegment.CreateChain(constraintSet, out ConstraintSegment segmentBegin, out ConstraintSegment segmentEnd);

            // Start final colors as completely empty: no colors possible at all
            ColorSet[] finalColors = new ColorSet[boardView.Count];

            do
            {
                IReadOnlyList<uint> segmentColors = GetColorsFromSegments(segmentBegin);
                if (AreCompatible(boardView, segmentColors))
                {
                    Merge(segmentColors, finalColors);
                }
            } while (segmentEnd.Bump(boardView.Count));

            // TODO: this will place this constraint on the dirty list again, which is slightly redundant...
            var result = boardView.IntersectAll(finalColors);

            return result;
        }

        private IReadOnlyList<uint> GetColorsFromSegments(ConstraintSegment begin)
        {
            uint[] colorSets = new uint[boardSize];

            ConstraintSegment current = begin;
            uint emptyColor = ColorSpace.Empty; //ColorSet emptyColor = ColorSet.Empty.AddColor(ColorSpace.Empty);

            int i = 0;

            while (i != current.StartIndex)
            {
                colorSets[i] = emptyColor;
                i++;
            }

            while (current != null)
            {
                Debug.Assert(i == current.StartIndex);
                uint currentColor = current.Constraint.color;
                while (i != current.EndIndex)
                {
                    colorSets[i] = currentColor;
                    i++;
                }

                current = current.Next;
            }

            while (i != boardSize)
            {
                colorSets[i] = emptyColor;
                i++;
            }

            return colorSets;
        }

        private bool AreCompatible(IBoardView boardColors, IReadOnlyList<uint> segmentColors)
        {
            Debug.Assert(boardColors.Count == segmentColors.Count);
            for (int i = 0; i < segmentColors.Count; i++)
            {
                if (!boardColors[i].HasColor(segmentColors[i]))
                    return false;
            }

            return true;
        }

        private void Merge(IReadOnlyList<uint> from, ColorSet[] into)
        {
            Debug.Assert(from.Count == into.Length);

            for (int i = 0; i < from.Count; i++)
            {
                into[i] = into[i].AddColor(from[i]);
            }
        }
    }
}

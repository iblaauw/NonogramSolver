using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace NonogramSolver.Backend
{
    internal class ConstraintList : IList<Constraint> // TODO: the IList implement is probably unnecessary
    {
        private List<Constraint> constraints;
        private readonly int boardSize;

        public ConstraintList(int index, bool isRow, int size)
        {
            IsDirty = true;
            Index = index;
            boardSize = size;
            IsRow = isRow;

            constraints = new List<Constraint>();
        }

        public bool IsDirty { get; private set; }
        public int Index { get; private set; }
        public bool IsRow { get; private set; }

        public void SetDirty()
        {
            IsDirty = true;
        }

        public BoardState.IntersectResult ConstrainBoard(IBoardView boardView)
        {
            ConstraintSegment.CreateChain(constraints, out ConstraintSegment segmentBegin, out ConstraintSegment segmentEnd);

            // Start final colors as completely empty: no colors possible at all
            ColorSet[] finalColors = new ColorSet[boardView.Count];

            do
            {
                IReadOnlyList<ColorSet> segmentColors = GetColorsFromSegments(segmentBegin);
                if (AreCompatible(boardView, segmentColors))
                {
                    Merge(segmentColors, finalColors);
                }
            } while (segmentEnd.Bump(boardView.Count));

            var result = boardView.IntersectAll(finalColors);

            // This has to be after the above IntersectAll, which will try to mark this constraint as Dirty
            IsDirty = false;

            return result;
        }

        private IReadOnlyList<ColorSet> GetColorsFromSegments(ConstraintSegment begin)
        {
            ColorSet[] colorSets = new ColorSet[boardSize];

            ConstraintSegment current = begin;
            ColorSet emptyColor = ColorSet.Empty.AddColor(ColorSpace.Empty);

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
                ColorSet currentColorSet = ColorSet.Empty.AddColor(currentColor);
                while (i != current.EndIndex)
                {
                    colorSets[i] = currentColorSet;
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

        private bool AreCompatible(IReadOnlyList<ColorSet> boardColors, IReadOnlyList<ColorSet> segmentColors)
        {
            Debug.Assert(boardColors.Count == segmentColors.Count);
            var contained = boardColors.Zip(segmentColors, (b, s) => b.Intersect(s) != ColorSet.Empty);
            return contained.All(b => b);
        }

        private bool AreCompatible(IBoardView boardColors, IReadOnlyList<ColorSet> segmentColors)
        {
            Debug.Assert(boardColors.Count == segmentColors.Count);
            for (int i = 0; i < segmentColors.Count; i++)
            {
                ColorSet result = boardColors[i].Intersect(segmentColors[i]);
                if (result.IsEmpty)
                    return false;
            }

            return true;
        }

        private void Merge(IReadOnlyList<ColorSet> from, ColorSet[] into)
        {
            Debug.Assert(from.Count == into.Length);

            for (int i = 0; i < from.Count; i++)
            {
                into[i] = into[i].Union(from[i]);
            }
        }

        #region IList Overrides
        public Constraint this[int index]
        {
            get => constraints[index];
            set
            {
                constraints[index] = value;
            }
        }

        public int Count => constraints.Count;

        public bool IsReadOnly => false;

        public void Add(Constraint item) => constraints.Add(item);

        public void Clear() => constraints.Clear();

        public bool Contains(Constraint item) => constraints.Contains(item);

        public void CopyTo(Constraint[] array, int arrayIndex) => constraints.CopyTo(array, arrayIndex);

        public IEnumerator<Constraint> GetEnumerator() => constraints.GetEnumerator();

        public int IndexOf(Constraint item) => constraints.IndexOf(item);

        public void Insert(int index, Constraint item) => constraints.Insert(index, item);

        public bool Remove(Constraint item) => constraints.Remove(item);

        public void RemoveAt(int index) => constraints.RemoveAt(index);

        IEnumerator IEnumerable.GetEnumerator() => constraints.GetEnumerator();

        #endregion
    }
}

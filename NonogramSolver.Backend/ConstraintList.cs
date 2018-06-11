using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace NonogramSolver.Backend
{
    internal class ConstraintList : IList<Constraint>
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

        public IReadOnlyList<ColorSet> CalculateColorSets(BoardState board)
        {
            ColorSet[] boardColors = GetCurrentBoardColors(board);
            Debug.Assert(boardColors.Length == boardSize);
            ConstraintSegment.CreateChain(constraints, out ConstraintSegment segmentBegin, out ConstraintSegment segmentEnd);

            // Start final colors as completely empty: no colors possible at all
            ColorSet[] finalColors = new ColorSet[boardSize];

            do
            {
                IReadOnlyList<ColorSet> segmentColors = GetColorsFromSegments(segmentBegin);
                if (AreCompatible(boardColors, segmentColors))
                {
                    Merge(segmentColors, finalColors);
                }
            } while (segmentEnd.Bump(boardSize));

            IsDirty = false;
            return finalColors;
        }

        private ColorSet[] GetCurrentBoardColors(BoardState boardState)
        {
            Func<int, Tile> getter;
            if (IsRow)
            {
                getter = (i => boardState[Index, i]);
            }
            else
            {
                getter = (i => boardState[i, Index]);
            }

            ColorSet[] boardColors = new ColorSet[boardSize];
            for (int i = 0; i < boardSize; i++)
            {
                boardColors[i] = getter(i).Colors;
            }

            return boardColors;
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

        private void Merge(IReadOnlyList<ColorSet> from, ColorSet[] into)
        {
            Debug.Assert(from.Count == into.Length);

            for (int i = 0; i < from.Count; i++)
            {
                into[i] = into[i].Union(from[i]);
            }
        }

        private class ConstraintCombination
        {
            private List<int> startIndices;
            private ConstraintList parent;

            public ConstraintCombination(ConstraintList parent)
            {
                this.parent = parent;
                BuildInitial();
            }

            private void BuildInitial()
            {
                startIndices = Enumerable.Repeat(0, parent.Count).ToList();
                BuildWorker(0, 0);
            }

            private void BuildWorker(int currentPoint, int index)
            {
                if (index >= parent.Count)
                    return;

                if (index > 0)
                {
                    if (parent[index-1].color == parent[index].color)
                    {
                        currentPoint++;
                    }
                }

                startIndices[index] = currentPoint;

                currentPoint += (int)(parent[index].number);

                Debug.Assert(currentPoint <= parent.boardSize);
                BuildWorker(currentPoint, index + 1);
            }

            private bool Bump()
            {
                int endPoint = parent.boardSize;
                uint prevColor = ColorSpace.Empty;
                for (int i = startIndices.Count - 1; i >= 0; i++)
                {
                    int endAt = startIndices[i] + (int)parent[i].number;

                    uint currentColor = parent[i].color;
                    if (currentColor == prevColor)
                    {
                        // Need to add a gap
                        endAt--;
                    }

                    prevColor = currentColor;

                    if (endAt < endPoint)
                    {
                        // TODO: this doesn't compile
                        //BuildFrom(startIndices[i] + 1, i);
                        return true;
                    }
                }

                return false;
            }

            private bool BumpWorker(int currentPoint, int index)
            {
                if (index < 0)
                    return false;

                // currentPoint represents the beginning of a segment
                int endOfCurrent = currentPoint + (int)(parent[index].number);
                int startOfNext;
                if (index < parent.Count - 1)
                {
                    startOfNext = startIndices[index + 1];

                    if (parent[index].color == parent[index+1].color)
                    {
                        startOfNext--;
                    }
                }
                else
                {
                    startOfNext = parent.Count - 1;
                }

                int startOfCurrent = startIndices[index];
                if (endOfCurrent < startOfNext)
                {
                    BuildWorker(startOfCurrent + 1, index);
                    return true;
                }

                return BumpWorker(startOfCurrent, index - 1);
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

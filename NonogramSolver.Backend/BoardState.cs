using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NonogramSolver.Backend
{
    interface IBoardView
    {
        ColorSet this[int index] { get; }
        int Count { get; }
        BoardState.IntersectResult IntersectAll(IReadOnlyList<ColorSet> colorSets);
    }

    class BoardState
    {
        private ColorSet[,] colors;
        private Board owningBoard;
        private readonly Guesser guesser;

        public enum IntersectResult
        {
            Changed,
            NoChange,
            NoSolution
        }

        public BoardState(Board board)
        {
            owningBoard = board;
            guesser = new Guesser(board);
            CreateTiles();
        }

        private BoardState(BoardState other)
        {
            // A copy constructor, essentially
            colors = (ColorSet[,])other.colors.Clone();
            owningBoard = other.owningBoard;
            guesser = new Guesser(other.owningBoard);
        }

        public ColorSet this[int x, int y] => colors[x, y];

        public Guesser Guesser => guesser;

        public ISolvedBoard ExtractSolvedBoard()
        {
            int numRows = owningBoard.NumRows;
            int numCols = owningBoard.NumColumns;
            SolvedBoard solvedBoard = new SolvedBoard(numRows, numCols);

            for (int i = 0; i < numRows; i++)
            {
                for (int j = 0; j < numCols; j++)
                {
                    ColorSet tile = colors[i, j];
                    Debug.Assert(tile.IsSingle());
                    solvedBoard[i, j] = tile.GetSingleColor();
                }
            }

            return solvedBoard;
        }

        public IBoardView CreateRowView(int index)
        {
            Debug.Assert(index >= 0 && index < owningBoard.NumRows);
            return new RowView(index, this);
        }

        public IBoardView CreateColView(int index)
        {
            Debug.Assert(index >= 0 && index < owningBoard.NumColumns);
            return new ColView(index, this);
        }

        public BoardState CreateNewLayer()
        {
            return new BoardState(this);
        }

        public void SetColor(int x, int y, ColorSet value)
        {
            ColorSet original = colors[x, y];
            colors[x, y] = value;
            if (original != value)
            {
                owningBoard.OnTileDirty(x, y);
            }
        }

        public bool CalculateIsSolved()
        {
            for (int i = 0; i < owningBoard.NumRows; i++)
            {
                for (int j = 0; j < owningBoard.NumColumns; j++)
                {
                    ColorSet val = colors[i, j];
                    if (!val.IsSingle())
                        return false;
                }
            }
            return true;
        }

        private void CreateTiles()
        {
            int numRows = owningBoard.NumRows;
            int numCols = owningBoard.NumColumns;

            colors = new ColorSet[numRows, numCols];

            var fullColor = ColorSet.CreateFullColorSet(owningBoard.ColorSpace.MaxColor);
            for (int i = 0; i < numRows; i++)
            {
                for (int j = 0; j < numCols; j++)
                {
                    colors[i, j] = fullColor;
                }
            }
        }

        private class RowView : IBoardView
        {
            private readonly int rowIndex;
            private readonly BoardState owner;

            public RowView(int rowIndex, BoardState owner)
            {
                this.rowIndex = rowIndex;
                this.owner = owner;
            }

            public ColorSet this[int index] => owner.colors[rowIndex, index];

            public int Count => owner.owningBoard.NumColumns;

            public IntersectResult IntersectAll(IReadOnlyList<ColorSet> colorSets)
            {
                bool changed = false;
                for (int i = 0; i < Count; i++)
                {
                    var color = owner.colors[rowIndex, i];
                    ColorSet newColor = color.Intersect(colorSets[i]);

                    if (newColor.IsEmpty)
                        return IntersectResult.NoSolution;

                    if (newColor == color)
                        continue;

                    owner.colors[rowIndex, i] = newColor;
                    changed = true;
                    owner.owningBoard.OnTileDirty(rowIndex, i);
                }

                return changed ? IntersectResult.Changed : IntersectResult.NoChange;
            }
        }

        private class ColView : IBoardView
        {
            private readonly int colIndex;
            private readonly BoardState owner;

            public ColView(int colIndex, BoardState owner)
            {
                this.colIndex = colIndex;
                this.owner = owner;
            }

            public ColorSet this[int index] => owner.colors[index, colIndex];

            public int Count => owner.owningBoard.NumRows;

            public IntersectResult IntersectAll(IReadOnlyList<ColorSet> colorSets)
            {
                bool changed = false;

                for (int i = 0; i < Count; i++)
                {
                    var color = owner.colors[i, colIndex];
                    ColorSet newColor = color.Intersect(colorSets[i]);

                    if (newColor.IsEmpty)
                        return IntersectResult.NoSolution;

                    if (newColor == color)
                        continue;

                    owner.colors[i, colIndex] = newColor;
                    changed = true;
                    owner.owningBoard.OnTileDirty(i, colIndex);
                }

                return changed ? IntersectResult.Changed : IntersectResult.NoChange;
            }
        }
    }
}

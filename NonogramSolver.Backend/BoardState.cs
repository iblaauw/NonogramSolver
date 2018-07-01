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
        ConstraintState ConstraintState { get; }
        ConstrainResult IntersectAll(IReadOnlyList<ColorSet> colorSets);
    }

    class BoardState
    {
        private ColorSet[,] colors;
        private ConstraintState[] rowConstraints;
        private ConstraintState[] colConstraints;
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
            CreateConstraintStates(board);
        }

        private BoardState(BoardState other)
        {
            // A copy constructor, essentially

            colors = (ColorSet[,])other.colors.Clone();
            rowConstraints = other.rowConstraints.Select(c => c.Clone()).ToArray();
            colConstraints = other.colConstraints.Select(c => c.Clone()).ToArray();

            owningBoard = other.owningBoard;

            // Specifically don't clone the other board's guesser
            guesser = new Guesser(other.owningBoard);
        }

        public ColorSet this[int x, int y] => colors[x, y];

        public Guesser Guesser => guesser;

        public IReadOnlyList<ConstraintState> RowConstraintStates => rowConstraints;
        public IReadOnlyList<ConstraintState> ColConstraintStates => colConstraints;

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

        private void CreateConstraintStates(Board board)
        {
            rowConstraints = board.RowConstraints.Select(c => new ConstraintState(board.NumColumns, c.Count)).ToArray();
            colConstraints = board.ColumnConstraints.Select(c => new ConstraintState(board.NumRows, c.Count)).ToArray();
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

            public ConstraintState ConstraintState => owner.rowConstraints[rowIndex];

            public ConstrainResult IntersectAll(IReadOnlyList<ColorSet> colorSets)
            {
                for (int i = 0; i < Count; i++)
                {
                    var color = owner.colors[rowIndex, i];
                    ColorSet newColor = color.Intersect(colorSets[i]);

                    if (newColor.IsEmpty)
                        return ConstrainResult.NoSolution;

                    if (newColor != color)
                    {
                        owner.colors[rowIndex, i] = newColor;
                        owner.owningBoard.OnTileDirty(rowIndex, i);
                    }
                }

                return ConstrainResult.Success;
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

            public ConstraintState ConstraintState => owner.colConstraints[colIndex];

            public ConstrainResult IntersectAll(IReadOnlyList<ColorSet> colorSets)
            {
                for (int i = 0; i < Count; i++)
                {
                    var color = owner.colors[i, colIndex];
                    ColorSet newColor = color.Intersect(colorSets[i]);

                    if (newColor.IsEmpty)
                        return ConstrainResult.NoSolution;

                    if (newColor != color)
                    {
                        owner.colors[i, colIndex] = newColor;
                        owner.owningBoard.OnTileDirty(i, colIndex);
                    }
                }

                return ConstrainResult.Success;
            }
        }
    }
}

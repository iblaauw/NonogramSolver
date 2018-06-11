using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NonogramSolver.Backend
{
    class BoardState
    {
        private Tile[,] tiles;

        public BoardState(Board owningBoard)
        {
            CreateTiles(owningBoard);
        }

        public Tile this[int x, int y] => tiles[x, y];

        public bool IntersectColorSetsOnRow(int row, IReadOnlyList<ColorSet> colors)
        {
            int numCols = tiles.GetLength(1);
            Debug.Assert(colors.Count == numCols);

            bool changed = false;
            for (int i = 0; i < numCols; i++)
            {
                bool c = tiles[row, i].IntersectWith(colors[i]);
                changed = changed || c;
            }
            return changed;
        }

        public bool IntersectColorSetsOnColumn(int col, IReadOnlyList<ColorSet> colors)
        {
            int numRows = tiles.GetLength(0);
            Debug.Assert(colors.Count == numRows);

            bool changed = false;
            for (int i = 0; i < numRows; i++)
            {
                bool c = tiles[i, col].IntersectWith(colors[i]);
                changed = changed || c;
            }
            return changed;
        }

        public ISolvedBoard ExtractSolvedBoard()
        {
            int numRows = tiles.GetLength(0);
            int numCols = tiles.GetLength(1);
            SolvedBoard solvedBoard = new SolvedBoard(numRows, numCols);

            for (int i = 0; i < numRows; i++)
            {
                for (int j = 0; j < numCols; j++)
                {
                    Tile tile = tiles[i, j];
                    Debug.Assert(tile.IsDecided);
                    solvedBoard[i, j] = tile.DecidedColor;
                }
            }

            return solvedBoard;
        }

        private void CreateTiles(Board board)
        {
            int numRows = board.NumRows;
            int numCols = board.NumColumns;
            tiles = new Tile[numRows, numCols];
            for (int i = 0; i < numRows; i++)
            {
                for (int j = 0; j < numCols; j++)
                {
                    tiles[i, j] = new Tile(i, j, board);
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace NonogramSolver.Backend
{
    public class UnsolvableBoardException : Exception
    {
        public UnsolvableBoardException() { }
        public UnsolvableBoardException(string message) : base(message) { }
        public UnsolvableBoardException(string message, Exception inner) : base(message, inner) { }
    }

    internal class Tile
    {
        private readonly Board board;

        public Tile(int row, int column, Board board)
        {
            Row = row;
            Column = column;
            this.board = board;
            Colors = ColorSet.CreateFullColorSet(board.ColorSpace.MaxColor);

            IsDecided = false;
            DecidedColor = 0;
        }

        public ColorSet Colors { get; set; }
        public readonly int Row;
        public readonly int Column;

        public bool IsDecided { get; private set; }
        public uint DecidedColor { get; private set; }

        public bool IntersectWith(ColorSet colorSet)
        {
            ColorSet original = Colors;
            ColorSet newColors = colorSet.Intersect(original);
            if (newColors != original)
            {
                if (newColors.IsSingle())
                {
                    Debug.Assert(!IsDecided && DecidedColor == 0);
                    IsDecided = true;
                    DecidedColor = newColors.GetSingleColor();
                }

                if (newColors.IsEmpty)
                    throw new UnsolvableBoardException();

                Colors = newColors;
                FireOnDirty();
                return true;
            }

            return false;
        }

        private void FireOnDirty()
        {
            board.OnTileDirty(Row, Column);
        }
    }
}

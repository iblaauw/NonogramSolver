using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NonogramSolver.Backend
{
    class Guesser
    {
        private int row;
        private int col;
        private uint color;
        private readonly Board board;

        public class Guess
        {
            public Guess(int x, int y, uint color)
            {
                X = x;
                Y = y;
                Color = color;
            }
            
            public readonly int X;
            public readonly int Y;
            public readonly uint Color;
        }

        public Guesser(Board board)
        {
            this.board = board;
            Reset();
        }

        public Guess GenerateNext(BoardState state)
        {
            Guess guess = null;
            while (!IsDone())
            {
                if (IsCompatible(state))
                {
                    guess = new Guess(row, col, color);
                    Increment();
                    break;
                }

                Increment();
            }

            return guess;
        }

        public void Reset()
        {
            row = 0;
            col = 0;
            color = 0;
        }

        private void Increment()
        {
            color++;
            if (color == board.ColorSpace.MaxColor)
            {
                color = 0;
                col++;
                if (col == board.NumColumns)
                {
                    col = 0;
                    row++;
                }
            }
        }

        private bool IsDone()
        {
            return row >= board.NumRows;
        }

        private bool IsCompatible(BoardState state)
        {
            ColorSet val = state[row, col];
            return val.HasColor(color) && !val.IsSingle();
        }
    }
}

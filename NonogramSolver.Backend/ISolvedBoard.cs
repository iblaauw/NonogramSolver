using System;
using System.Collections.Generic;
using System.Text;

namespace NonogramSolver.Backend
{
    public interface ISolvedBoard
    {
        uint this[int x, int y] { get; }
    }

    public class SolvedBoard : ISolvedBoard
    {
        private readonly uint[,] colors;

        public SolvedBoard(int numRows, int numCols)
        {
            colors = new uint[numRows, numCols];
        }

        public uint this[int x, int y] {
            get => colors[x, y];
            set => colors[x, y] = value;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NonogramSolver.Backend
{
    // This class takes a 2 dimensional grid and extracts row / column constraints out of it
    internal class ConstraintExtractor
    {
        private readonly uint[,] grid;
        private readonly int numRows;
        private readonly int numCols;
        private Constraint current;
        private List<Constraint> currentSet;

        public ConstraintExtractor(uint[,] grid)
        {
            this.grid = grid;
            numRows = grid.GetLength(0);
            numCols = grid.GetLength(1);

            current = new Constraint();
            currentSet = new List<Constraint>();
        }

        public IReadOnlyList<IConstraintSet> RowConstraints { get; private set; }
        public IReadOnlyList<IConstraintSet> ColumnConstraints { get; private set; }

        public void Extract()
        {
            RowConstraints = Enumerable.Range(0, numRows).Select(i => CreateRowConstraint(i)).ToList();
            ColumnConstraints = Enumerable.Range(0, numCols).Select(i => CreateColConstraint(i)).ToList();
        }

        private IConstraintSet CreateRowConstraint(int index)
        {
            Func<int, uint> getter = col => grid[index, col];
            return CreateGenericConstraint(getter, numCols);
        }

        private IConstraintSet CreateColConstraint(int index)
        {
            Func<int, uint> getter = row => grid[row, index];
            return CreateGenericConstraint(getter, numRows);
        }

        public IConstraintSet CreateGenericConstraint(Func<int, uint> getter, int count)
        {
            currentSet = new List<Constraint>();
            current = new Constraint();

            for (int i = 0; i < count; i++)
            {
                uint color = getter(i);
                Accumulate(color);
            }

            // If we were accumulating when we hit the end of a row,
            // the current constraint hasn't been pushed yet
            Push();

            return BoardFactory.CreateConstraintSet(currentSet);
        }

        private void Accumulate(uint color)
        {
            if (current.color != color)
            {
                Push();
                if (color != 0)
                {
                    current.color = color;
                    current.number = 1;
                }
            }
            else
            {
                if (color != 0)
                {
                    current.number++;
                }
            }
        }

        private void Push()
        {
            if (current.number != 0)
            {
                currentSet.Add(current);
                current = new Constraint();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NonogramSolver.Backend
{
    // Wraps an IConstraintList with some extra information.
    // This extra information helps the actual solver generate the board view,
    // keep track of if its in the dirty queue, etc
    internal class ConstraintListWrapper
    {
        public ConstraintListWrapper(int index, bool isRow, IConstraintList constraint)
        {
            Constraint = constraint;
            Index = index;
            IsRow = isRow;
        }

        public IConstraintList Constraint { get; private set; }
        public int Index { get; private set; }
        public bool IsRow { get; private set; }
        public bool IsDirty { get; set; } = false;

        public IBoardView CreateBoardView(BoardState boardState)
        {
            return IsRow ? boardState.CreateRowView(Index) : boardState.CreateColView(Index);
        }
    }
}

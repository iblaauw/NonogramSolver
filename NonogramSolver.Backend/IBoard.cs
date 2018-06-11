using System;
using System.Collections.Generic;
using System.Text;

namespace NonogramSolver.Backend
{
    public interface IBoard
    {
        int NumRows { get; }
        int NumColumns { get; }
        IReadOnlyList<IConstraintSet> RowConstraints { get; }
        IReadOnlyList<IConstraintSet> ColumnConstraints { get; }
        IProgressManager ProgressManager { get; }

        ISolvedBoard Solve();
    }
}

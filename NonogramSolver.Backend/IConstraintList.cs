using System;
using System.Collections.Generic;
using System.Text;

namespace NonogramSolver.Backend
{
    internal interface IConstraintList
    {
        int EstimatedCost { get; }

        void CalculateEstimatedCost(IBoardView boardView);
        ConstrainResult ConstrainBoard(IBoardView boardView);
    }
}

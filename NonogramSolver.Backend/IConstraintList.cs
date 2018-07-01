using System;
using System.Collections.Generic;
using System.Text;

namespace NonogramSolver.Backend
{
    internal interface IConstraintList
    {
        void CalculateEstimatedCost(IBoardView boardView);
        ConstrainResult ConstrainBoard(IBoardView boardView);
    }
}

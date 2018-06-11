using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NonogramSolver.Backend
{
    public static class BoardFactory
    {
        public static IBoard CreateBoard(IEnumerable<IConstraintSet> rowConstraints, IEnumerable<IConstraintSet> colConstraints, uint maxColor)
        {
            ColorSpace colors = new ColorSpace(maxColor);
            Board board = new Board(rowConstraints, colConstraints, colors);
            return board;
        }

        public static IConstraintSet CreateConstraintSet(IEnumerable<Constraint> constraints)
        {
            return new ConstraintSet(constraints);
        }
    }
}

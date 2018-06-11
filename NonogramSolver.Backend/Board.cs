using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace NonogramSolver.Backend
{
    internal class Board : IBoard
    {
        private BoardState boardState;
        private List<ConstraintList> rowConstraintList;
        private List<ConstraintList> colConstraintList;

        public Board(IEnumerable<IConstraintSet> rowConstraints, IEnumerable<IConstraintSet> colConstraints, ColorSpace colors)
        {
            RowConstraints = rowConstraints.ToList();
            ColumnConstraints = colConstraints.ToList();

            NumRows = RowConstraints.Count;
            NumColumns = ColumnConstraints.Count;
            ColorSpace = colors;

            boardState = new BoardState(this);

            CreateConstraints();
        }

        public int NumRows { get; private set; }
        public int NumColumns { get; private set; }

        public IReadOnlyList<IConstraintSet> RowConstraints { get; private set; }
        public IReadOnlyList<IConstraintSet> ColumnConstraints { get; private set; }

        public IProgressManager ProgressManager => throw new NotImplementedException();

        public ColorSpace ColorSpace { get; private set; }

        public ISolvedBoard Solve()
        {
            if (boardState == null)
            {
                boardState = new BoardState(this);
            }

            SolverLoop();

            ISolvedBoard solved = boardState.ExtractSolvedBoard();
            boardState = null;
            return solved;
        }

        public void OnTileDirty(int row, int col)
        {
            rowConstraintList[row].SetDirty();
            colConstraintList[col].SetDirty();
        }

        private void CreateConstraints()
        {
            rowConstraintList = Enumerable.Range(0, NumRows).Select(i => new ConstraintList(i, true, NumColumns)).ToList();
            colConstraintList = Enumerable.Range(0, NumColumns).Select(i => new ConstraintList(i, false, NumRows)).ToList();

            Debug.Assert(rowConstraintList.Count == NumRows);
            Debug.Assert(colConstraintList.Count == NumColumns);

            SetupConstraintLists(rowConstraintList, RowConstraints);
            SetupConstraintLists(colConstraintList, ColumnConstraints);
        }

        private static void SetupConstraintLists(IReadOnlyList<ConstraintList> lists, IReadOnlyList<IConstraintSet> constraints)
        {
            // Pair each ConstraintList with its corresponding item IConstraintSet
            var pairs = lists.Zip(constraints, (l, s) => new Tuple<ConstraintList, IConstraintSet>(l, s));

            // Flatten the IConstraintSet, so now each contained Constraint is paired with its ConstraintList
            var listConstraintPairs = pairs.SelectMany(t => t.Item2, (t, c) => new Tuple<ConstraintList, Constraint>(t.Item1, c));

            // Add the constraint to its corresponding ConstraintList
            foreach (var tuple in listConstraintPairs)
            {
                tuple.Item1.Add(tuple.Item2);
            }
        }

        private void SolverLoop()
        {
            bool changed = false;
            do
            {
                bool rchange = InnerConstraintLoop(rowConstraintList);
                bool cchange = InnerConstraintLoop(colConstraintList);
                changed = rchange || cchange;
            } while (changed);
        }

        private bool InnerConstraintLoop(IEnumerable<ConstraintList> constraints)
        {
            bool changed = false;
            foreach (ConstraintList constr in constraints)
            {
                if (constr.IsDirty)
                {
                    var colors = constr.CalculateColorSets(boardState);
                    bool change;
                    if (constr.IsRow)
                    {
                        change = boardState.IntersectColorSetsOnRow(constr.Index, colors);
                    }
                    else
                    {
                        change = boardState.IntersectColorSetsOnColumn(constr.Index, colors);
                    }

                    changed = (changed || change);
                }
            }

            return changed; 
        }
    }
}

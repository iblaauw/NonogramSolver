using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace NonogramSolver.Backend
{

    public class UnsolvableBoardException : Exception
    {
        public UnsolvableBoardException() { }
        public UnsolvableBoardException(string message) : base(message) { }
        public UnsolvableBoardException(string message, Exception inner) : base(message, inner) { }
    }

    internal class Board : IBoard
    {
        private BoardManager boardManager;
        private List<ConstraintList> rowConstraintList;
        private List<ConstraintList> colConstraintList;

        public Board(IEnumerable<IConstraintSet> rowConstraints, IEnumerable<IConstraintSet> colConstraints, ColorSpace colors)
        {
            RowConstraints = rowConstraints.ToList();
            ColumnConstraints = colConstraints.ToList();

            NumRows = RowConstraints.Count;
            NumColumns = ColumnConstraints.Count;
            ColorSpace = colors;

            boardManager = new BoardManager(this);

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
            if (boardManager == null)
            {
                boardManager = new BoardManager(this);
            }

            SolverLoop();

            ISolvedBoard solved = boardManager.CurrentLayer.ExtractSolvedBoard();
            boardManager = null;
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
            while (true)
            {
                bool success = OuterConstraintLoop();
                if (!success)
                {
                    // We hit "no solution" for the current layer. Pop it, and push a new one with a different guess
                    DoPopLayer();
                    DoPushLayer();
                    continue;
                }

                if (boardManager.CurrentLayer.CalculateIsSolved())
                    break;

                DoPushLayer();
            }

            //do
            //{
            //    bool success = OuterConstraintLoop();
            //    if (!success)
            //    {
            //        // We hit "no solution" for the current layer. Pop it, and push a new one with another guess (done immediately after this if)
            //        DoPopLayer();
            //    }

            //    DoPushLayer();
            //}
            //while (!boardManager.CurrentLayer.CalculateIsSolved());
        }

        // Returns true if it completes normally, and false if a NoSolution situation was found
        private bool OuterConstraintLoop()
        {
            bool changed = false;
            do
            {
                var rowResult = InnerConstraintLoop(rowConstraintList);
                if (rowResult == BoardState.IntersectResult.NoSolution)
                    return false;

                var colResult = InnerConstraintLoop(colConstraintList);
                if (colResult == BoardState.IntersectResult.NoSolution)
                    return false;

                changed = (rowResult == BoardState.IntersectResult.Changed) || (rowResult == BoardState.IntersectResult.Changed);
            } while (changed);

            return true;
        }

        private BoardState.IntersectResult InnerConstraintLoop(IEnumerable<ConstraintList> constraints)
        {
            bool changed = false;
            foreach (ConstraintList constr in constraints)
            {
                if (constr.IsDirty)
                {
                    BoardState state = boardManager.CurrentLayer;
                    IBoardView boardView = constr.IsRow ? state.CreateRowView(constr.Index) : state.CreateColView(constr.Index);
                    var result = constr.ConstrainBoard(boardView);

                    if (result == BoardState.IntersectResult.NoSolution)
                        return result;

                    if (result == BoardState.IntersectResult.Changed)
                    {
                        changed = true;
                    }
                }
            }

            return changed ? BoardState.IntersectResult.Changed : BoardState.IntersectResult.NoChange; 
        }

        // Returns false if we have exhausted all options
        private void DoPushLayer()
        {
            // TODO: this is a bit aggressive, but not a huge hit to performance
            SetAllConstraintsDirty();

            BoardState boardState = boardManager.CurrentLayer;
            Guesser.Guess guess = boardState.Guesser.GenerateNext(boardState);

            // If guess is null, we've tried everything for the current board state, go back one and try again
            // Repeat this in a loop until we succeed or we've hit the end of our pushed layers
            while (guess == null)
            {
                DoPopLayer();
                boardState = boardManager.CurrentLayer;
                guess = boardState.Guesser.GenerateNext(boardState);
            }

            boardManager.PushLayer();

            // Apply our guess to the new layer
            // (Not the current layer, or we can't undo it...)
            ColorSet guessColor = ColorSet.CreateSingle(guess.Color);
            boardManager.CurrentLayer.SetColor(guess.X, guess.Y, guessColor);
        }

        private void DoPopLayer()
        {
            // Popping a layer should _never_ fail. If it does, then we have exhausted all possibilities
            if (!boardManager.PopLayer())
                throw new UnsolvableBoardException();
        }

        private void SetAllConstraintsDirty()
        {
            foreach (var constr in rowConstraintList)
            {
                constr.SetDirty();
            }

            foreach (var constr in colConstraintList)
            {
                constr.SetDirty();
            }
        }
    }
}

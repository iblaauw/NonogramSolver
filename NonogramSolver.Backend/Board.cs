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

    internal enum ConstrainResult
    {
        Success,
        NoSolution,
    }

    internal class Board : IBoard
    {
        private BoardManager boardManager;
        private List<ConstraintList> rowConstraintList;
        private List<ConstraintList> colConstraintList;

        private readonly Queue<ConstraintList> dirtyConstraints;

        public Board(IEnumerable<IConstraintSet> rowConstraints, IEnumerable<IConstraintSet> colConstraints, ColorSpace colors)
        {
            RowConstraints = rowConstraints.ToList();
            ColumnConstraints = colConstraints.ToList();

            NumRows = RowConstraints.Count;
            NumColumns = ColumnConstraints.Count;
            ColorSpace = colors;

            boardManager = new BoardManager(this);

            dirtyConstraints = new Queue<ConstraintList>();

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
            dirtyConstraints.Enqueue(rowConstraintList[row]);
            dirtyConstraints.Enqueue(colConstraintList[col]);
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
                //bool success = OuterConstraintLoop();

                // Apply constraints as much as possible
                var result = ApplyConstraintsLoop();

                //if (!success)
                // if we are violating a constraint
                if (result == ConstrainResult.NoSolution)
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
        }

        private ConstrainResult ApplyConstraintsLoop()
        {
            while (dirtyConstraints.Count > 0)
            {
                ConstraintList constraint = dirtyConstraints.Dequeue();
                var result = ApplyConstraint(constraint);
                if (result == ConstrainResult.NoSolution)
                    return ConstrainResult.NoSolution;
            }

            return ConstrainResult.Success;
        }

        private ConstrainResult ApplyConstraint(ConstraintList constraint)
        {
            BoardState state = boardManager.CurrentLayer;
            IBoardView boardView = constraint.IsRow ? state.CreateRowView(constraint.Index) : state.CreateColView(constraint.Index);
            return constraint.ConstrainBoard(boardView);
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
                dirtyConstraints.Enqueue(constr);
            }

            foreach (var constr in colConstraintList)
            {
                dirtyConstraints.Enqueue(constr);
            }
        }
    }
}

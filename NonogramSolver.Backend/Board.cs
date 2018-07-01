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
        private const int ENUMERATION_THRESHOLD = 1000;

        private BoardManager boardManager;
        private List<ConstraintListWrapper> rowConstraintList;
        private List<ConstraintListWrapper> colConstraintList;

        private volatile int debugConstraintIndex;
        private volatile bool debugConstraintIsRow;

        private readonly PriorityQueue<long, ConstraintListWrapper> dirtyConstraints;

        public Board(IEnumerable<IConstraintSet> rowConstraints, IEnumerable<IConstraintSet> colConstraints, ColorSpace colors)
        {
            RowConstraints = rowConstraints.ToList();
            ColumnConstraints = colConstraints.ToList();

            NumRows = RowConstraints.Count;
            NumColumns = ColumnConstraints.Count;
            ColorSpace = colors;

            boardManager = new BoardManager(this);

            dirtyConstraints = new PriorityQueue<long, ConstraintListWrapper>();

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
            var rowConstr = rowConstraintList[row];
            if (!rowConstr.IsDirty)
            {
                Enqueue(rowConstr);
            }

            var colConstr = colConstraintList[col];
            if (!colConstr.IsDirty)
            {
                Enqueue(colConstr);
            }
        }

        private void CreateConstraints()
        {
            var rowConstrainers = RowConstraints.Select(c => new Constrainer(c)).ToList();
            var colConstrainers = ColumnConstraints.Select(c => new Constrainer(c)).ToList();
            rowConstraintList = Enumerable.Range(0, NumRows).Select(i => new ConstraintListWrapper(i, true, rowConstrainers[i])).ToList();
            colConstraintList = Enumerable.Range(0, NumColumns).Select(i => new ConstraintListWrapper(i, false, colConstrainers[i])).ToList();

            Debug.Assert(rowConstraintList.Count == NumRows);
            Debug.Assert(colConstraintList.Count == NumColumns);
        }

        private void SolverLoop()
        {
            SetAllConstraintsDirty();
            while (true)
            {
                // Apply constraints as much as possible
                var result = ApplyConstraintsLoop();

                // if we are violating a constraint
                if (result == ConstrainResult.NoSolution)
                {
                    Debug.WriteLine("No Solution hit");

                    // We hit "no solution" for the current layer. Pop it, and push a new one with a different guess
                    DoPopLayer();
                    DoPushLayer();
                    continue;
                }

                Debug.WriteLine("Loop finished");

                if (boardManager.CurrentLayer.CalculateIsSolved())
                    break;

                DoPushLayer();
            }
        }

        private ConstrainResult ApplyConstraintsLoop()
        {
            uint tick = 0;
            uint tickReport = 1;
            while (dirtyConstraints.Count > 0)
            {
                ConstraintListWrapper constraint = dirtyConstraints.Dequeue();
                Debug.Assert(constraint.IsDirty);
                constraint.IsDirty = false;

                debugConstraintIndex = constraint.Index;
                debugConstraintIsRow = constraint.IsRow;

                var result = ApplyConstraint(constraint);
                if (result == ConstrainResult.NoSolution)
                    return ConstrainResult.NoSolution;

                tick++;
                if (tick == tickReport)
                {
                    Debug.WriteLine("TICK " + tick.ToString());
                    tickReport = tickReport << 1;
                }
            }

            return ConstrainResult.Success;
        }

        private ConstrainResult ApplyConstraint(ConstraintListWrapper constraint)
        {
            BoardState state = boardManager.CurrentLayer;
            IBoardView boardView = constraint.CreateBoardView(state);

            long oldCost = boardView.ConstraintState.Cost;
            Debug.Assert(oldCost >= 0);

            constraint.Constraint.CalculateEstimatedCost(boardView);

            long newCost = boardView.ConstraintState.Cost;

            Debug.Assert(newCost <= oldCost);

            if (newCost <= ENUMERATION_THRESHOLD || oldCost == newCost)
            {
                // We've judged that it is cheap enough to brute force hit this, or we don't have any other option
                return constraint.Constraint.ConstrainBoard(boardView);
            }
            else
            {
                constraint.IsDirty = true;
                dirtyConstraints.Enqueue(newCost, constraint);
                return ConstrainResult.Success;
            }
        }

        // Returns false if we have exhausted all options
        private void DoPushLayer()
        {
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

            // TODO: this is pretty inefficient, it's O(n log n) when it could be O(n)
            while (dirtyConstraints.Count > 0)
            {
                var constr = dirtyConstraints.Dequeue();
                constr.IsDirty = false;
            }
        }

        private void SetAllConstraintsDirty()
        {
            BoardState boardState = boardManager.CurrentLayer;
            foreach (var constr in rowConstraintList)
            {
                IBoardView boardView = constr.CreateBoardView(boardState);
                constr.Constraint.CalculateEstimatedCost(boardView);
                Enqueue(constr);
            }

            foreach (var constr in colConstraintList)
            {
                IBoardView boardView = constr.CreateBoardView(boardState);
                constr.Constraint.CalculateEstimatedCost(boardView);
                Enqueue(constr);
            }
        }

        private void Enqueue(ConstraintListWrapper constraint)
        {
            var state = boardManager.CurrentLayer;
            var constraintStates = constraint.IsRow ? state.RowConstraintStates : state.ColConstraintStates;
            long cost = constraintStates[constraint.Index].Cost;

            constraint.IsDirty = true;
            dirtyConstraints.Enqueue(cost, constraint);
        }
    }
}

﻿using System;
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
        private List<Constrainer> rowConstraintList;
        private List<Constrainer> colConstraintList;

        private readonly Queue<Constrainer> dirtyConstraints;

        public Board(IEnumerable<IConstraintSet> rowConstraints, IEnumerable<IConstraintSet> colConstraints, ColorSpace colors)
        {
            RowConstraints = rowConstraints.ToList();
            ColumnConstraints = colConstraints.ToList();

            NumRows = RowConstraints.Count;
            NumColumns = ColumnConstraints.Count;
            ColorSpace = colors;

            boardManager = new BoardManager(this);

            dirtyConstraints = new Queue<Constrainer>();

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
                dirtyConstraints.Enqueue(rowConstr);
                rowConstr.IsDirty = true;
            }

            var colConstr = colConstraintList[col];
            if (!colConstr.IsDirty)
            {
                dirtyConstraints.Enqueue(colConstr);
                colConstr.IsDirty = true;
            }
        }

        private void CreateConstraints()
        {
            rowConstraintList = Enumerable.Range(0, NumRows).Select(i => new Constrainer(i, true, NumColumns, RowConstraints[i])).ToList();
            colConstraintList = Enumerable.Range(0, NumColumns).Select(i => new Constrainer(i, false, NumRows, ColumnConstraints[i])).ToList();

            Debug.Assert(rowConstraintList.Count == NumRows);
            Debug.Assert(colConstraintList.Count == NumColumns);
        }

        private void SolverLoop()
        {
            SetAllConstraintsDirty();
            while (true)
            {
                //bool success = OuterConstraintLoop();

                // Apply constraints as much as possible
                var result = ApplyConstraintsLoop();

                //if (!success)
                // if we are violating a constraint
                if (result == ConstrainResult.NoSolution)
                {
                    //Debug.WriteLine("No Solution hit");

                    // We hit "no solution" for the current layer. Pop it, and push a new one with a different guess
                    DoPopLayer();
                    DoPushLayer();
                    continue;
                }

                //Debug.WriteLine("Loop finished");

                if (boardManager.CurrentLayer.CalculateIsSolved())
                    break;

                DoPushLayer();
            }
        }

        private ConstrainResult ApplyConstraintsLoop()
        {
            //uint tick = 0;
            //uint tickReport = 1;
            while (dirtyConstraints.Count > 0)
            {
                Constrainer constraint = dirtyConstraints.Dequeue();
                Debug.Assert(constraint.IsDirty);
                constraint.IsDirty = false;
                var result = ApplyConstraint(constraint);
                if (result == ConstrainResult.NoSolution)
                    return ConstrainResult.NoSolution;

                //tick++;
                //if (tick == tickReport)
                //{
                //    Debug.WriteLine("TICK " + tick.ToString());
                //    tickReport = tickReport << 1;
                //}
            }

            return ConstrainResult.Success;
        }

        private ConstrainResult ApplyConstraint(Constrainer constraint)
        {
            BoardState state = boardManager.CurrentLayer;
            var allConstraintStates = constraint.IsRow ? state.RowConstraintStates : state.ColConstraintStates;
            IBoardView boardView = constraint.IsRow ? state.CreateRowView(constraint.Index) : state.CreateColView(constraint.Index);
            return constraint.ConstrainBoard(boardView, allConstraintStates[constraint.Index]);
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

            foreach (var constr in dirtyConstraints)
            {
                constr.IsDirty = false;
            }
            dirtyConstraints.Clear();
        }

        private void SetAllConstraintsDirty()
        {
            foreach (var constr in rowConstraintList)
            {
                dirtyConstraints.Enqueue(constr);
                constr.IsDirty = true;
            }

            foreach (var constr in colConstraintList)
            {
                dirtyConstraints.Enqueue(constr);
                constr.IsDirty = true;
            }
        }
    }
}

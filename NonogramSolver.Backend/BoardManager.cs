using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace NonogramSolver.Backend
{
    class BoardManager
    {
        private BoardState currentLayer;
        private readonly Stack<BoardState> previousStates;

        public BoardManager(Board owner)
        {
            currentLayer = new BoardState(owner);
            previousStates = new Stack<BoardState>();
        }

        public BoardState CurrentLayer => currentLayer;

        public void PushLayer()
        {
            BoardState newState = currentLayer.Clone();
            previousStates.Push(currentLayer);
            currentLayer = newState;
        }

        public void PopLayer()
        {
            currentLayer = previousStates.Pop();
        }
    }
}

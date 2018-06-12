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
            BoardState newState = currentLayer.CreateNewLayer();
            previousStates.Push(currentLayer);
            currentLayer = newState;
        }

        /// <summary>
        /// Pops a BoardState layer from the state stack
        /// </summary>
        /// <returns>True if a layer was successfully popped. False if there were no more layers to pop</returns>
        public bool PopLayer()
        {
            if (previousStates.Count == 0)
                return false;

            currentLayer = previousStates.Pop();
            return true;
        }
    }
}

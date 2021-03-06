﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NonogramSolver.Backend;

namespace NonogramSolver.Tests
{
    [TestClass]
    class FullTest
    {

        [TestMethod]
        public void Basic()
        {
            // O O X
            // X X O
            // O O X

            const uint BlackColor = 1;

            List<Constraint> row1 = new List<Constraint>
            {
                new Constraint(BlackColor, 1),
            };
            List<Constraint> row2 = new List<Constraint>
            {
                new Constraint(BlackColor, 2),
            };
            List<Constraint> row3 = new List<Constraint>
            {
                new Constraint(BlackColor, 1),
            };

            var row1Set = BoardFactory.CreateConstraintSet(row1);
            var row2Set = BoardFactory.CreateConstraintSet(row2);
            var row3Set = BoardFactory.CreateConstraintSet(row3);

            List<Constraint> col1 = new List<Constraint>
            {
                new Constraint(BlackColor, 1),
            };
            List<Constraint> col2 = new List<Constraint>
            {
                new Constraint(BlackColor, 1),
            };
            List<Constraint> col3 = new List<Constraint>
            {
                new Constraint(BlackColor, 1),
                new Constraint(BlackColor, 1),
            };

            var col1Set = BoardFactory.CreateConstraintSet(col1);
            var col2Set = BoardFactory.CreateConstraintSet(col2);
            var col3Set = BoardFactory.CreateConstraintSet(col3);

            var rowConstraints = new[] { row1Set, row2Set, row3Set };
            var colConstraints = new[] { col1Set, col2Set, col3Set };

            IBoard board = BoardFactory.CreateBoard(rowConstraints, colConstraints, BlackColor);
            ISolvedBoard solved = board.Solve();

            Assert.True(solved[0, 0] == 0);
            Assert.True(solved[0, 1] == 0);
            Assert.True(solved[0, 2] == BlackColor);

            Assert.True(solved[1, 0] == BlackColor);
            Assert.True(solved[1, 1] == BlackColor);
            Assert.True(solved[1, 2] == 0);

            Assert.True(solved[2, 0] == 0);
            Assert.True(solved[2, 1] == 0);
            Assert.True(solved[2, 2] == BlackColor);
        }

        [TestMethod]
        public void Basic2()
        {
            // X X O R
            // O X R R
            // R R X O
            // R O X X

            const uint BlackColor = 1;
            const uint RedColor = 2;

            Constraint blackConstr = new Constraint(BlackColor, 1);
            Constraint blackConstr2 = new Constraint(BlackColor, 2);
            Constraint redConstr = new Constraint(RedColor, 1);
            Constraint redConstr2 = new Constraint(RedColor, 2);

            var row0 = BoardFactory.CreateConstraintSet(new[] { blackConstr2, redConstr });
            var row1 = BoardFactory.CreateConstraintSet(new[] { blackConstr, redConstr2 });
            var row2 = BoardFactory.CreateConstraintSet(new[] { redConstr2, blackConstr });
            var row3 = BoardFactory.CreateConstraintSet(new[] { redConstr, blackConstr2 });

            var col0 = BoardFactory.CreateConstraintSet(new[] { blackConstr, redConstr2 });
            var col1 = BoardFactory.CreateConstraintSet(new[] { blackConstr2, redConstr });
            var col2 = BoardFactory.CreateConstraintSet(new[] { redConstr, blackConstr2 });
            var col3 = BoardFactory.CreateConstraintSet(new[] { redConstr2, blackConstr });

            var rowConstraints = new[] { row0, row1, row2, row3 };
            var colConstraints = new[] { col0, col1, col2, col3 };

            IBoard board = BoardFactory.CreateBoard(rowConstraints, colConstraints, RedColor);

            ISolvedBoard solvedBoard = board.Solve();

            Assert.True(solvedBoard[0, 0] == BlackColor);
            Assert.True(solvedBoard[0, 1] == BlackColor);
            Assert.True(solvedBoard[0, 2] == 0);
            Assert.True(solvedBoard[0, 3] == RedColor);

            Assert.True(solvedBoard[1, 0] == 0);
            Assert.True(solvedBoard[1, 1] == BlackColor);
            Assert.True(solvedBoard[1, 2] == RedColor);
            Assert.True(solvedBoard[1, 3] == RedColor);

            Assert.True(solvedBoard[2, 0] == RedColor);
            Assert.True(solvedBoard[2, 1] == RedColor);
            Assert.True(solvedBoard[2, 2] == BlackColor);
            Assert.True(solvedBoard[2, 3] == 0);

            Assert.True(solvedBoard[3, 0] == RedColor);
            Assert.True(solvedBoard[3, 1] == 0);
            Assert.True(solvedBoard[3, 2] == BlackColor);
            Assert.True(solvedBoard[3, 3] == BlackColor);
        }

        [TestMethod]
        public void Complex()
        {
            // 0 0 G G G G 0 0
            // 0 G 0 0 0 0 G 0
            // 0 G 0 g g 0 G 0
            // G 0 g g g g 0 G
            // G 0 0 g g 0 0 G
            // 0 G G 0 0 G G 0
            // 0 0 G B B G 0 0
            // 0 0 0 B B 0 0 0
            // 0 0 0 B B 0 0 0

            const uint GreenColor = 1;
            const uint LightGreenColor = 2;
            const uint BrownColor = 3;

            Constraint greenConstr = new Constraint(GreenColor, 1);
            Constraint greenConstr2 = new Constraint(GreenColor, 2);
            Constraint greenConstr4 = new Constraint(GreenColor, 4);

            Constraint lgreenConstr = new Constraint(LightGreenColor, 1);
            Constraint lgreenConstr2 = new Constraint(LightGreenColor, 2);
            Constraint lgreenConstr3 = new Constraint(LightGreenColor, 3);
            Constraint lgreenConstr4 = new Constraint(LightGreenColor, 4);

            Constraint brownConstr2 = new Constraint(BrownColor, 2);
            Constraint brownConstr3 = new Constraint(BrownColor, 3);

            var row1 = BoardFactory.CreateConstraintSet(new[] { greenConstr4 });
            var row2 = BoardFactory.CreateConstraintSet(new[] { greenConstr, greenConstr });
            var row3 = BoardFactory.CreateConstraintSet(new[] { greenConstr, lgreenConstr2, greenConstr });
            var row4 = BoardFactory.CreateConstraintSet(new[] { greenConstr, lgreenConstr4, greenConstr });
            var row5 = BoardFactory.CreateConstraintSet(new[] { greenConstr, lgreenConstr2, greenConstr });
            var row6 = BoardFactory.CreateConstraintSet(new[] { greenConstr2, greenConstr2 });
            var row7 = BoardFactory.CreateConstraintSet(new[] { greenConstr, brownConstr2, greenConstr });
            var row8 = BoardFactory.CreateConstraintSet(new[] { brownConstr2 });
            var row9 = BoardFactory.CreateConstraintSet(new[] { brownConstr2 });

            var col1 = BoardFactory.CreateConstraintSet(new[] { greenConstr2 });
            var col2 = BoardFactory.CreateConstraintSet(new[] { greenConstr2, greenConstr });
            var col3 = BoardFactory.CreateConstraintSet(new[] { greenConstr, lgreenConstr, greenConstr2 });
            var col4 = BoardFactory.CreateConstraintSet(new[] { greenConstr, lgreenConstr3, brownConstr3 });
            var col5 = BoardFactory.CreateConstraintSet(new[] { greenConstr, lgreenConstr3, brownConstr3 });
            var col6 = BoardFactory.CreateConstraintSet(new[] { greenConstr, lgreenConstr, greenConstr2 });
            var col7 = BoardFactory.CreateConstraintSet(new[] { greenConstr2, greenConstr });
            var col8 = BoardFactory.CreateConstraintSet(new[] { greenConstr2 });

            var rows = new[] { row1, row2, row3, row4, row5, row6, row7, row8, row9 };
            var cols = new[] { col1, col2, col3, col4, col5, col6, col7, col8 };
            IBoard board = BoardFactory.CreateBoard(rows, cols, BrownColor);
            ISolvedBoard solved = board.Solve();

            Assert.True(solved[0, 0] == 0);
            Assert.True(solved[0, 1] == 0);
            Assert.True(solved[0, 2] == GreenColor);
            Assert.True(solved[0, 3] == GreenColor);
            Assert.True(solved[0, 4] == GreenColor);
            Assert.True(solved[0, 5] == GreenColor);
            Assert.True(solved[0, 6] == 0);
            Assert.True(solved[0, 7] == 0);

            Assert.True(solved[1, 0] == 0);
            Assert.True(solved[1, 1] == GreenColor);
            Assert.True(solved[1, 2] == 0);
            Assert.True(solved[1, 3] == 0);
            Assert.True(solved[1, 4] == 0);
            Assert.True(solved[1, 5] == 0);
            Assert.True(solved[1, 6] == GreenColor);
            Assert.True(solved[1, 7] == 0);

            Assert.True(solved[2, 0] == 0);
            Assert.True(solved[2, 1] == GreenColor);
            Assert.True(solved[2, 2] == 0);
            Assert.True(solved[2, 3] == LightGreenColor);
            Assert.True(solved[2, 4] == LightGreenColor);
            Assert.True(solved[2, 5] == 0);
            Assert.True(solved[2, 6] == GreenColor);
            Assert.True(solved[2, 7] == 0);

            Assert.True(solved[3, 0] == GreenColor);
            Assert.True(solved[3, 1] == 0);
            Assert.True(solved[3, 2] == LightGreenColor);
            Assert.True(solved[3, 3] == LightGreenColor);
            Assert.True(solved[3, 4] == LightGreenColor);
            Assert.True(solved[3, 5] == LightGreenColor);
            Assert.True(solved[3, 6] == 0);
            Assert.True(solved[3, 7] == GreenColor);

            Assert.True(solved[4, 0] == GreenColor);
            Assert.True(solved[4, 1] == 0);
            Assert.True(solved[4, 2] == 0);
            Assert.True(solved[4, 3] == LightGreenColor);
            Assert.True(solved[4, 4] == LightGreenColor);
            Assert.True(solved[4, 5] == 0);
            Assert.True(solved[4, 6] == 0);
            Assert.True(solved[4, 7] == GreenColor);

            Assert.True(solved[5, 0] == 0);
            Assert.True(solved[5, 1] == GreenColor);
            Assert.True(solved[5, 2] == GreenColor);
            Assert.True(solved[5, 3] == 0);
            Assert.True(solved[5, 4] == 0);
            Assert.True(solved[5, 5] == GreenColor);
            Assert.True(solved[5, 6] == GreenColor);
            Assert.True(solved[5, 7] == 0);

            Assert.True(solved[6, 0] == 0);
            Assert.True(solved[6, 1] == 0);
            Assert.True(solved[6, 2] == GreenColor);
            Assert.True(solved[6, 3] == BrownColor);
            Assert.True(solved[6, 4] == BrownColor);
            Assert.True(solved[6, 5] == GreenColor);
            Assert.True(solved[6, 6] == 0);
            Assert.True(solved[6, 7] == 0);

            Assert.True(solved[7, 0] == 0);
            Assert.True(solved[7, 1] == 0);
            Assert.True(solved[7, 2] == 0);
            Assert.True(solved[7, 3] == BrownColor);
            Assert.True(solved[7, 4] == BrownColor);
            Assert.True(solved[7, 5] == 0);
            Assert.True(solved[7, 6] == 0);
            Assert.True(solved[7, 7] == 0);

            Assert.True(solved[8, 0] == 0);
            Assert.True(solved[8, 1] == 0);
            Assert.True(solved[8, 2] == 0);
            Assert.True(solved[8, 3] == BrownColor);
            Assert.True(solved[8, 4] == BrownColor);
            Assert.True(solved[8, 5] == 0);
            Assert.True(solved[8, 6] == 0);
            Assert.True(solved[8, 7] == 0);
        }

        [TestMethod]
        public void FlagTest()
        {
            const uint W = 0;
            const uint B = 1;
            const uint G = 2;

            uint[,] flagImage = new uint[5, 5]
            {
                { W, W, B, B, B },
                { W, B, B, B, W },
                { W, B, W, G, W },
                { W, G, G, G, W },
                { G, G, G, W, W },
            };

            TestImage(flagImage, G);
        }

        [TestMethod]
        public void LadybugTest()
        {
            const uint W = 0;
            const uint B = 1;
            const uint R = 2;
            const uint G = 3;

            uint[,] ladybugImage = new uint[20, 20]
            {
                {  W, W, W, W, B, B, W, W, W, B, B, B, W, W, W, W, B, B, W, W },
                {  W, W, W, W, W, B, B, B, W, B, W, W, W, W, B, B, B, W, W, W },
                {  W, W, W, W, W, W, W, B, R, R, R, R, B, B, B, W, B, B, W, B },
                {  W, W, W, W, B, R, R, R, R, R, R, R, B, B, B, B, B, B, B, B },
                {  W, W, W, B, R, R, R, R, R, R, R, W, B, B, B, B, B, W, B, W },
                {  W, W, B, R, R, R, R, R, R, R, R, W, W, B, B, B, B, B, B, W },
                {  W, W, B, R, R, R, B, B, R, R, R, B, B, B, B, B, B, B, W, W },
                {  W, B, R, R, R, R, B, B, R, R, R, B, B, B, W, B, B, B, W, W },
                {  W, B, R, R, R, R, R, R, R, R, R, B, B, B, W, W, R, R, W, B },
                {  W, R, R, R, R, R, R, R, R, R, B, R, R, R, R, R, R, R, W, B },
                {  W, R, R, R, R, R, R, R, R, B, R, R, R, R, R, R, R, R, B, B },
                {  B, B, R, B, B, R, R, R, B, R, R, R, R, R, R, R, R, R, W, W },
                {  B, W, R, B, B, R, R, B, R, R, R, R, B, B, R, R, R, B, W, W },
                {  B, G, R, R, R, R, B, R, R, R, R, R, B, B, R, R, R, B, B, W },
                {  W, G, G, R, R, B, R, R, R, R, R, R, R, R, R, R, R, W, B, W },
                {  W, G, G, R, B, R, R, R, B, B, R, R, R, R, R, R, B, W, B, B },
                {  W, G, W, W, R, R, R, R, B, B, R, R, R, R, R, B, W, W, W, W },
                {  W, W, W, W, G, G, R, R, R, R, R, R, R, B, B, W, W, W, W, W },
                {  W, W, W, G, G, G, G, W, B, R, R, B, B, W, W, W, W, W, W, W },
                {  W, W, W, W, W, W, B, B, B, W, W, W, W, W, W, W, W, W, W, W },
            };

            TestImage(ladybugImage, G);
        }

        [TestMethod]
        public void SolvingTest()
        {
            ConstraintHelper Black = new ConstraintHelper(1);
            ConstraintHelper Orange = new ConstraintHelper(2);
            ConstraintHelper LightBlue = new ConstraintHelper(3);
            ConstraintHelper DarkBlue = new ConstraintHelper(4);

            List<IConstraintHelper> rowConstraintHelpers = new List<IConstraintHelper>()
            {
                9*LightBlue + 4*Black + 9*LightBlue,
                5*LightBlue + 3*LightBlue + 3*Black + 5*Black + 7*LightBlue,
                3*LightBlue + 2*LightBlue + 2*Black + 10*Black + 6*LightBlue,
                6*LightBlue + 15*Black + 2*LightBlue,
                3*LightBlue + 17*Black + 5*LightBlue,

                5*LightBlue + 18*Black + 5*LightBlue,
                4*LightBlue + 7*Black + 3*Black + 4*Black + 3*LightBlue,
                6*Black + Black + 3*Black + 3*LightBlue,
                7*Black + 3*Black,
                6*Black + 2*Black + 2*Black + 2*Black,

                6*Black + Black + 2*Black + Black + 2*Black + 2*Black,
                6*Black + Black + Black + Black + Black + 3*Black,
                5*Black + 4*Black + 2*Orange + 2*Black + 3*Black,
                4*Black + Black + 4*Orange + 2*Black,
                4*Black + Black + 3*Orange + 2*Black,
                
                3*Black + Black + Orange + 2*Black,
                9*DarkBlue + 2*Black + 3*Black + 6*DarkBlue,
                10*DarkBlue + 6*Black + 4*Black + 7*DarkBlue,
                6*DarkBlue + 6*Black + 2*Black,
                6*Black + 2*Black + 4*DarkBlue,

                4*DarkBlue + 6*Black + 4*Black + 2*DarkBlue,
                3*DarkBlue + 7*Black + 7*Black,
                3*DarkBlue + 6*Black + 6*Black,
                7*Black + 5*Black + 2*DarkBlue,
                2*DarkBlue + LightBlue + 6*Black + 3*Black,

                3*LightBlue + 6*Black + 2*Black + 2*LightBlue,
                3*LightBlue + 6*Black + Black + 3*LightBlue,
                2*LightBlue + 4*Black + 2*Black + 2*Black + 2*Black + 2*Black + 2*LightBlue,
                LightBlue + 2*Black + 2*Black + Black + Orange + Black + Orange + 2*Black + Black + Orange + Black + LightBlue,
                2*LightBlue + Black + Black + 2*Black + Black + 5*Orange + Black + 3*Orange + Black + 2*LightBlue,

                2*LightBlue + 5*Black + Black + 4*Orange + Black + Black + 4*Orange + Black + 3*LightBlue,
                4*LightBlue + 6*Black + 4*Orange + Black + 2*Black + 3*Orange + Black + 3*LightBlue,
                DarkBlue + 6*LightBlue + 6*Black + 2*Orange + 4*Black + 2*Orange + 2*Black + 3*LightBlue + 2*DarkBlue,
                3*DarkBlue + 6*LightBlue + 4*Black + 2*Orange + Black + 3*Black + 4*LightBlue + 3*DarkBlue,
                4*DarkBlue + 4*LightBlue + 2*Black + 2*LightBlue + 4*DarkBlue,

                6*DarkBlue + 4*LightBlue + LightBlue + 3*DarkBlue,
                5*DarkBlue + 2*LightBlue + 2*LightBlue + 3*DarkBlue,
                3*DarkBlue + 5*LightBlue + 3*LightBlue + 3*DarkBlue,
                4*DarkBlue + 8*LightBlue + 5*LightBlue + 2*DarkBlue,
                6*DarkBlue + 20*LightBlue + 3*DarkBlue,

                8*DarkBlue + 14*LightBlue + 4*DarkBlue,
                7*DarkBlue + 12*LightBlue + 4*DarkBlue,
                10*DarkBlue + 3*LightBlue + 6*DarkBlue,
                19*DarkBlue,
                14*DarkBlue
            };

            Assert.True(rowConstraintHelpers.Count == 45);

            List<IConstraintHelper> colConstraintHelpers = new List<IConstraintHelper>()
            {
                LightBlue + 2*DarkBlue + DarkBlue + DarkBlue + 3*LightBlue + 3*DarkBlue,
                2*LightBlue + LightBlue + 3*DarkBlue + DarkBlue + DarkBlue + DarkBlue + 2*LightBlue + 4*LightBlue + 3*DarkBlue,
                3*LightBlue + LightBlue + 3*DarkBlue + DarkBlue + DarkBlue + LightBlue + 3*LightBlue + 3*DarkBlue,
                2*LightBlue + LightBlue + 2*LightBlue + 3*DarkBlue + 2*DarkBlue + 2*LightBlue + 3*LightBlue + 3*DarkBlue,
                2*LightBlue + 4*LightBlue + 3*DarkBlue + 2*DarkBlue + LightBlue + 4*LightBlue + 2*DarkBlue + DarkBlue,

                2*LightBlue + 4*LightBlue + 3*DarkBlue + DarkBlue + 2*LightBlue + 3*LightBlue + 2*DarkBlue + 2*DarkBlue,
                LightBlue + 2*LightBlue + LightBlue + 3*DarkBlue + 5*Black + 3*LightBlue + 5*DarkBlue,
                LightBlue + 2*LightBlue + 2*DarkBlue + 8*Black + 3*LightBlue + 5*DarkBlue,
                3*LightBlue + 5*Black + 2*DarkBlue + 10*Black + LightBlue + 2*LightBlue + 4*DarkBlue,
                2*LightBlue + 10*Black + DarkBlue + 9*Black + 4*LightBlue + 3*DarkBlue,

                2*LightBlue + 12*Black + 9*Black + 2*Black + LightBlue + 3*LightBlue + 2*DarkBlue,
                LightBlue + 13*Black + 12*Black + 2*Black + 3*LightBlue + 2*DarkBlue + DarkBlue,
                22*Black + 6*Black + 3*LightBlue + 4*DarkBlue,
                10*Black + 6*Black + 4*Black + 3*LightBlue + 5*DarkBlue,
                Black + 6*Black + 3*Black + 4*Black + 2*LightBlue + 5*DarkBlue,

                6*Black + 2*Black + 2*Black + 3*Black + 3*LightBlue + 4*DarkBlue,
                5*Black + Black + Black + Black + 7*Black + 4*LightBlue + 3*DarkBlue,
                Black + 4*Black + 2*Black + Black + Black + 4*Orange + 2*Black + 3*LightBlue + 3*DarkBlue,
                6*Black + 4*Black + Black + 5*Orange + Black + 3*LightBlue + 3*DarkBlue,
                7*Black + Black + Orange + Black + Black + 6*Orange + Black + 3*LightBlue + 3*DarkBlue,

                8*Black + 3*Orange + Black + 2*Black + 3*Orange + 2*Black + 3*LightBlue + 3*DarkBlue,
                6*Black + 2*Black + 4*Orange + Black + Orange + 3*Black + 3*LightBlue + 3*DarkBlue,
                5*Black + Black + Black + 2*Orange + Black + 4*LightBlue + 2*DarkBlue,
                4*Black + 2*Black + Black + 2*Black + 3*Black + 4*LightBlue + 2*DarkBlue,
                5*Black + 2*Black + 4*Black + 2*Black + Orange + Black + 4*LightBlue + 2*DarkBlue,

                LightBlue + 6*Black + 2*Black + 3*Black + Black + 3*Orange + Black + 4*LightBlue + 3*DarkBlue,
                LightBlue + 9*Black + 3*Black + 4*Black + 2*Black + 3*Orange + Black + 4*LightBlue + 3*DarkBlue,
                LightBlue + LightBlue + 11*Black + 8*Black + 4*Orange + Black + 5*LightBlue + 2*DarkBlue,
                4*LightBlue + 4*Black + DarkBlue + 5*Black + Black + 2*Orange + Black + LightBlue + 4*LightBlue + 3*DarkBlue,
                5*LightBlue + 2*DarkBlue + 4*Black + LightBlue + 2*Black + 3*LightBlue + 4*LightBlue + 4*DarkBlue,

                3*LightBlue + 2*LightBlue + LightBlue + 2*DarkBlue + DarkBlue + 3*Black + 2*LightBlue + 6*LightBlue + 6*DarkBlue,
                3*LightBlue + 2*LightBlue + LightBlue + 2*DarkBlue + DarkBlue + 2*Black + LightBlue + 4*LightBlue + 8*DarkBlue,
                3*LightBlue + 4*LightBlue + 2*DarkBlue + DarkBlue + 2*LightBlue + 3*LightBlue + 6*DarkBlue + 2*DarkBlue,
                2*LightBlue + 3*LightBlue + 2*DarkBlue + 2*DarkBlue + DarkBlue + 4*LightBlue + 5*DarkBlue,
                LightBlue + 2*LightBlue + 2*DarkBlue + DarkBlue + DarkBlue + 2*DarkBlue
            };

            Assert.True(colConstraintHelpers.Count == 35);

            var rowConstraints = rowConstraintHelpers.Select(h => h.ToSet());
            var colConstraints = colConstraintHelpers.Select(h => h.ToSet());

            IBoard board = BoardFactory.CreateBoard(rowConstraints, colConstraints, 4);
            rowConstraints = null;
            colConstraints = null;

            ISolvedBoard solved = board.Solve();
            Assert.True(solved != null);
        }

        private static void TestImage(uint[,] image, uint maxColor)
        {
            IBoard board = CreateBoardFromImage(image, maxColor);
            ISolvedBoard solved = board.Solve();
            CompareSolution(image, solved);
        }

        private static IBoard CreateBoardFromImage(uint[,] image, uint maxColor)
        {
            BoardFactory.CreateConstraintsForGrid(image, out IReadOnlyList<IConstraintSet> rows, out IReadOnlyList<IConstraintSet> cols);

            return BoardFactory.CreateBoard(rows, cols, maxColor);
        }

        private static void CompareSolution(uint[,] image, ISolvedBoard solvedBoard)
        {
            int numRows = image.GetLength(0);
            int numCols = image.GetLength(1);

            for (int row = 0; row < numRows; row++)
            {
                for (int col = 0; col < numCols; col++)
                {
                    Assert.True(image[row, col] == solvedBoard[row, col]);
                }
            }
        }

    }
}

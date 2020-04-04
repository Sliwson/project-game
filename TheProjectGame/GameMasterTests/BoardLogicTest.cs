using NUnit.Framework;
using GameMaster;
using System.Drawing;
using System.Collections.Generic;
using System;

namespace GameMasterTests
{
    public class BoardLogicTest
    {
        private GameMaster.GameMaster gameMaster;
        private BoardLogicComponent boardLogicComponent;
        private Point size;

        [SetUp]
        public void Setup()
        {
            gameMaster = new GameMaster.GameMaster();
            boardLogicComponent = gameMaster.BoardLogic;
            size = new Point(gameMaster.Configuration.BoardX, gameMaster.Configuration.BoardY);
        }

        [Test]
        public void BoardLogicComponent_ConstructorShouldInitializeField()
        {
            for (int y = 0; y < size.Y; y++)
                for (int x = 0; x < size.X; x++)
                    Assert.IsNotNull(boardLogicComponent.GetField(new Point(x, y)));
        }

        [Test]
        public void Clean_ShouldCleanAllFields()
        {
            ChangeBoard();
            boardLogicComponent.Clean();
            for (int y = 0; y < size.Y; y++)
                for (int x = 0; x < size.X; x++)
                    Assert.IsTrue(IsFieldClean(boardLogicComponent.GetField(x, y)));
        }

        [Test]
        public void GetPointWhere_ShouldReturnAgentIfExists()
        {
            boardLogicComponent.Clean();
            var field = boardLogicComponent.GetField(4, 4);
            var agent = new Agent(0, Messaging.Enumerators.TeamId.Blue, new Point(4,4));
            field.Agent = agent;

            Assert.AreEqual(new Point(4, 4), boardLogicComponent.GetPointWhere(p => p.Agent == agent).Value);
        }

        [Test]
        public void GetPointWhere_ShouldReturnAllFakeGoalsThatExist()
        {
            boardLogicComponent.Clean();
            ChangeBoard();
            Assert.AreEqual(2, boardLogicComponent.GetPointsWhere(p => p.State == FieldState.FakeGoal).Count);
        }

        [Test]
        public void GetDiscoverArray_ShouldCalculateDistancesCorrectly()
        {
            boardLogicComponent.Clean();
            ChangeBoard();

            int[,] schema = { { 0, 1, 2 }, { 1, 2, 3 }, { 2, 3, 4 } };
            var result = boardLogicComponent.GetDiscoverArray(new Point(1, 1));
            for (int y = 0; y < 3; y++)
                for (int x = 0; x < 3; x++)
                    Assert.AreEqual(schema[y, x], result[y, x]);
        }

        [Test]
        public void IsFieldInGoalArea_ShouldReturnTrueForBlueTeam()
        {
            var goalAreaHeight = gameMaster.Configuration.GoalAreaHeight;
            for (int y = 0; y < goalAreaHeight; y++)
                for (int x = 0; x < size.X; x++)
                    Assert.IsTrue(boardLogicComponent.IsFieldInGoalArea(new Point(x, y)));
        }
        
        [Test]
        public void IsFieldInGoalArea_ShouldReturnTrueForRedTeam()
        {
            var goalAreaHeight = gameMaster.Configuration.GoalAreaHeight;
            for (int y = size.Y - 1; y >= size.Y - goalAreaHeight; y--)
                for (int x = 0; x < size.X; x++)
                    Assert.IsTrue(boardLogicComponent.IsFieldInGoalArea(new Point(x, y)));
        }

        [Test]
        public void IsFieldInTaskArea_ShouldReturnTrueForTaskArea()
        {
            var goalAreaHeight = gameMaster.Configuration.GoalAreaHeight;
            for (int y = goalAreaHeight; y < size.Y - goalAreaHeight; y++)
                for (int x = 0; x < size.X; x++)
                    Assert.IsTrue(boardLogicComponent.IsFieldInTaskArea(new Point(x, y)));
        }

        [Test]
        public void AllFieldsShouldBeCoveredInTaskGoalAreaTests()
        {
            var goalAreaHeight = gameMaster.Configuration.GoalAreaHeight;
            var wasCovered = new bool[size.Y, size.X];

            for (int y = 0; y < goalAreaHeight; y++)
                for (int x = 0; x < size.X; x++)
                    wasCovered[y, x] = true;

            for (int y = size.Y - 1; y >= size.Y - goalAreaHeight; y--)
                for (int x = 0; x < size.X; x++)
                    wasCovered[y, x] = true;

            for (int y = goalAreaHeight; y < size.Y - goalAreaHeight; y++)
                for (int x = 0; x < size.X; x++)
                    wasCovered[y, x] = true;

            for (int y = 0; y < size.Y; y++)
                for (int x = 0; x < size.X; x++)
                    Assert.IsTrue(wasCovered[y, x]);
        }

        private bool IsFieldClean(Field f)
        {
            return f.Agent == null && f.Pieces.Count == 0 && f.State == FieldState.Empty;
        }

        private void ChangeBoard()
        {
            Action<Field, Point> changeField = (Field f, Point position) => {
                f.Agent = new Agent(0, Messaging.Enumerators.TeamId.Blue, position);
                f.State = FieldState.FakeGoal;
                f.Pieces.Push(new Piece(false));
            };

            var pos1 = new Point(0, 0);
            var pos2 = new Point(size.X - 1, size.Y - 1);
            var field1 = boardLogicComponent.GetField(pos1);
            var field2 = boardLogicComponent.GetField(pos2);
            changeField(field1, pos1);
            changeField(field2, pos2);
        }
    }
}
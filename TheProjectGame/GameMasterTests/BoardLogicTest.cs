using NUnit.Framework;
using GameMaster;
using System.Drawing;
using System.Collections.Generic;
using System;

namespace GameMasterTests
{
    public class BoardLogicTest
    {
        private BoardLogicComponent boardLogicComponent;
        readonly Point size = new Point(5, 10);

        [SetUp]
        public void Setup()
        {
            boardLogicComponent = new BoardLogicComponent(size);
        }

        [Test]
        public void BoardIsInitialized()
        {
            for (int y = 0; y < size.Y; y++)
                for (int x = 0; x < size.X; x++)
                    Assert.IsNotNull(boardLogicComponent.GetField(new Point(x, y)));
        }

        [Test]
        public void BoardIsCleaned()
        {
            ChangeBoard();
            boardLogicComponent.Clean();
            for (int y = 0; y < size.Y; y++)
                for (int x = 0; x < size.X; x++)
                    Assert.IsTrue(IsFieldClean(boardLogicComponent.GetField(x, y)));
        }

        [Test]
        public void AgentIsFound()
        {
            boardLogicComponent.Clean();
            var field = boardLogicComponent.GetField(4, 4);
            var agent = new Agent(0, Messaging.Enumerators.TeamId.Blue, new Point(4,4));
            field.Agent = agent;

            Assert.AreEqual(new Point(4, 4), boardLogicComponent.GetPointWhere(p => p.Agent == agent).Value);
        }

        [Test]
        public void FakeGoalsAreFound()
        {
            boardLogicComponent.Clean();
            ChangeBoard();
            Assert.AreEqual(2, boardLogicComponent.GetPointsWhere(p => p.State == FieldState.FakeGoal).Count);
        }

        [Test]
        public void DistancesAreCalculatedCorrectly()
        {
            boardLogicComponent.Clean();
            ChangeBoard();

            int[,] schema = { { 0, 1, 2 }, { 1, 2, 3 }, { 2, 3, 4 } };
            var result = boardLogicComponent.GetDiscoverArray(new Point(1, 1));
            for (int y = 0; y < 3; y++)
                for (int x = 0; x < 3; x++)
                    Assert.AreEqual(schema[y, x], result[y, x]);
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
                f.Pieces.Push(new Piece());
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
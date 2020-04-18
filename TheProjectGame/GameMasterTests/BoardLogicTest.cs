using GameMaster;
using NUnit.Framework;
using System;
using System.Drawing;

namespace GameMasterTests
{
    public class BoardLogicTest
    {
        private GameMaster.GameMaster gameMaster;
        private BoardLogicComponent BoardLogicComponent;
        private Point size;

        [SetUp]
        public void Setup()
        {
            var configurationProvider = new GameMaster.MockConfigurationProvider();
            var Configuration = configurationProvider.GetConfiguration();
            gameMaster = new GameMaster.GameMaster(Configuration);
            BoardLogicComponent = gameMaster.BoardLogic;
            size = new Point(gameMaster.Configuration.BoardX, gameMaster.Configuration.BoardY);
        }

        [Test]
        public void BoardLogicComponent_ConstructorShouldInitializeField()
        {
            for (int y = 0; y < size.Y; y++)
                for (int x = 0; x < size.X; x++)
                    Assert.IsNotNull(BoardLogicComponent.GetField(new Point(x, y)));
        }

        [Test]
        public void Clean_ShouldCleanAllFields()
        {
            ChangeBoard();
            BoardLogicComponent.Clean();
            for (int y = 0; y < size.Y; y++)
                for (int x = 0; x < size.X; x++)
                    Assert.IsTrue(IsFieldClean(BoardLogicComponent.GetField(x, y)));
        }

        [Test]
        public void GetPointWhere_ShouldReturnAgentIfExists()
        {
            BoardLogicComponent.Clean();
            var field = BoardLogicComponent.GetField(4, 4);
            var agent = new Agent(0, Messaging.Enumerators.TeamId.Blue, new Point(4, 4));
            field.Agent = agent;

            Assert.AreEqual(new Point(4, 4), BoardLogicComponent.GetPointWhere(p => p.Agent == agent).Value);
        }

        [Test]
        public void GetDiscoverArray_ShouldCalculateDistancesCorrectly()
        {
            BoardLogicComponent.Clean();
            ChangeBoard();

            int[,] schema = { { 0, 1, 2 }, { 1, 2, 3 }, { 2, 3, 4 } };
            var result = BoardLogicComponent.GetDiscoverArray(new Point(1, 1));
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
                    Assert.IsTrue(BoardLogicComponent.IsFieldInGoalArea(new Point(x, y)));
        }

        [Test]
        public void IsFieldInGoalArea_ShouldReturnTrueForRedTeam()
        {
            var goalAreaHeight = gameMaster.Configuration.GoalAreaHeight;
            for (int y = size.Y - 1; y >= size.Y - goalAreaHeight; y--)
                for (int x = 0; x < size.X; x++)
                    Assert.IsTrue(BoardLogicComponent.IsFieldInGoalArea(new Point(x, y)));
        }

        [Test]
        public void IsFieldInTaskArea_ShouldReturnTrueForTaskArea()
        {
            var goalAreaHeight = gameMaster.Configuration.GoalAreaHeight;
            for (int y = goalAreaHeight; y < size.Y - goalAreaHeight; y++)
                for (int x = 0; x < size.X; x++)
                    Assert.IsTrue(BoardLogicComponent.IsFieldInTaskArea(new Point(x, y)));
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

        [Test]
        public void GenerateBoard_ShouldInitializeAllGoalFields()
        {
            var config = gameMaster.Configuration;

            BoardLogicComponent.Clean();
            BoardLogicComponent.GenerateGoals();

            var goalsCount = BoardLogicComponent.GetPointsWhere(p => p.State == FieldState.Goal).Count;
            Assert.AreEqual(config.NumberOfGoals * 2, goalsCount);
        }

        private bool IsFieldClean(Field f)
        {
            return f.Agent == null && f.Pieces.Count == 0 && f.State == FieldState.Empty;
        }

        private void ChangeBoard()
        {
            Action<Field, Point> changeField = (Field f, Point position) =>
            {
                f.Agent = new Agent(0, Messaging.Enumerators.TeamId.Blue, position);
                f.Pieces.Push(new Piece(false));
            };

            var pos1 = new Point(0, 0);
            var pos2 = new Point(size.X - 1, size.Y - 1);
            var field1 = BoardLogicComponent.GetField(pos1);
            var field2 = BoardLogicComponent.GetField(pos2);
            changeField(field1, pos1);
            changeField(field2, pos2);
        }
    }
}
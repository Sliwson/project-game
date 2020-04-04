using System;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Messaging.Enumerators;

namespace GameMaster
{
    public class BoardLogicComponent
    {
        private Field[,] fields;
        private Point size;
        private int piecesDropped = 0;
        private Random random = new Random();
        private GameMaster gameMaster;
        private double timeFromLastDrop = 0;
        private NLog.Logger logger;

        public BoardLogicComponent(GameMaster gameMaster, Point size)
        {
            fields = new Field[size.Y, size.X];
            for (int y = 0; y < size.Y; y++)
                for (int x = 0; x < size.X; x++)
                    fields[y, x] = new Field();

            this.size = size;
            this.gameMaster = gameMaster;
            logger = gameMaster.Logger.Get();

            piecesDropped = 0;
        }

        public void GenerateGoals()
        {
            logger.Info("[Board] Generating goals");

            var conf = gameMaster.Configuration;

            //blue
            var rectangle = GetGoalAreaRectangle(TeamId.Blue);
            GenerateGoalFieldsInRectangle(rectangle, FieldState.Goal, conf.NumberOfGoals);
            GenerateGoalFieldsInRectangle(rectangle, FieldState.FakeGoal, conf.NumberOfFakeGoals);
            
            //red
            MirrorBlueGoalArea();
        }

        private void GenerateGoalFieldsInRectangle(Rectangle rectangle, FieldState state, int count)
        {
            //TODO: log error on failure
            for (int i = 0; i < count; i++)
            {
                int treshold = 1000;
                while (treshold > 0)
                {
                    var point = GetRandomPointInRectangle(rectangle);
                    if (fields[point.Y, point.X].State == FieldState.Empty)
                    {
                        fields[point.Y, point.X].State = state;
                        break;
                    }

                    treshold--;
                }

                if (treshold == 0)
                    logger.Error("[Board] Cannot generate goal {nr} in rectangle {rectangle}", i, rectangle);
            }
        }
        
        private void MirrorBlueGoalArea()
        {
            var conf = gameMaster.Configuration;
            for (int y = 0; y < conf.GoalAreaHeight; y++)
            {
                for (int x = 0; x < size.X; x++)
                {
                    var y1 = size.Y - y - 1;
                    fields[y1, x].State = fields[y, x].State;
                }
            }
        }

        public void Clean()
        {
            for (int y = 0; y < size.Y; y++)
                for (int x = 0; x < size.X; x++)
                    fields[y, x].Clean();
        }

        public Field GetField(Point point)
        {
            return fields[point.Y, point.X];
        }

        public Field GetField(int x, int y)
        {
            return fields[y, x];
        }

        public void PlaceAgent(Agent a)
        {
            var f = GetField(a.Position);
            if (f.Agent != null)
                logger.Error("[Board] Agent {id} requested placement on field {position}, but it's already occupied", a.Id, a.Position);

            f.Agent = a;
        }

        public Point? GetPointWhere(Func<Field, bool> predicate)
        {
            for (int y = 0; y < size.Y; y++)
                for (int x = 0; x < size.X; x++)
                    if (predicate(fields[y, x]))
                        return new Point(x, y);

            return null;
        }

        public List<Point> GetPointsWhere(Func<Field, bool> predicate)
        {
            var list = new List<Point>();
            for (int y = 0; y < size.Y; y++)
                for (int x = 0; x < size.X; x++)
                    if (predicate(fields[y, x]))
                        list.Add(new Point(x, y));

            return list;
        }

        public int[,] GetDiscoverArray(Point field)
        {
            int[,] tab = new int[3, 3];
            for (int y = field.Y - 1; y <= field.Y + 1; y++)
            {
                for (int x = field.X - 1; x <= field.X + 1; x++)
                {
                    int taby = y - field.Y + 1;
                    int tabx = x - field.X + 1;

                    if (!IsPointOnBoard(x, y))
                        tab[taby, tabx] = -1;        
                    else
                        tab[taby, tabx] = CalculateDistanceToNearestPiece(new Point(x, y));
                }
            }

            return tab;
        }

        public bool CanMove(Agent agent, Direction direction)
        {
            var agentPoint = GetPointWhere(f => f.Agent == agent);
            if (!agentPoint.HasValue)
            {
                logger.Error("[Board] Agent {id} requested move, but is not placed on board", agent.Id);
                return false;
            }

            var newPosition = GetPointInDirection(agentPoint.Value, direction);
            if (IsPointOnBoard(newPosition) && GetField(newPosition).Agent == null && !IsFieldInOppositeGoalArea(agent.Team, newPosition))
                return true;
            else
                return false;
        }

        public Point GetRandomPositionForAgent(TeamId team)
        {
            var area = GetGoalAreaRectangle(team);
            int treshold = 1000;
            while (treshold > 0)
            {
                var point = GetRandomPointInRectangle(area);
                if (fields[point.Y, point.X].Agent == null)
                    return point;

                treshold--;
            }

            return new Point(-1, -1);
        }

        public void Update(double dt)
        {
            var conf = gameMaster.Configuration;
            timeFromLastDrop += dt;
            if (timeFromLastDrop > conf.GeneratePieceDelay.TotalSeconds)
                DropPiece();
        }

        private void DropPiece()
        {
            if (piecesDropped >= gameMaster.Configuration.NumberOfPieces)
                return;

            timeFromLastDrop = 0;
            var point = GetRandomPointInRectangle(GetGameAreaRectangle());
            var isSham = random.NextDouble() < gameMaster.Configuration.ShamProbability;
            var piece = new Piece(isSham);
            fields[point.Y, point.X].Pieces.Push(piece);
            piecesDropped++;

            logger.Info("[Board] Piece {number} dropped (field={field}, sham={shamValue})", piecesDropped, point, isSham);
        }

        public void MoveAgent(Agent agent, Direction direction)
        {
            var field = GetField(agent.Position);
            field.Agent = null;
            var newPoint = GetPointInDirection(agent.Position, direction);
            var newField = GetField(newPoint);
            
            logger.Info("[Board] Agent {number} moved from {from} to {to}", agent.Id, agent.Position, newPoint); 
           
            newField.Agent = agent;
            agent.Position = newPoint;
        }

        public int CalculateDistanceToNearestPiece(Point from)
        {
            var pieces = GetPointsWhere(f => { return f.Pieces.Count != 0; });
            if (pieces.Count == 0)
                return -1;

            return pieces.Min(p => GetDistance(p, from));
        }

        public bool IsFieldInGoalArea(Point position)
        {
            var goalSize = gameMaster.Configuration.GoalAreaHeight;
            return position.Y < goalSize || position.Y >= size.Y - goalSize;
        }

        public bool IsFieldInTaskArea(Point position)
        {
            return !IsFieldInGoalArea(position);
        }

        private int GetDistance(Point p1, Point p2)
        {
            return Math.Abs(p1.X - p2.X) +  Math.Abs(p1.Y - p2.Y);
        }

        private Point GetPointInDirection(Point p, Direction direction)
        {
            switch (direction)
            {
                case Direction.East:
                    return new Point(p.X + 1, p.Y);
                case Direction.North:
                    return new Point(p.X, p.Y + 1);
                case Direction.South:
                    return new Point(p.X, p.Y - 1);
                case Direction.West:
                    return new Point(p.X - 1, p.Y);
            }

            return new Point(p.X, p.Y);
        }

        private bool IsFieldInOppositeGoalArea(TeamId agentTeam, Point position)
        {
            var goalSize = gameMaster.Configuration.GoalAreaHeight;
            if (agentTeam == TeamId.Blue)
                return position.Y >= size.Y - goalSize;
            else
                return position.Y < goalSize;
        }

        private bool IsPointOnBoard(int x, int y)
        {
            return x >= 0 && x < size.X && y >= 0 && y < size.Y;
        }

        private bool IsPointOnBoard(Point p)
        {
            return IsPointOnBoard(p.X, p.Y);
        }

        private Point GetRandomPointInRectangle(Rectangle r)
        {
            var x = random.Next(r.Left, r.Right);
            var y = random.Next(r.Top, r.Bottom);
            return new Point(x, y);
        }

        private Rectangle GetGoalAreaRectangle(TeamId team)
        {
            var config = gameMaster.Configuration;
            if (team == TeamId.Blue)
                return new Rectangle(0, 0, size.X, config.GoalAreaHeight);
            else
                return new Rectangle(0, size.Y - config.GoalAreaHeight, size.X, config.GoalAreaHeight);
        }
        
        private Rectangle GetGameAreaRectangle()
        {
            var config = gameMaster.Configuration;
            return new Rectangle(0, config.GoalAreaHeight, size.X, size.Y - 2 * config.GoalAreaHeight);
        }
    }
}

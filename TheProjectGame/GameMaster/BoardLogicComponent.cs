using System;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace GameMaster
{
    public class BoardLogicComponent
    {
        private Field[,] fields;
        private Point size;

        public BoardLogicComponent(Point size)
        {
            fields = new Field[size.Y, size.X];
            for (int y = 0; y < size.Y; y++)
                for (int x = 0; x < size.X; x++)
                    fields[y, x] = new Field();

            this.size = size;
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

        private int CalculateDistanceToNearestPiece(Point from)
        {
            var pieces = GetPointsWhere(f => { return f.Pieces.Count != 0; });
            if (pieces.Count == 0)
                return -1;

            return pieces.Min(p => GetDistance(p, from));
        }

        private int GetDistance(Point p1, Point p2)
        {
            return Math.Abs(p1.X - p2.X) +  Math.Abs(p1.Y - p2.Y);
        }

        private bool IsPointOnBoard(int x, int y)
        {
            return x >= 0 && x < size.X && y >= 0 && y < size.Y;
        }

        private bool IsPointOnBoard(Point p)
        {
            return IsPointOnBoard(p.X, p.Y);
        }
    }
}

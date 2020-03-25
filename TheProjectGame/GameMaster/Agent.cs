using System;
using Messaging.Enumerators;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace GameMaster
{
    public class Agent
    {
        public int Id { get; private set; }
        public TeamId Team { get; private set; }
        public bool IsTeamLeader { get; private set; } = false;
        public double Timeout { get; private set; } = 0;
        public Piece Piece { get; private set; } = null;
        public Point Position { get; private set; }

        public Agent(int id, TeamId team, Point position, bool isTeamLeader = false)
        {
            Id = id;
            Team = team;
            Position = position;
            IsTeamLeader = isTeamLeader;
        }

        public void Update(double dt)
        {
            Timeout = Math.Max(Timeout - dt, 0.0);
        }

        public void PickUpPiece(Piece p)
        {
            Piece = p;
        }

        public Piece RemovePiece()
        {
            var p = Piece;
            Piece = null;
            return p;
        }

        public bool CanPerformAction()
        {
            return Timeout <= 0;
        }
    }
}

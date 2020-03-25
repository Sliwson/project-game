using System;
using Messaging.Enumerators;
using System.Collections.Generic;
using System.Text;

namespace GameMaster
{
    public class Agent
    {
        public int Id { get; private set; }
        public TeamId Team { get; private set; }
        public bool IsTeamLeader { get; private set; } = false;
        public double Timeout { get; private set; } = 0;
        public Piece Piece { get; private set; } = null;

        public Agent(int id, TeamId team, bool isTeamLeader = false)
        {
            Id = id;
            Team = team;
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

using System;
using Messaging.Enumerators;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;

namespace GameMaster
{
    public class Agent
    {
        public int Id { get; private set; }
        public TeamId Team { get; private set; }
        public Point Position { get; set; }
        public bool IsTeamLeader { get; private set; } = false;
        public double Timeout { get; private set; } = 0;
        public Piece Piece { get; private set; } = null;

        private List<ContactRequest> contactRequests = new List<ContactRequest>();

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

        public void AddTimeout(double value)
        {
            Timeout += value;
        }

        public void InformationExchangeRequested(int targetId, bool teamLeader)
        {
            contactRequests.Add(new ContactRequest { Id = targetId, TeamLeader = teamLeader });
        }

        public void InformationExchangePerformed(int targetId)
        {
            var found = contactRequests.Find(r => r.Id == targetId);
            if (found != null)
                contactRequests.Remove(found);
        }

        public bool HaveToExchange()
        {
            return contactRequests.Any(r => r.TeamLeader);
        }

        public bool CanExchange(int targetId)
        {
            var found = contactRequests.Find(r => r.Id == targetId);
            if (found == null)
                return false;

            if (HaveToExchange() && found.TeamLeader == false)
                return false;

            return true;
        }

        private class ContactRequest
        {
            public int Id { get; set; }
            public bool TeamLeader { get; set; }
        }
    }
}

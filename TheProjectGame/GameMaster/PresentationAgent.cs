using Messaging.Enumerators;
using System.Drawing;

namespace GameMaster
{
    public class PresentationAgent
    {
        public int Id { get; private set; }
        public TeamId Team { get; private set; }
        public Point Position { get; private set; }
        public bool IsTeamLeader { get; private set; }
        public bool HasPiece { get; private set; }

        public PresentationAgent(int id, TeamId team, Point position, bool isTeamLeader, bool hasPiece)
        {
            Id = id;
            Team = team;
            Position = position;
            IsTeamLeader = isTeamLeader;
            HasPiece = hasPiece;
        }

        public PresentationAgent(Agent agent)
        {
            Id = agent.Id;
            Team = agent.Team;
            Position = agent.Position;
            IsTeamLeader = agent.IsTeamLeader;
            HasPiece = agent.Piece != null;
        }
    }
}
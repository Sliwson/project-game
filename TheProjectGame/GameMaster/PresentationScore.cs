using GameMaster.Enums;
using Messaging.Enumerators;

namespace GameMaster
{
    public class PresentationScore
    {
        public int RedTeamScore { get; private set; }
        public int BlueTeamScore { get; private set; }
        public GameResult GameResult { get; private set; }

        public PresentationScore(ScoreComponent score)
        {
            RedTeamScore = score.GetScore(TeamId.Red);
            BlueTeamScore = score.GetScore(TeamId.Blue);
            GameResult = score.GetGameResult();
        }
    }
}
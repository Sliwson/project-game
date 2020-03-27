using System;
using System.Collections.Generic;
using System.Text;
using GameMaster.Enums;
using Messaging.Enumerators;

namespace GameMaster
{
    public class ScoreComponent
    {
        private GameMaster gameMaster;
        private Dictionary<TeamId, int> scores = new Dictionary<TeamId, int>();
        private int scoreTarget = 0;

        public ScoreComponent(GameMaster gameMaster)
        {
            this.gameMaster = gameMaster;
            Reset(gameMaster.Configuration.NumberOfGoals);
        }

        public void Reset(int scoreTarget)
        {
            this.scoreTarget = scoreTarget;
            scores.Add(TeamId.Blue, 0);
            scores.Add(TeamId.Red, 0);
        }

        public void TeamScored(TeamId team)
        {
            scores[team] += 1;
        }

        public int GetScore(TeamId team)
        {
            return scores[team];
        }

        public GameResult GetGameResult()
        {
            if (scores[TeamId.Blue] >= scoreTarget && scores[TeamId.Red] >= scoreTarget)
                return GameResult.Draw;
            else if (scores[TeamId.Blue] >= scoreTarget)
                return GameResult.BlueWin;
            else if (scores[TeamId.Red] >= scoreTarget)
                return GameResult.RedWin;
            else
                return GameResult.None;
        }
    }
}

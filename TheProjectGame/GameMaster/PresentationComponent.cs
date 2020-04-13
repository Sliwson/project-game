using System.Collections.Generic;

namespace GameMaster
{
    public class PresentationComponent
    {
        private GameMaster gameMaster;

        public PresentationComponent(GameMaster gameMaster)
        {
            this.gameMaster = gameMaster;
        }

        public PresentationData GetPresentationData()
        {
            return new PresentationData(GetPresentationScore(), GetPresentationFields(), GetPresentationAgents());
        }

        private PresentationScore GetPresentationScore()
        {
            return new PresentationScore(gameMaster.ScoreComponent);
        }

        private PresentationField[,] GetPresentationFields()
        {
            var result = new PresentationField[gameMaster.Configuration.BoardY, gameMaster.Configuration.BoardX];
            for (int y = 0; y < result.GetLength(0); y++)
            {
                for (int x = 0; x < result.GetLength(1); x++)
                {
                    result[y, x] = new PresentationField(gameMaster.BoardLogic.GetField(x, y));
                }
            }
            return result;
        }

        private List<PresentationAgent> GetPresentationAgents()
        {
            var result = new List<PresentationAgent>();
            foreach (var agent in gameMaster.Agents)
            {
                result.Add(new PresentationAgent(agent));
            }
            return result;
        }
    }
}
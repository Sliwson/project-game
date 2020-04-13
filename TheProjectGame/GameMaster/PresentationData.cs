using System.Collections.Generic;

namespace GameMaster
{
    public class PresentationData
    {
        public PresentationScore Score { get; private set; }
        public PresentationField[,] BoardFields { get; private set; }
        public List<PresentationAgent> Agents { get; private set; }

        public PresentationData(PresentationScore score, PresentationField[,] fields, List<PresentationAgent> agents)
        {
            Score = score;
            BoardFields = fields;
            Agents = agents;
        }
    }
}
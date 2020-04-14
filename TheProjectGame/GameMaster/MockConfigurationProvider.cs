using GameMaster.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameMaster
{
    class MockConfigurationProvider : IConfigurationProvider
    {
        public GameMasterConfiguration GetConfiguration()
        {
            return new GameMasterConfiguration
            {
                MovePenalty = new TimeSpan(1500),
                InformationExchangePenalty = new TimeSpan(1000),
                DiscoveryPenalty = new TimeSpan(700),
                PutPenalty = new TimeSpan(500),
                CheckForShamPenalty = new TimeSpan(1000),
                BoardX = 40,
                BoardY = 40,
                GoalAreaHeight = 5,
                NumberOfGoals = 10,
                NumberOfPieces = 20,
                GeneratePieceDelay = new TimeSpan(5000),
                ShamProbability = 0.3f,
                AgentsLimit = 5,
                DestroyPiecePenalty = new TimeSpan(700)
            };
        }
    }
}

using Messaging.Enumerators;
using System;
using System.Collections.Generic;
using System.Text;

namespace Messaging.Responses
{
    public class ExchangeInformationResponse : IResponsePayload
    {
        public int RespondToId { get; set; }
        public int[,] Distances { get; set; }
        public GoalInformation[,] RedTeamGoalAreaInformation { get; set; }
        public GoalInformation[,] BlueTeamGoalAreaInformation { get; set; }
    }
}

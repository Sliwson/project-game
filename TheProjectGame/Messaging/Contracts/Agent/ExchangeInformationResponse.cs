using Messaging.Enumerators;

namespace Messaging.Contracts.Agent
{
    public class ExchangeInformationResponse : IPayload
    {
        public MessageId GetMessageId() => MessageId.ExchangeInformationResponse;

        public int RespondToId { get; private set; }
        public int[,] Distances { get; private set; }
        public GoalInformation[,] RedTeamGoalAreaInformation { get; private set; }
        public GoalInformation[,] BlueTeamGoalAreaInformation { get; private set; }

        public ExchangeInformationResponse(int respondToId, int[,] distances, GoalInformation[,] redTeamGoalAreaInformation, GoalInformation[,] blueTeamGoalAreaInformation)
        {
            RespondToId = respondToId;
            Distances = distances;
            RedTeamGoalAreaInformation = redTeamGoalAreaInformation;
            BlueTeamGoalAreaInformation = blueTeamGoalAreaInformation;
        }
    }
}

namespace Messaging.Enumerators
{
    // Important: ALL these types must be required in penalties in StartGamePayload
    // It means you should not add anything here unless you know it is necessary
    public enum ActionType
    {
        Move,
        CheckForSham,
        Discovery,
        DestroyPiece,
        PutPiece,
        InformationExchange
    }

    public static class ActionTypeExtensions
    {
        public static string ToJsonPropertyName(this ActionType actionType)
        {
            var actionTypeString = actionType.ToString();
            return char.ToLower(actionTypeString[0]) + actionTypeString.Substring(1);
        }
    }
}

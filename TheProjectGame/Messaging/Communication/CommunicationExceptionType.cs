namespace Messaging.Communication
{
    public enum CommunicationExceptionType
    {
        SocketNotCreated,
        InvalidConfigFile,
        NoConfig,
        NoGameMaster,
        NoClient,
        InvalidSocket,
        SocketInUse,
        DuplicatedGameMaster,
        DuplicatedHostId,
        InvalidEndpoint,
        NoIpAddress,
        InvalidMessageSize,
        GameMasterDisconnected
    }

    public static class CommunicationExceptionTypeExtensions
    {
        internal static string GetMessage(this CommunicationExceptionType exceptionType)
        {
            switch (exceptionType)
            {
                case CommunicationExceptionType.SocketNotCreated:
                    return "Unable to create socket";
                case CommunicationExceptionType.InvalidConfigFile:
                    return "Invalid config file for Communication Server";
                case CommunicationExceptionType.NoConfig:
                    return "No config loaded for Communication Server";
                case CommunicationExceptionType.NoGameMaster:
                    return "Game Master has not connected to Communication Server yet";
                case CommunicationExceptionType.NoClient:
                    return "No client exists for requested host id";
                case CommunicationExceptionType.InvalidSocket:
                    return "Connection has already been closed or socket is invalid";
                case CommunicationExceptionType.SocketInUse:
                    return "Requested socket is already in use for other host id";
                case CommunicationExceptionType.DuplicatedGameMaster:
                    return "Game Master is already connected";
                case CommunicationExceptionType.DuplicatedHostId:
                    return "This host id is already in use";
                case CommunicationExceptionType.InvalidEndpoint:
                    return "Supplied IP Address or port is invalid";
                case CommunicationExceptionType.NoIpAddress:
                    return "No network adapters with an IPv4 address found in the system";
                case CommunicationExceptionType.InvalidMessageSize:
                    return "Requested message has invalid size (empty or too long)";
                case CommunicationExceptionType.GameMasterDisconnected:
                    return "Game Master has been disconnected";
            }

            return "";
        }
    }
}

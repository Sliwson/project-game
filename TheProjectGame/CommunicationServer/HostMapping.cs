using Messaging.Communication;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;

namespace CommunicationServer
{
    internal class HostMapping
    {
        private ConcurrentDictionary<int, Socket> mapping;
        private ConcurrentDictionary<Socket, int> inversedMapping;

        private int lastHostId = 0;
        private int gmHostId = 0;

        private static Logger logger = LogManager.GetCurrentClassLogger(); 

        internal HostMapping()
        {
            mapping = new ConcurrentDictionary<int, Socket>();
            inversedMapping = new ConcurrentDictionary<Socket, int>();
        }

        internal int GetHostIdForSocket(Socket socket)
        {
            if (socket != null && inversedMapping.TryGetValue(socket, out int hostId))
                return hostId;

            throw new CommunicationErrorException(CommunicationExceptionType.NoClient);
        }

        internal Socket GetSocketForHostId(int hostId)
        {
            if(mapping.TryGetValue(hostId, out Socket result) && result != null)
                return result;

            throw new CommunicationErrorException(CommunicationExceptionType.NoClient);
        }

        internal int AddClientToMapping(ClientType clientType, Socket socket)
        {
            if(socket == null)
                throw new CommunicationErrorException(CommunicationExceptionType.InvalidSocket);

            var hostId = clientType == ClientType.Agent ? ++lastHostId : gmHostId;

            if (inversedMapping.ContainsKey(socket))
                throw new CommunicationErrorException(CommunicationExceptionType.SocketInUse);
            else if (hostId == gmHostId && mapping.ContainsKey(gmHostId))
                throw new CommunicationErrorException(CommunicationExceptionType.DuplicatedGameMaster);

            if (!mapping.TryAdd(hostId, socket))
                throw new CommunicationErrorException(CommunicationExceptionType.DuplicatedHostId);

            inversedMapping.TryAdd(socket, hostId);

            logger.Info("Client of type {clientType} has been registered", clientType);
            return hostId;
        }

        internal bool IsHostGameMaster(int hostId)
        {
            return hostId == gmHostId;
        }

        internal int GetGameMasterHostId()
        {
            if (!mapping.TryGetValue(gmHostId, out Socket gmSocket) || gmSocket == null)
                throw new CommunicationErrorException(CommunicationExceptionType.NoGameMaster);

            return gmHostId;
        }
    }
}

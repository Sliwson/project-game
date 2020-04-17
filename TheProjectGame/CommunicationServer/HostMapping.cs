using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace CommunicationServer
{
    internal class HostMapping
    {
        private ConcurrentDictionary<int, Socket> mapping;
        private ConcurrentDictionary<Socket, int> inversedMapping;

        private int lastHostId = 0;
        private int gmHostId = 0;

        internal HostMapping()
        {
            mapping = new ConcurrentDictionary<int, Socket>();
            inversedMapping = new ConcurrentDictionary<Socket, int>();
        }

        internal int GetHostIdForSocket(Socket socket)
        {
            if (inversedMapping.TryGetValue(socket, out int result))
                return result;

            throw new KeyNotFoundException($"No host binded with requested socket");
        }

        internal Socket GetSocketForHostId(int hostId)
        {
            if(mapping.TryGetValue(hostId, out Socket result))
                return result;

            throw new KeyNotFoundException($"No socket found for host with ID: {hostId}");
        }

        internal int AddClientToMapping(ClientType clientType, Socket socket)
        {
            var hostId = clientType == ClientType.Agent ? ++lastHostId : gmHostId;

            if (inversedMapping.ContainsKey(socket))
                throw new ArgumentException($"There is already a host binded with requested socket");
            else if (hostId == gmHostId && mapping.ContainsKey(gmHostId))
                throw new ArgumentException($"Game Master is already registered");

            string errorMessage = "Unable to register socket";

            if (!mapping.TryAdd(hostId, socket))
            {
                throw new Exception(errorMessage);
            }
            if (!inversedMapping.TryAdd(socket, hostId))
            {
                while (!mapping.TryRemove(hostId, out _))
                    Console.WriteLine(errorMessage);

                throw new Exception(errorMessage);
            }

            return hostId;
        }
    }
}

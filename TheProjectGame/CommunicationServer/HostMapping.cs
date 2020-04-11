using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace CommunicationServer
{
    internal class HostMapping
    {
        private Dictionary<int, IPEndPoint> mapping;
        private Dictionary<IPEndPoint, int> inversedMapping;

        private int lastHostId = 0;

        internal HostMapping()
        {
            mapping = new Dictionary<int, IPEndPoint>();
            inversedMapping = new Dictionary<IPEndPoint, int>();
        }

        internal int GetHostIdForAddress(IPEndPoint ipAddress)
        {
            if (inversedMapping.TryGetValue(ipAddress, out int result))
                return result;

            throw new KeyNotFoundException($"No host was found for IP address: {ipAddress}");
        }

        internal IPEndPoint GetAddressForHostId(int hostId)
        {
            if(mapping.TryGetValue(hostId, out IPEndPoint result))
                return result;

            throw new KeyNotFoundException($"No address found for host with ID: {hostId}");
        }

        internal void AddAddressToMapping(IPEndPoint ipAddress)
        {
            if (mapping.ContainsValue(ipAddress))
                throw new ArgumentException($"There is already a host with address: {ipAddress}");

            lastHostId++;

            mapping.Add(lastHostId, ipAddress);
            inversedMapping.Add(ipAddress, lastHostId);
        }
    }
}

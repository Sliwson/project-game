using Messaging.Contracts.GameMaster;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Messaging.Serialization.Extensions
{
    internal static class DiscoverResponseJObjectExtensions
    {
        private enum DiscoveryDirection
        {
            FromCurrent,
            N,
            NE,
            E,
            SE,
            S,
            SW,
            W,
            NW
        }

        internal static JObject ToJObject(this DiscoverResponse payload)
        {
            JObject result = new JObject();
            foreach (DiscoveryDirection direction in Enum.GetValues(typeof(DiscoveryDirection)))
            {
                GetCoordinatesFromDirection(direction, out int y, out int x);
                if (payload.Distances[y, x] == -1)
                    continue;

                var propertyName = "distance" + direction.ToString();
                if (!result.TryAdd(propertyName, payload.Distances[y, x]))
                    throw new JsonSerializationException($"Duplicated property \"{propertyName}\" found in serialized object");
            }

            return result;
        }

        internal static DiscoverResponse ToDiscoverResponse(this JObject jObject)
        {
            var resultDistances = new int[3, 3];

            foreach (DiscoveryDirection direction in Enum.GetValues(typeof(DiscoveryDirection)))
            {
                GetCoordinatesFromDirection(direction, out int y, out int x);

                var propertyName = "distance" + direction.ToString();
                var property = jObject.Property(propertyName);
                if (property == null)
                    resultDistances[y, x] = -1;
                else if (!int.TryParse(property.Value.ToString(), out int value))
                    throw new JsonSerializationException($"No valid property \"{propertyName}\" found in serialized object");
                else
                    resultDistances[y, x] = value;
                    
            }

            return new DiscoverResponse(resultDistances);
        }


        private static void GetCoordinatesFromDirection(DiscoveryDirection direction, out int y, out int x)
        {
            switch (direction)
            {
                case DiscoveryDirection.FromCurrent:
                    y = 1;
                    x = 1;
                    break;
                case DiscoveryDirection.N:
                    y = 2;
                    x = 1;
                    break;
                case DiscoveryDirection.NE:
                    y = 2;
                    x = 2;
                    break;
                case DiscoveryDirection.E:
                    y = 1;
                    x = 2;
                    break;
                case DiscoveryDirection.SE:
                    y = 0;
                    x = 2;
                    break;
                case DiscoveryDirection.S:
                    y = 0;
                    x = 1;
                    break;
                case DiscoveryDirection.SW:
                    y = 0;
                    x = 0;
                    break;
                case DiscoveryDirection.W:
                    y = 1;
                    x = 0;
                    break;
                case DiscoveryDirection.NW:
                    y = 2;
                    x = 0;
                    break;
                default:
                    y = 0;
                    x = 0;
                    break;
            }
        }
    }
}

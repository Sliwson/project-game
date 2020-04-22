using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Messaging.Enumerators
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Direction
    {
        [EnumMember(Value = "N")]
        North,

        [EnumMember(Value = "E")]
        East,

        [EnumMember(Value = "S")]
        South,

        [EnumMember(Value = "W")]
        West
    }

    public static class DirectionExtensions
    {
        public static Direction GetOppositeDirection(this Direction direction)
        {
            switch (direction)
            {
                case Direction.North:
                    return Direction.South;
                case Direction.South:
                    return Direction.North;
                case Direction.West:
                    return Direction.East;
                case Direction.East:
                    return Direction.West;
            }
            return Direction.North;
        }

        public static Direction GetNextDirection(this Direction direction)
        {
            switch (direction)
            {
                case Direction.North:
                    return Direction.East;
                case Direction.South:
                    return Direction.West;
                case Direction.West:
                    return Direction.North;
                case Direction.East:
                    return Direction.South;
            }
            return Direction.North;
        }
    }
}

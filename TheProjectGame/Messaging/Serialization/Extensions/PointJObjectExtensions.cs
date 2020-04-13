using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Drawing;

namespace Messaging.Serialization.Extensions
{
    internal static class PointJObjectExtensions
    {
        internal static JObject ToJObject(this Point point)
        {
            return new JObject(
                new JProperty("x", point.X),
                new JProperty("y", point.Y));
        }

        internal static Point ToPoint(this JObject jObject)
        {
            int resultX, resultY;

            var xProperty = jObject.Property("x");
            if (xProperty == null || !int.TryParse(xProperty.Value.ToString(), out resultX))
                throw new JsonSerializationException("No valid property \"x\" found in serialized object");

            var yProperty = jObject.Property("y");
            if (yProperty == null || !int.TryParse(yProperty.Value.ToString(), out resultY))
                throw new JsonSerializationException("No valid property \"x\" found in serialized object");

            return new Point(resultX, resultY);
        }
    }
}

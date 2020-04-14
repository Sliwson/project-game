using Newtonsoft.Json;

namespace Messaging.Contracts
{
    public class NumberOfPlayers
    {
        [JsonRequired]
        [JsonProperty(PropertyName = "allies")]
        public int Allies { get; set; }

        [JsonRequired]
        [JsonProperty(PropertyName = "enemies")]
        public int Enemies { get; set; }
    }
}

namespace GameMasterPresentation.Configuration
{
    public static class ConfigurationSchema
    {
        public static string GetConfigurationSchema()
        {
            string configurationSchema = 
                @"{
                    ""$schema"": ""http://json-schema.org/draft-04/schema"",
                    ""type"": ""object"",
                    ""required"":[""CsIP"", ""CsPort"", ""movePenalty"", ""askPenalty"",
                        ""discoveryPenalty"", ""putPenalty"", ""checkForShamPenalty"",
                        ""destroyPiecePenalty"" ""boardX"", ""boardY"", ""goalAreaHight"",
                        ""numberOfGoals"", ""numberOfPieces"", ""shamPieceProbability"" ],
                    ""properties"": {
                        ""CsIP"": { ""type"": ""string"" },
                        ""CsPort"": { ""type"": ""integer"" },
                        ""movePenalty"": { ""type"": ""integer"" },
                        ""informationExchangePenalty"": { ""type"": ""integer"" },
                        ""discoveryPenalty"": { ""type"": ""integer"" },
                        ""putPenalty"": { ""type"": ""integer"" },
                        ""checkForShamPenalty"": { ""type"": ""integer"" },
                        ""destroyPiecePenalty"": { ""type"": ""integer"" },
                        ""boardX"": { ""type"": ""integer"" },
                        ""boardY"": { ""type"": ""integer"" },
                        ""goalAreaHeight"": { ""type"": ""integer"" },
                        ""numberOfGoals"": { ""type"": ""integer"" },
                        ""numberOfPieces"": { ""type"": ""integer"" },
                        ""teamSize"": { ""type"": ""integer"" },
                        ""shamPieceProbability"": { ""type"":""number"" }
                    }
                }";
            return configurationSchema;
        }
    }
}
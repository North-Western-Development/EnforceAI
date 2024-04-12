using System.Collections.Generic;
using Newtonsoft.Json;

namespace EnforceAI.Server;

public class FirstNames
{
    [JsonProperty] public List<string> Male { get; set; }
    [JsonProperty] public List<string> Female { get; set; }
    [JsonProperty] public List<string> Neutral { get; set; }
}
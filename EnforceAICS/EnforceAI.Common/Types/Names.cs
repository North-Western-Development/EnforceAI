using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace EnforceAI.Server;

public class Names
{
    [JsonProperty] public FirstNames FirstNames { get; set; }
    [JsonProperty] public List<string> LastNames { get; set; }

    public override string ToString()
    {
        StringBuilder stringified = new StringBuilder();
        
        stringified.AppendLine("First Names:");
        stringified.AppendLine($"  Male:");
        foreach (var name in FirstNames.Male)
        {
            stringified.AppendLine($"    {name}");
        }
        stringified.AppendLine($"  Female:");
        foreach (var name in FirstNames.Female)
        {
            stringified.AppendLine($"    {name}");
        }
        stringified.AppendLine($"  Neutral:");
        foreach (var name in FirstNames.Neutral)
        {
            stringified.AppendLine($"    {name}");
        }
        stringified.AppendLine("Last Names:");
        foreach (var name in LastNames)
        {
            stringified.AppendLine($"  {name}");
        }

        return stringified.ToString();
    }
}
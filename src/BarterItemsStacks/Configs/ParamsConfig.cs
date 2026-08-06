using System.Text.Json.Serialization;

namespace BarterItemsStacks.Configs;

public class ParamsConfig
{
    public const string FileName = "params.json";
    
    [JsonInclude]
    private float? WeightLimitMultiplier;
    
    [JsonIgnore]
    public float WeightMultiplier => Gt1(WeightLimitMultiplier ?? 1);

    [JsonIgnore]
    public float? WeightLimitRaw => WeightLimitMultiplier;
    
    private static float Gt1(float v) => v < 1 ? 1 : v;
}
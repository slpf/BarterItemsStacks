using System.Text.Json.Serialization;

namespace BarterItemsStacks.Configs;

public class ItemsConfig
{
    public const string FileName = "config.jsonc";

    public Dictionary<string, ItemRule> Items { get; set; } = new();

    public sealed class ItemRule
    {
        [JsonInclude]
        private int? StackSize;

        [JsonInclude]
        private int? MaxResource;

        [JsonInclude]
        private int? ItemHeight;

        [JsonInclude]
        private int? ItemWidth;

        [JsonInclude]
        private double? WeightMultiplier;

        [JsonInclude]
        private double? PriceMultiplier;

        [JsonIgnore]
        public int Stack => Gt0(StackSize ?? 0);

        [JsonIgnore]
        public int Resource => Gt0(MaxResource ?? 0);

        [JsonIgnore]
        public int Height => Gt0(ItemHeight ?? 0);

        [JsonIgnore]
        public int Width => Gt0(ItemWidth ?? 0);

        [JsonIgnore]
        public double Weight => Gt0(WeightMultiplier ?? 0);

        [JsonIgnore]
        public double Price => Gt0(PriceMultiplier ?? 0);

        private static int Gt0(int v) => v < 0 ? 0 : v;

        private static double Gt0(double v) => v < 0 ? 0 : v;

        private static int Clamp(int v, int min, int max)
            => v < min ? min : (v > max ? max : v);
    }
}

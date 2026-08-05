using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Request;
using System.Text.Json.Serialization;

namespace BarterItemsStacks.PlantOneTripwire
{
    public record PlantOneTripwireModel : BaseInteractionRequestData
    {
        [JsonPropertyName("tripwire")]
        public MongoId? Tripwire { get; set; }

        [JsonPropertyName("plantingKit")]
        public MongoId? PlantingKit { get; set; }
    }
}

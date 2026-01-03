using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Request;
using System.Text.Json.Serialization;

namespace BarterItemsStacks.RemoveOneFromStack
{
    public record  RemoveOneFromStackModel : BaseInteractionRequestData
    {
        [JsonPropertyName("item")]
        public MongoId? Item { get; set; }
    }
}

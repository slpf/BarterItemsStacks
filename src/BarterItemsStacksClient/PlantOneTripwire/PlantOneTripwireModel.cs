using Newtonsoft.Json;
using System;
using EFT.InventoryLogic.Operations;

namespace BarterItemsStacksClient.PlantOneTripwire
{
    [Serializable]
    public class PlantOneTripwireModel(string tripwire, string plantingKit) : BaseInventoryCommand
    {
        public string Action = "PlantOneTripwire";

        [JsonProperty("tripwire")]
        public string Tripwire = tripwire;

        [JsonProperty("plantingKit")]
        public string PlantingKit = plantingKit;

        public override bool Queued => false;
    }
}

using Newtonsoft.Json;
using System;

namespace BarterItemsStacksClient.PlantOneTripwire
{
    [Serializable]
    public class PlantOneTripwireModel : GClass3471
    {
        public string Action = "PlantOneTripwire";

        [JsonProperty("tripwire")]
        public string Tripwire;

        [JsonProperty("plantingKit")]
        public string PlantingKit;

        public PlantOneTripwireModel(string tripwire, string plantingKit)
        {
            Tripwire = tripwire;
            PlantingKit = plantingKit;
        }

        public override bool Queued
        {
            get
            {
                return false;
            }
        }
    }
}

using Newtonsoft.Json;
using System;
using EFT.InventoryLogic.Operations;

namespace BarterItemsStacksClient.RemoveOneFromStack
{
    [Serializable]
    public class RemoveOneFromStackModel(string item) : BaseInventoryCommand
    {
        public string Action = "RemoveOneFromStack";

        [JsonProperty("item")]
        public string Item = item;

        public override bool Queued
        {
            get
            {
                return false;
            }
        }
    }
}

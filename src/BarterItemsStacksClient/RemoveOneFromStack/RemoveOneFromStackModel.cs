using Newtonsoft.Json;
using System;

namespace BarterItemsStacksClient.RemoveOneFromStack
{
    [Serializable]
    public class RemoveOneFromStackModel : GClass3471
    {
        public string Action = "RemoveOneFromStack";

        [JsonProperty("item")]
        public string Item;

        public RemoveOneFromStackModel(string item)
        {
            Item = item;
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

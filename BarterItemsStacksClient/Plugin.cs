using BarterItemsStacksClient.Patches;
using BepInEx;
using BepInEx.Logging;
using System.Collections.Generic;

namespace BarterItemsStacksClient
{
    [BepInPlugin("com.slpf.barteritemsstacks", "BarterItemsStacksClient", "1.1.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource LogSource;
        public static HashSet<string> StackItemsIDs { get; private set; } = new HashSet<string>();

        private void Awake()
        {
            LogSource = Logger;

            new UpdateItemViewPatch().Enable();
            //new MergePatch().Enable();
            new HideoutMethod23Patch().Enable();
            new HideoutMethod21Patch().Enable();
        }
    }

}


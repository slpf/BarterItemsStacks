using BarterItemsStacksClient.Patches;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BarterItemsStacksClient
{
    [BepInPlugin("com.slpf.barteritemsstacks", "BarterItemsStacksClient", "1.2.1")]
    [BepInDependency("com.lacyway.mc", BepInDependency.DependencyFlags.SoftDependency)]
    public class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource LogSource;
        public static HashSet<string> StackItemsIDs { get; private set; } = new HashSet<string>();

        private void Awake()
        {
            LogSource = Logger;

            new UpdateItemViewPatch().Enable();
            new MergePatch().Enable();
            new TransferMaxPatch().Enable();
            new HideoutMethod23Patch().Enable();
            new HideoutMethod21Patch().Enable();
            new COCCheckCompatibilityPatch().Enable();
            new CanApplyItemPatch().Enable();
            new RepaitKitStackUsePatch().Enable();
            new PlaceItemTriggerPatch().Enable();
            new ConvertOperationResultToOperationPatch().Enable();

            if (Chainloader.PluginInfos.ContainsKey("com.lacyway.mc"))
            {
                new ExecutePossibleActionPatch().Enable();
            }
        }
    }
}

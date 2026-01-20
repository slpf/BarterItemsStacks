using BepInEx;
using BepInEx.Logging;
using System.Collections.Generic;
using BarterItemsStacksClient.Patches;
using BarterItemsStacksClient.Patches.Compatibility;
using BarterItemsStacksClient.Patches.Hideout;
using BarterItemsStacksClient.Patches.Interactions;
using BarterItemsStacksClient.Patches.Quest;
using BarterItemsStacksClient.Patches.UIGridItemView;

namespace BarterItemsStacksClient
{
    [BepInPlugin("com.slpf.barteritemsstacks", "BarterItemsStacksClient", "1.3.1")]
    [BepInDependency("com.lacyway.mc", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.tyfon.uifixes", BepInDependency.DependencyFlags.SoftDependency)]
    public class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource LogSource;

        private void Awake()
        {
            LogSource = Logger;
            
            Settings.Init(Config);

            new UpdateItemViewPatch().Enable();
            new MergePatch().Enable();
            new TransferMaxPatch().Enable();
            new HideoutMethod23Patch().Enable();
            new HideoutMethod21Patch().Enable();
            new COCCheckCompatibilityPatch().Enable();
            new CanApplyItemPatch().Enable();
            new RepaitKitStackUsePatch().Enable();
            new PlaceItemTriggerPatch().Enable();
            new PlaceItemProtectPatch().Enable();
            new ConvertOperationResultToOperationPatch().Enable();

            if (HarmonyLib.AccessTools.TypeByName("MergeConsumables.Patches.ExecutePossibleAction_Patch") != null)
            {
                new MCExecutePossibleActionPatch().Enable();
            }

            if (HarmonyLib.AccessTools.TypeByName("UIFixes.SortPatches+StackFirstPatch") != null)
            {
                new UIFixesStackAllPatch().Enable();
            }
            
            if (HarmonyLib.AccessTools.TypeByName("StashManagementHelper.Helpers.ItemManager") != null)
            {
                new StashManagementHelperMergePatch().Enable();
            }
        }
    }
}

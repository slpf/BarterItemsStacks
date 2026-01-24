using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;

namespace BarterItemsStacksClient.Patches.Interactions
{
    internal class MergePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(InteractionsHandlerClass), nameof(InteractionsHandlerClass.Merge));
        }

        [PatchPrefix]
        public static bool Prefix(InteractionsHandlerClass __instance, Item item, Item targetItem, TraderControllerClass itemController, bool simulate, ref GStruct154<GClass3417> __result)
        {
            if (!Utils.CanMergeResources(item, targetItem))
            {
                __result = new GClass1522("Cannot merge items with different resource values");
                return false;
            }

            if (item.SpawnedInSession == targetItem.SpawnedInSession)
            {
                return true;
            }

            if (Utils.CanIgnoreFirStatus(item, targetItem))
            {
                return true;
            }

            __result = new GClass1522("Cannot merge FIR and non-FIR items");
            return false;
        }
    }
}
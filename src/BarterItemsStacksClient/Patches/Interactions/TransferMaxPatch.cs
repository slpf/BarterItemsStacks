using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;

namespace BarterItemsStacksClient.Patches.Interactions
{
    internal class TransferMaxPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(InteractionsHandlerClass), nameof(InteractionsHandlerClass.TransferMax));
        }

        [PatchPrefix]
        public static bool Prefix(InteractionsHandlerClass __instance, Item item, Item targetItem, int count, TraderControllerClass itemController, bool simulate, ref GStruct154<GClass3425> __result)
        {
            if (!Utils.CanMergeResources(item, targetItem))
            {
                __result = new GClass1522("Cannot transfer items with different resource values");
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

            __result = new GClass1522("Cannot transfer FIR and non-FIR items");
            return false;
        }
    }
}

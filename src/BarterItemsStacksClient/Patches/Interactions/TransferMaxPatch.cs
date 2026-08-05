using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;
using Diz.LanguageExtensions;

namespace BarterItemsStacksClient.Patches.Interactions
{
    internal class TransferMaxPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(ItemManipulator), nameof(ItemManipulator.TransferMax));
        }

        [PatchPrefix]
        public static bool Prefix(Item item, Item targetItem, int count, ItemController itemController, bool simulate, ref OperationResult<TransferResult> __result)
        {
            if (!Utils.CanMergeResources(item, targetItem))
            {
                __result = new StringError("Cannot transfer items with different resource values");
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

            __result = new StringError("Cannot transfer FIR and non-FIR items");
            return false;
        }
    }
}

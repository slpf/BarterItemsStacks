using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;
using Diz.LanguageExtensions;

namespace BarterItemsStacksClient.Patches.Interactions
{
    internal class MergePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(ItemManipulator), nameof(ItemManipulator.Merge));
        }

        [PatchPrefix]
        public static bool Prefix(Item item, Item targetItem, ItemController itemController, bool simulate, ref OperationResult<MergeResult> __result)
        {
            if (!Utils.CanMergeResources(item, targetItem))
            {
                __result = new StringError("Cannot merge items with different resource values");
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

            __result = new StringError("Cannot merge FIR and non-FIR items");
            return false;
        }
    }
}
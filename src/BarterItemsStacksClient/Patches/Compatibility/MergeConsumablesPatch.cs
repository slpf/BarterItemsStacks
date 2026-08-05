using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;

namespace BarterItemsStacksClient.Patches.Compatibility
{
    internal class MergeConsumablesPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(AccessTools.TypeByName("MergeConsumables.Patches.ExecutePossibleAction_Patch"), "Prefix");
        }

        [PatchPrefix]
        [HarmonyPriority(Priority.First)]
        private static bool Prefix(ItemContext itemContext, Item targetItem, ref bool __result)
        {
            if (itemContext.Item.StackObjectsCount > 1 || targetItem.StackObjectsCount > 1 || Utils.CanMergeResources(itemContext.Item, targetItem))
            {
                __result = true;
                return false;
            }

            return true;
        }
    }
}

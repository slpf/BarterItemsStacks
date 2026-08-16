using System.Collections.Generic;
using System.Reflection;
using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;
using InvContainer = EFT.InventoryLogic.IContainer;

namespace BarterItemsStacksClient.Patches.Interactions
{
    public class ContainerStackPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(InteractionsHandlerClass), "smethod_0");
        }

        [PatchPrefix]
        public static bool Prefix(IEnumerable<InvContainer> containersToPut, Item itemToMerge,
            ref StackableItemItemClass mergeableItem, int overrideCount, ref bool __result)
        {
            Item found = null;
            __result = Utils.FindStackForMerge(containersToPut, itemToMerge, out found, overrideCount);
            mergeableItem = found as StackableItemItemClass;

            if (mergeableItem == null)
            {
                __result = false;
            }

            return false;
        }
    }
}

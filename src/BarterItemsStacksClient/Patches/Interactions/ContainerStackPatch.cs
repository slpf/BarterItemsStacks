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
            ref Item mergeableItem, int overrideCount, ref bool __result)
        {
            __result = Utils.FindStackForMerge(containersToPut, itemToMerge, out mergeableItem, overrideCount);
            return false;
        }
    }
}

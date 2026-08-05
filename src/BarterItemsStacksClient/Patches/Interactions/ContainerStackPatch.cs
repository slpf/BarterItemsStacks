using System.Collections.Generic;
using System.Reflection;
using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace BarterItemsStacksClient.Patches.Interactions;

public class ContainerStackPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(ItemManipulator), nameof(ItemManipulator.TryFindMergeableItem));
    }

    [PatchPrefix]
    public static bool Prefix(IEnumerable<IContainer> containersToPut, Item itemToMerge,
        ref Item mergeableItem, int overrideCount, ref bool __result)
    {
        __result = Utils.FindStackForMerge(containersToPut, itemToMerge, out mergeableItem, overrideCount);
        return false;
    }
}
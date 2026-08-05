using System.Reflection;
using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace BarterItemsStacksClient.Patches.Interactions;

public class TopUpStackPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(Item), nameof(Item.IsSameItem));
    }

    [PatchPrefix]
    public static bool Prefix(Item __instance, Item other, ref bool __result)
    {
        bool ignoreFir = Utils.CanIgnoreFirStatus(__instance, other);

        __result = __instance.TemplateId == other.TemplateId && __instance.Id != other.Id && (ignoreFir || __instance.SpawnedInSession == other.SpawnedInSession);
        return false;
    }
}
using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;

namespace BarterItemsStacksClient.Patches.Hideout
{
    internal class CircleCheckCompatibilityPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(HideoutAreaContainer.HideoutAreaGrid), nameof(HideoutAreaContainer.HideoutAreaGrid.CheckCompatibility));
        }

        [PatchPostfix]
        public static void Prefix(HideoutAreaContainer.HideoutAreaGrid __instance, Item item, ref bool __result)
        {
            if (__instance.ID.Contains("CircleOfCultists") && item.StackObjectsCount > 1)
            {
                __result = false;
            }
        }
    }
}

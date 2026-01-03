using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;

namespace BarterItemsStacksClient.Patches.Hideout
{
    internal class COCCheckCompatibilityPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(HideoutAreaContainerItemClass.Class2311), nameof(HideoutAreaContainerItemClass.Class2311.CheckCompatibility));
        }

        [PatchPostfix]
        public static void Prefix(HideoutAreaContainerItemClass.Class2311 __instance, Item item, ref bool __result)
        {
            if ((__instance.ID.Contains("CircleOfCultists") && item.StackObjectsCount > 1))
            {
                __result = false;
            }
        }
    }
}

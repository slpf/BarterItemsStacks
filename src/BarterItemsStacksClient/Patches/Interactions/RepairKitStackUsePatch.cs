using EFT;
using Comfort.Common;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;
using EFT.InventoryLogic;
using EFT.UI;

namespace BarterItemsStacksClient.Patches.Interactions
{
    internal class RepairKitStackUsePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(ItemContextInteractionsSwitcher), nameof(ItemContextInteractionsSwitcher.CanModifyItem));
        }

        [PatchPrefix]
        public static bool Prefix(ItemContextInteractionsSwitcher __instance, out FailedResult result, ref bool __result)
        {
            if (__instance._itemContext is RepairItemContext && ((RepairItemContext)__instance._itemContext).RepairKit.StackObjectsCount > 1)
            {
                result = new FailedResult("You can't do this to this item".Localized(), 0);
                __result = false;
                return false;
            }

            result = null;
            return true;
        }
    }
}

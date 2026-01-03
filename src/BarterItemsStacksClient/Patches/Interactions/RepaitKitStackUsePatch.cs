using Comfort.Common;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;

namespace BarterItemsStacksClient.Patches.Interactions
{
    internal class RepaitKitStackUsePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(ContextInteractionSwitcherClass), nameof(ContextInteractionSwitcherClass.method_1));
        }

        [PatchPrefix]
        public static bool Prefix(ContextInteractionSwitcherClass __instance, out FailedResult result, ref bool __result)
        {
            if (__instance.ItemContextAbstractClass is GClass3462 && ((GClass3462)__instance.ItemContextAbstractClass).RepairKit.StackObjectsCount > 1)
            {
                result = new FailedResult("You can't do this to this item".Localized(null), 0);
                __result = false;
                return false;
            }

            result = null;
            return true;
        }
    }
}

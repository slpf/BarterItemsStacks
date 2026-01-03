using Comfort.Common;
using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;

namespace BarterItemsStacksClient.Patches.Interactions
{
    internal class CanApplyItemPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GClass3009<HealthControllerClass.GClass3015>), nameof(GClass3009<HealthControllerClass.GClass3015>.HasPartsToApply));
        }

        [PatchPrefix]
        public static bool Prefix(GClass3009<HealthControllerClass.GClass3015> __instance, Item item, ref IResult __result)
        {
            if (item.StackObjectsCount > 1)
            {
                var fComp = item.GetItemComponent<FoodDrinkComponent>();
                var mComp = item.GetItemComponent<MedKitComponent>();

                if ((mComp == null && fComp == null) || (fComp != null && fComp.MaxResource == 1))
                {
                    return true;
                }

                __result = new FailedResult("Inventory/IncompatibleItem", 0);

                return false;
            }


            return true;
        }
    }
}

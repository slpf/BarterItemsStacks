using Comfort.Common;
using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;
using EFT.HealthSystem;

namespace BarterItemsStacksClient.Patches.Interactions
{
    internal class CanApplyItemPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BaseHealthController<OfflineHealthController.Effect>), nameof(BaseHealthController<>.HasPartsToApply));
        }

        [PatchPrefix]
        public static bool Prefix(BaseHealthController<OfflineHealthController.Effect> __instance, Item item, ref IResult __result)
        {
            if (item.StackObjectsCount > 1)
            {
                FoodDrinkComponent fComp = item.GetItemComponent<FoodDrinkComponent>();
                MedKitComponent mComp = item.GetItemComponent<MedKitComponent>();

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

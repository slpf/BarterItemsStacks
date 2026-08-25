using EFT;
using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;

namespace BarterItemsStacksClient.Patches.Interactions
{
    internal class SplitDiscoveryExperiencePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(SplitResult), nameof(SplitResult.RaiseEvents));
        }

        [PatchPrefix]
        private static void Prefix(SplitResult __instance, IItemOwner controller, CommandStatus status)
        {
            if (status != CommandStatus.Succeed
                || controller.SearchController is not ActiveSearchController searchController)
            {
                return;
            }

            searchController.SetItemAsKnown(__instance.ResultItem, false);
        }
    }
}

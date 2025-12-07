using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;

namespace BarterItemsStacksClient.Patches
{
    internal class MergePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(InteractionsHandlerClass), nameof(InteractionsHandlerClass.Merge));
        }

        [PatchPrefix]
        public static bool Prefix(InteractionsHandlerClass __instance, Item item, Item targetItem, TraderControllerClass itemController, bool simulate)
        {
            if (item.SpawnedInSession != targetItem.SpawnedInSession)
            {
                return false;
            }

            var itemResource = item.GetItemComponent<ResourceComponent>();
            var targetResource = targetItem.GetItemComponent<ResourceComponent>();

            return itemResource is null || targetResource is null || itemResource.Value == targetResource.Value;
        }
    }
}

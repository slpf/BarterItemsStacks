using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using EFT.InventoryLogic.Operations;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;

namespace BarterItemsStacksClient.Patches.Interactions
{
    internal class SplitLootExperiencePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(
                typeof(Player.PlayerInventoryController),
                nameof(Player.PlayerInventoryController.InProcess),
                new[]
                {
                    typeof(ItemController),
                    typeof(Item),
                    typeof(ItemAddress),
                    typeof(bool),
                    typeof(IInventoryOperation),
                    typeof(Callback)
                });
        }

        [PatchPrefix]
        private static void Prefix(
            Player.PlayerInventoryController __instance,
            ItemController executor,
            Item item,
            ItemAddress to,
            bool succeed,
            IInventoryOperation operation)
        {
            if (!succeed
                || item == null
                || to == null
                || operation is not ISubOperation { BaseInventoryOperation: SplitOperation }
                || !executor.CheckTransferOwners(item, to, out _)
                || __instance.Player?.StatisticsManager is not BaseStatisticsManager statisticsManager)
            {
                return;
            }

            foreach (Item splitItem in item.GetAllItems())
            {
                statisticsManager._lootedItemsIds.Add(splitItem.Id);
            }
        }
    }
}

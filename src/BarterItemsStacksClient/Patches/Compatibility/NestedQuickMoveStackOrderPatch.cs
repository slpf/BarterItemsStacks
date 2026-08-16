using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using InvContainer = EFT.InventoryLogic.IContainer;

namespace BarterItemsStacksClient.Patches.Compatibility
{
    public class NestedQuickMoveStackOrderPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(InteractionsHandlerClass), nameof(InteractionsHandlerClass.QuickFindAppropriatePlace));
        }

        [PatchPrefix]
        [HarmonyPriority(Priority.Low)]
        [HarmonyAfter("QuickFindPlacePatch")]
        public static void Prefix(Item item, ref IEnumerable<CompoundItem> targets)
        {
            if (item == null || targets == null || item.StackMaxSize <= 1)
            {
                return;
            }

            List<CompoundItem> list = targets.ToList();
            if (list.Count <= 1)
            {
                return;
            }

            List<CompoundItem> withMerge = new List<CompoundItem>();
            List<CompoundItem> withoutMerge = new List<CompoundItem>();

            foreach (CompoundItem container in list)
            {
                bool has = ContainsMergeable(container, item);

                if (has)
                {
                    withMerge.Add(container);
                }
                else
                {
                    withoutMerge.Add(container);
                }
            }

            if (withMerge.Count > 0 && withoutMerge.Count > 0)
            {
                targets = withMerge.Concat(withoutMerge).ToList();
            }
        }

        private static IEnumerable<InvContainer> GetContainers(CompoundItem compoundItem, Item item)
        {
            if (compoundItem is StashItemClass stash)
            {
                return stash.Grids;
            }

            if (compoundItem is InventoryEquipment equipment)
            {
                return equipment.GetPrioritizedContainersForLoot(item);
            }

            return compoundItem.Containers;
        }

        private static bool ContainsMergeable(CompoundItem container, Item item)
        {
            IEnumerable<InvContainer> containers = GetContainers(container, item);
            return containers != null && Utils.FindStackForMerge(containers, item, out _);
        }
    }
}

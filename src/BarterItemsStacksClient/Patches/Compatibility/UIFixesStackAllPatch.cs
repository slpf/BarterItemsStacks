using System.Collections.Generic;
using System.Linq;
using Diz.LanguageExtensions;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;
using System.Threading.Tasks;
using EFT.InventoryLogic;

namespace BarterItemsStacksClient.Patches.Compatibility
{
    internal class UIFixesStackAllPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(AccessTools.TypeByName("UIFixes.SortPatches+StackFirstPatch"), "StackAll");
        }

        [PatchPrefix]
        private static bool Prefix(CompoundItem compoundItem, InventoryController inventoryController, ref Task<Error> __result)
        {
            __result = StackAll(compoundItem, inventoryController);
            return false;
        }

        private static async Task<Error> StackAll(CompoundItem compoundItem, InventoryController inventoryController)
        {
            Error error = null;
            
            var mergeableItems = compoundItem.Grids.SelectMany(g => g.Items)
                .Where(i => i.StackObjectsCount < i.StackMaxSize && Utils.IsFullResource(i))
                .Reverse()
                .ToArray();

            foreach (Item item in mergeableItems)
            {
                if (item.StackObjectsCount == 0 || item.StackObjectsCount == item.StackMaxSize)
                {
                    continue;
                }

                if (FindStackForMerge(compoundItem.Grids, item, out Item targetItem))
                {
                    var operation = InteractionsHandlerClass.TransferOrMerge(item, targetItem, inventoryController, true);
                    if (operation.Succeeded)
                    {
                        await inventoryController.TryRunNetworkTransaction(operation);
                    }
                    else
                    {
                        error = operation.Error;
                    }
                }
            }

            return error;
        }

        private static bool FindStackForMerge(IEnumerable<StashGridClass> grids, Item itemToMerge, out Item mergeableItem)
        {
            int minimumStackSpace = itemToMerge.StackObjectsCount;
            bool ignoreFir = Utils.CanIgnoreFirStatus(itemToMerge, itemToMerge);

            mergeableItem = grids.SelectMany(x => x.Items)
                .Where(x => x != itemToMerge)
                .Where(x => x.TemplateId == itemToMerge.TemplateId)
                .Where(x => ignoreFir || x.SpawnedInSession == itemToMerge.SpawnedInSession)
                .Where(x => x.StackObjectsCount < x.StackMaxSize)
                .Where(Utils.IsFullResource)
                .FirstOrDefault(x => minimumStackSpace <= x.StackMaxSize - x.StackObjectsCount);

            return mergeableItem != null;
        }
    }
}

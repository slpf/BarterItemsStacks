using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Diz.LanguageExtensions;
using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace BarterItemsStacksClient.Patches.Compatibility;

public class AdvancedStashSortingPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(AccessTools.TypeByName("AdvancedStashSorting.Patches.SortPreparationPatch"), "StackGroup");
    }

    [PatchPrefix]
    private static bool Prefix(List<Item> group, InventoryController inventoryController,
        List<IOperationResult> stagedOperations, bool runNetworkTransactions, ref Task<Error> __result)
    {
        __result = StackGroupFixed(group, inventoryController, stagedOperations, runNetworkTransactions);
        return false;
    }

    private static async Task<Error> StackGroupFixed(List<Item> group, InventoryController inventoryController,
        List<IOperationResult> stagedOperations, bool runNetworkTransactions)
    {
        List<Item> stacks = new List<Item>();

        foreach (Item item in group)
        {
            if (item.StackObjectsCount <= 0 || item.StackObjectsCount >= item.StackMaxSize) continue;
            if (!Utils.IsFullResource(item)) continue;

            stacks.Add(item);
        }

        HashSet<(string, string)> failedPairs = new HashSet<(string, string)>();

        while (true)
        {
            Item target = null;

            for (int i = 0; i < stacks.Count; i++)
            {
                Item candidate = stacks[i];

                if (candidate.StackObjectsCount <= 0 || candidate.StackObjectsCount >= candidate.StackMaxSize) continue;

                if (target == null || candidate.StackObjectsCount > target.StackObjectsCount) target = candidate;
            }

            if (target == null) break;

            Item source = null;

            for (int i = 0; i < stacks.Count; i++)
            {
                Item candidate = stacks[i];

                if (candidate == target || candidate.StackObjectsCount <= 0 ||
                    candidate.StackObjectsCount >= candidate.StackMaxSize) continue;

                if (failedPairs.Contains((candidate.Id, target.Id)) ||
                    failedPairs.Contains((target.Id, candidate.Id))) continue;

                if (source == null || candidate.StackObjectsCount < source.StackObjectsCount) source = candidate;
            }

            if (source == null) break;

            int targetCount = target.StackObjectsCount;
            int sourceCount = source.StackObjectsCount;
            OperationResult<ITransferOrMergeResult> operation =
                ItemManipulator.TransferOrMerge(source, target, inventoryController, runNetworkTransactions);

            if (operation.Failed)
            {
                failedPairs.Add((source.Id, target.Id));
                continue;
            }

            if (!runNetworkTransactions)
            {
                stagedOperations.Add(operation.Value);
            }
            else
            {
                var result = await inventoryController.TryRunNetworkTransaction(operation);
                if (result.Failed) return new StringError(result.Error);
            }

            if (target.StackObjectsCount == targetCount && source.StackObjectsCount == sourceCount) break;
        }

        return null;
    }
}

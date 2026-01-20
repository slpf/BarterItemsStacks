using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BepInEx.Logging;
using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace BarterItemsStacksClient.Patches.Compatibility;

public class StashManagementHelperMergePatch : ModulePatch
{
    public static ManualLogSource Logger { get; set; }
    
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(AccessTools.TypeByName("StashManagementHelper.Helpers.ItemManager"), "MergeItems");
    }

    [PatchPrefix]
    private static bool Prefix(CompoundItem items, InventoryController inventoryController, bool simulate, ref Task __result)
    {
        __result = MergeItemsFixed(items, inventoryController, simulate);
        return false;
    }
    
    private static async Task MergeItemsFixed(CompoundItem items, InventoryController inventoryController, bool simulate)
    {
        if (items == null) throw new ArgumentNullException(nameof(items));
        if (inventoryController == null) throw new ArgumentNullException(nameof(inventoryController));

        try
        {
            foreach (var grid in items.Grids)
            {
                var stackableGroups = grid.Items
                    .Where(i => i.Owner != null && i.StackObjectsCount < i.StackMaxSize)
                    .GroupBy(i => new { i.TemplateId, i.SpawnedInSession })
                    .Where(g => g.Count() > 1)
                    .ToList();

                foreach (var group in stackableGroups)
                {
                    var failedMergePairs = new HashSet<(string, string)>();
                    bool mergesMade;

                    do
                    {
                        mergesMade = false;

                        var stacks = group
                            .Where(i => i.StackObjectsCount > 0)
                            .OrderByDescending(i => i.StackObjectsCount)
                            .ToList();

                        if (stacks.Count <= 1) break;

                        var targetStack = stacks.FirstOrDefault(s => s.StackObjectsCount < s.StackMaxSize);
                        if (targetStack == null) break;
                        
                        Item sourceStack = null;
                        
                        for (int i = stacks.Count - 1; i >= 0; i--)
                        {
                            var candidate = stacks[i];
                            if (candidate == targetStack) continue;

                            var pairKey = (candidate.Id, targetStack.Id);
                            var pairKeyReverse = (targetStack.Id, candidate.Id);

                            if (!failedMergePairs.Contains(pairKey) && !failedMergePairs.Contains(pairKeyReverse))
                            {
                                sourceStack = candidate;
                                break;
                            }
                        }

                        if (sourceStack == null) break;

                        var mergeResult = InteractionsHandlerClass.TransferOrMerge(sourceStack, targetStack, inventoryController, simulate);
                        
                        if (mergeResult.Succeeded)
                        {
                            await inventoryController.TryRunNetworkTransaction(mergeResult);
                            mergesMade = true;
                        }
                        else
                        {
                            failedMergePairs.Add((sourceStack.Id, targetStack.Id));
                            mergesMade = failedMergePairs.Count < (stacks.Count * (stacks.Count - 1) / 2);
                        }
                    } while (mergesMade);
                }
            }
        }
        catch (Exception e)
        {
            Logger.LogError($"Error merging items: {e.Message}");
            throw;
        }
    }
}
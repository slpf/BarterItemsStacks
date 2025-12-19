using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine;

namespace BarterItemsStacksClient.Patches.Quest
{
    public class PlaceItemProtectPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Player.PlayerInventoryController), nameof(Player.PlayerInventoryController.SetupItem));
        }

        [PatchPrefix]
        public static bool Prefix(Player.PlayerInventoryController __instance,
            Item item,
            string zone,
            Vector3 position,
            Quaternion rotation,
            float setupTime,
            Callback callback)
        {
            if (item.StackObjectsCount <= 1)
                return true;
            
            var sortingTable = __instance.Inventory.SortingTable;
            
            var location = sortingTable.Grid.FindLocationForItem(item);
            
            var beforeIds = new HashSet<string>(
                __instance.Inventory.AllRealPlayerItems.Select(x => x.Id),
                StringComparer.Ordinal);

            var split = InteractionsHandlerClass.SplitExact(item, 1, location, __instance, __instance, true);
            
            if (split.Failed)
            {
                callback?.Invoke(split.ToResult());
                return false;
            }
            
            __instance.TryRunNetworkTransaction(split, result =>
                {
                    if (!result.Succeed)
                    {
                        callback?.Invoke(result);
                        return;
                    }

                    var newItem = __instance.Inventory.AllRealPlayerItems
                        .FirstOrDefault(x =>
                            x != null &&
                            !beforeIds.Contains(x.Id) &&
                            x.TemplateId == item.TemplateId &&
                            x.StackObjectsCount == 1 &&
                            x.CurrentAddress != null &&
                            x.CurrentAddress.IsChildOf(sortingTable, false));

                    if (newItem == null)
                    {
                        callback?.Invoke(result);
                        return;
                    }
                    
                    __instance.SetupItem(newItem, zone, position, rotation, setupTime, callback);
                }
            );

            return false;
        }
    }
}
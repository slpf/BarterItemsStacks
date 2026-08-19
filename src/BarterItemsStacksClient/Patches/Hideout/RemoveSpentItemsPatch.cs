using Diz.LanguageExtensions;
using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EFT.Hideout;
using UnityEngine;

namespace BarterItemsStacksClient.Patches.Hideout
{
    internal class RemoveSpentItemsPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(HideoutRepresentation), nameof(HideoutRepresentation.RemoveSpentItems));
        }

        [PatchPrefix]
        public static bool Prefix(HideoutRepresentation __instance, IEnumerable<JsonType.ItemToHandover> items, ref IEnumerable<IItemOperationResult> __result)
        {
            List <IItemOperationResult> list = null;
            
            using (IEnumerator<JsonType.ItemToHandover> enumerator = items.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    HideoutRepresentation.CG_RemoveSpentItems @class = new HideoutRepresentation.CG_RemoveSpentItems();
                    @class.itemReference = enumerator.Current;
                    Item item = __instance._allStashItems.FirstOrDefault(@class.method_0);
                    bool flag;
                    
                    if (item == null)
                    {
                        flag = false;
                    }
                    else
                    {
                        ItemAddress currentAddress = item.CurrentAddress;
                        flag = currentAddress?.GetOwnerOrNull() != null;
                    }

                    if (!flag)
                    {
                        continue;
                    }
                    
                    if (list == null)
                    {
                        list = new List<IItemOperationResult>();
                    }
                    
                    OperationResult<IItemOperationResult> gstruct = default(OperationResult<IItemOperationResult>);
                    StackableItem stackableItemItemClass = item as StackableItem;

                    if (stackableItemItemClass != null && stackableItemItemClass.StackObjectsCount > @class.itemReference.count)
                    {
                        gstruct = ItemManipulator.SplitToNowhere(stackableItemItemClass, @class.itemReference.count, __instance._inventoryController, __instance._inventoryController, false).Cast<SplitToNowhereResult, IItemOperationResult>();
                    }
                    else if(item.StackObjectsCount > @class.itemReference.count)
                    {
                        gstruct = ItemManipulator.SplitToNowhere(item, @class.itemReference.count, __instance._inventoryController, __instance._inventoryController, false).Cast<SplitToNowhereResult, IItemOperationResult>();
                    }
                    else
                    {
                        gstruct = ItemManipulator.Remove(item, __instance._inventoryController).Cast<RemoveResult, IItemOperationResult>();
                    }
                    if (gstruct.Succeeded)
                    {
                        list.Add(gstruct.Value);
                    }
                    else
                    {
                        Debug.LogError(gstruct.Error);
                    }
                }
            }

            __instance.method_24();
            __result = list;

            return false;
        }
    }
}

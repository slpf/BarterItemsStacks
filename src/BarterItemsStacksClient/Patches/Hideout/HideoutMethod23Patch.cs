using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace BarterItemsStacksClient.Patches.Hideout
{
    internal class HideoutMethod23Patch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(HideoutClass), nameof(HideoutClass.method_23));
        }

        [PatchPrefix]
        public static bool Prefix(HideoutClass __instance, IEnumerable<GClass1433> items, ref IEnumerable<GInterface424> __result)
        {
            List <GInterface424> list = null;
            using (IEnumerator<GClass1433> enumerator = items.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    HideoutClass.Class1919 @class = new HideoutClass.Class1919();
                    @class.itemReference = enumerator.Current;
                    Item item = __instance.List_2.FirstOrDefault(new Func<Item, bool>(@class.method_0));
                    bool flag;
                    if (item == null)
                    {
                        flag = null != null;
                    }
                    else
                    {
                        ItemAddress currentAddress = item.CurrentAddress;
                        flag = ((currentAddress != null) ? currentAddress.GetOwnerOrNull() : null) != null;
                    }
                    if (flag)
                    {
                        if (list == null)
                        {
                            list = new List<GInterface424>();
                        }
                        GStruct154<GInterface424> gstruct = default(GStruct154<GInterface424>);
                        StackableItemItemClass stackableItemItemClass = item as StackableItemItemClass;

                        if (stackableItemItemClass != null && stackableItemItemClass.StackObjectsCount > @class.itemReference.count)
                        {
                            gstruct = InteractionsHandlerClass.SplitToNowhere(stackableItemItemClass, @class.itemReference.count, __instance.InventoryController_0, __instance.InventoryController_0, false).Cast<GClass3422, GInterface424>();
                        }
                        else if(item.StackMaxSize > 1 && item.StackObjectsCount > @class.itemReference.count)
                        {
                            gstruct = InteractionsHandlerClass.SplitToNowhere(item, @class.itemReference.count, __instance.InventoryController_0, __instance.InventoryController_0, false).Cast<GClass3422, GInterface424>();
                        }
                        else
                        {
                            gstruct = InteractionsHandlerClass.Remove(item, __instance.InventoryController_0, false).Cast<GClass3410, GInterface424>();
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
            }

            __instance.method_24();
            __result = list;

            return false;
        }
    }
}

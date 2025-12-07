using EFT.Hideout;
using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace BarterItemsStacksClient.Patches
{
    internal class HideoutMethod21Patch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(HideoutClass), nameof(HideoutClass.method_21));
        }

        [PatchPrefix]
        public static bool Prefix(HideoutClass __instance, ItemRequirement[] requirements, ref List<GStruct299> __result)
        {
            requirements = requirements.Where(r => r.IntCount > 0).ToArray();
            List<GStruct299> list = new List<GStruct299>(requirements.Length);
            foreach (ItemRequirement itemRequirement in requirements)
            {
                bool flag = itemRequirement is ToolRequirement;
                int num = itemRequirement.IntCount;
                __instance.method_40(HideoutClass.List_0, itemRequirement);
                foreach (Item item in HideoutClass.List_0)
                {
                    StackableItemItemClass stackableItemItemClass = item as StackableItemItemClass;

                    GStruct299 gstruct = new GStruct299
                    {
                        Item = item,
                        IsTool = flag,
                        Count = ((stackableItemItemClass == null) ? (item.StackMaxSize > 1) ? Mathf.Min(num, item.StackObjectsCount) : 1 : Mathf.Min(num, stackableItemItemClass.StackObjectsCount)),
                        RemoveReferenceItem = stackableItemItemClass != null ? num >= stackableItemItemClass.StackObjectsCount : (item.StackMaxSize > 1 ? num >= item.StackObjectsCount : true),
                        Requirements = requirements
                    };
                    num -= gstruct.Count;
                    list.Add(gstruct);
                    if (num <= 0)
                    {
                        break;
                    }
                }
            }
            HideoutClass.List_0.Clear();

            __result = list;

            return false;
        }
    }
}

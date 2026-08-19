using EFT.Hideout;
using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace BarterItemsStacksClient.Patches.Hideout
{
    internal class GetItemReferencesPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(HideoutRepresentation), nameof(HideoutRepresentation.GetItemReferences));
        }

        [PatchPrefix]
        public static bool Prefix(HideoutRepresentation __instance, ItemRequirement[] requirements, ref List<HideoutItemReference> __result)
        {
            requirements = requirements.Where(r => r.IntCount > 0).ToArray();
            List<HideoutItemReference> list = new List<HideoutItemReference>(requirements.Length);
            
            foreach (ItemRequirement itemRequirement in requirements)
            {
                bool flag = itemRequirement is ToolRequirement;
                int num = itemRequirement.IntCount;
                
                __instance.CG_GetItemReferences(HideoutRepresentation._itemsBuffer, itemRequirement);
                
                foreach (Item item in HideoutRepresentation._itemsBuffer)
                {
                    StackableItem stackableItemItemClass = item as StackableItem;

                    HideoutItemReference gstruct = new HideoutItemReference
                    {
                        Item = item,
                        IsTool = flag,
                        Count = ((stackableItemItemClass == null) ? (item.StackMaxSize > 1 || item.StackObjectsCount > 1) ? Mathf.Min(num, item.StackObjectsCount) : 1 : Mathf.Min(num, stackableItemItemClass.StackObjectsCount)),
                        RemoveReferenceItem = stackableItemItemClass != null ? num >= stackableItemItemClass.StackObjectsCount : (item.StackObjectsCount <= 1 || num >= item.StackObjectsCount),
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
            
            HideoutRepresentation._itemsBuffer.Clear();

            __result = list;

            return false;
        }
    }
}

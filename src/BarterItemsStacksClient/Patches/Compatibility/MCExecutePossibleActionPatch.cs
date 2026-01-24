using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BarterItemsStacksClient.Patches.Compatibility
{
    internal class MCExecutePossibleActionPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(AccessTools.TypeByName("MergeConsumables.Patches.ExecutePossibleAction_Patch"), "Prefix");
        }

        [PatchPrefix]
        [HarmonyPriority(Priority.First)]
        private static bool Prefix(ItemContextAbstractClass itemContext, Item targetItem, ref bool __result)
        {
            if (itemContext.Item.StackObjectsCount > 1 || targetItem.StackObjectsCount > 1)
            {
                __result = true;
                return false;
            }

            if (Utils.CanMergeResources(itemContext.Item, targetItem))
            {
                __result = true;
                return false;
            }

            return true;
        }

        
    }
}

using System.Reflection;
using Comfort.Logs;
using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace BarterItemsStacksClient.Patches.Interactions
{
    public class StacksInSpecialSlots : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Slot), nameof(Slot.method_7));
        }

        [PatchPrefix]
        private static bool Prefix(Slot __instance, Item item, bool ignoreRestrictions, bool ignoreMalfunction,
            bool simulate, ref GStruct154<GClass3414> __result)
        {
            if (!__instance.IsSpecial)
            {
                return true;
            }
            
            GStruct156<bool> check = __instance.method_5(item, ignoreRestrictions, ignoreMalfunction);
            
            if (check.Failed)
            {
                __result = check.Error;
                return false;
            }
            
            if (!ignoreRestrictions && !item.ParentRecursiveCheck(__instance.ParentItem))
            {
                __result = new Slot.GClass1579(item, __instance);
                return false;
            }
            
            int count = item.StackObjectsCount;
            
            if (simulate)
            {
                __result = new GClass3414(item, __instance.CreateItemAddress(), count, true);
                return false;
            }
            
            __instance.method_6(item);
            
            foreach (Slot slot in __instance.method_3(item))
            {
	            slot.BlockerSlots.Add(__instance);
            }
            
            __result = new GClass3414(item, item.Parent, count, false);

            return false;
        }
    }
}
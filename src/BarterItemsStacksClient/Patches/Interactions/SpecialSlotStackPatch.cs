using Diz.LanguageExtensions;
using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;

namespace BarterItemsStacksClient.Patches.Interactions
{
    public class SpecialSlotStackPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Slot), "method_7");
        }

        [PatchPrefix]
        public static bool Prefix(Slot __instance, Item item, bool ignoreRestrictions, bool ignoreMalfunction, bool simulate,
            ref GStruct154<GClass3414> __result)
        {
            if (!__instance.IsSpecial || item == null || item.StackMaxSize <= 1)
            {
                return true;
            }

            Item contained = __instance.ContainedItem;
            if (contained != null)
            {
                if (!simulate)
                {
                    return true;
                }

                if (contained.TemplateId == item.TemplateId
                    && contained.StackObjectsCount < contained.StackMaxSize)
                {
                    int available = contained.StackMaxSize - contained.StackObjectsCount;
                    __result = new GClass3414(item, __instance.CreateItemAddress(), available, true);
                    return false;
                }

                return true;
            }

            var option = __instance.method_5(item, ignoreRestrictions, ignoreMalfunction);
            if (option.Failed)
            {
                __result = option.Error;
                return false;
            }

            if (!ignoreRestrictions && !item.ParentRecursiveCheck(__instance.ParentItem))
            {
                __result = new Slot.GClass1579(item, __instance);
                return false;
            }

            if (simulate)
            {
                __result = new GClass3414(item, __instance.CreateItemAddress(), item.StackObjectsCount, true);
                return false;
            }

            __instance.method_6(item);
            foreach (Slot slot in __instance.method_3(item))
            {
                slot.BlockerSlots.Add(__instance);
            }

            __result = new GClass3414(item, item.Parent, item.StackObjectsCount, false);
            return false;
        }
    }
}

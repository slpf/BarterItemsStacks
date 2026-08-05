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
            return AccessTools.Method(typeof(Slot), nameof(Slot.AddInternal));
        }

        [PatchPrefix]
        public static bool Prefix(Slot __instance, Item item, bool ignoreRestrictions, bool ignoreMalfunction, bool simulate,
            ref OperationResult<ContainerAddResult> __result)
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
                    __result = new ContainerAddResult(item, __instance.CreateItemAddress(), available, true);
                    return false;
                }

                return true;
            }

            var option = __instance.CheckConditions(item, ignoreRestrictions, ignoreMalfunction);
            if (option.Failed)
            {
                __result = option.Error;
                return false;
            }

            if (!ignoreRestrictions && !item.ParentRecursiveCheck(__instance.ParentItem))
            {
                __result = new Slot.ItemFiltersWontAllowError(item, __instance);
                return false;
            }

            if (simulate)
            {
                __result = new ContainerAddResult(item, __instance.CreateItemAddress(), item.StackObjectsCount, true);
                return false;
            }

            __instance.Add(item);
            foreach (Slot slot in __instance.GetConflictingSlot(item))
            {
                slot.BlockerSlots.Add(__instance);
            }

            __result = new ContainerAddResult(item, item.Parent, item.StackObjectsCount, false);
            return false;
        }
    }
}

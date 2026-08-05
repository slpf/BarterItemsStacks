using Comfort.Common;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;
using Diz.LanguageExtensions;
using EFT;
using EFT.InventoryLogic;

namespace BarterItemsStacksClient.Patches.Quest
{
    internal class PlaceItemTriggerPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(InteractionContextHelper.CG_GetAvailableActions5), nameof(InteractionContextHelper.CG_GetAvailableActions5.method_0));
        }

        [PatchPrefix]
        public static bool Prefix(InteractionContextHelper.CG_GetAvailableActions5 __instance, bool successful)
        {
            if (__instance.resultItem.StackObjectsCount > 1)
            {
                __instance.owner.Player.PlantItemNetwork(__instance.resultItem.TemplateId, __instance.itemTrigger.Id, successful);
                __instance.owner.CloseObjectivesPanel();
                
                if (successful)
                {
                    ItemController inventoryController = __instance.owner.Player.InventoryController;
                    OperationResult gstruct = InteractionsHandlerClassExtensions.RemoveOneFromStack(__instance.resultItem, __instance.owner.Player.InventoryController, true);
                    Callback callback;
                    
                    if ((callback = __instance.callback_0) == null)
                    {
                        callback = (__instance.callback_0 = new Callback(__instance.method_1));
                    }
                    
                    inventoryController.TryRunNetworkTransaction(gstruct, callback);
                }

                return false;
            }

            return true;
        }
    }
}

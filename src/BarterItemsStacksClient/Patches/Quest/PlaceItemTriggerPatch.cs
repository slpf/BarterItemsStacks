using Comfort.Common;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;

namespace BarterItemsStacksClient.Patches.Quest
{
    internal class PlaceItemTriggerPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GetActionsClass.Class1772), nameof(GetActionsClass.Class1772.method_0));
        }

        [PatchPrefix]
        public static bool Prefix(GetActionsClass.Class1772 __instance, bool successful)
        {
            if (__instance.resultItem.StackObjectsCount > 1)
            {
                __instance.owner.Player.vmethod_6(__instance.resultItem.TemplateId, __instance.itemTrigger.Id, successful);
                __instance.owner.CloseObjectivesPanel();
                if (successful)
                {
                    TraderControllerClass inventoryController = __instance.owner.Player.InventoryController;
                    GStruct153 gstruct = InteractionsHandlerClassExtensions.RemoveOneFromStack(__instance.resultItem, __instance.owner.Player.InventoryController, true);
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

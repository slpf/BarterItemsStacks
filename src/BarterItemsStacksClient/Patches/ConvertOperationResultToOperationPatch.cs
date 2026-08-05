using SPT.Reflection.Patching;
using System.Reflection;
using BarterItemsStacksClient.RemoveOneFromStack;
using EFT.InventoryLogic;
using EFT.InventoryLogic.Operations;

namespace BarterItemsStacksClient.Patches
{
    internal class ConvertOperationResultToOperationPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(ItemController).GetMethod(nameof(ItemController.ConvertOperationResultToOperation));
        }

        [PatchPrefix]
        public static bool Prefix(ItemController __instance, IOperationResult operationResult, ref AbstractOperation __result)
        {
            if (operationResult is RemoveOneFromStackResult removeResult)
            {
                __result = new RemoveOneFromStackOperation(__instance.GetAndIncrementNextOperationId(), __instance, removeResult);
                return false;
            }

            return true;
        }
    }
}

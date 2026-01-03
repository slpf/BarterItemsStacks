using SPT.Reflection.Patching;
using System.Reflection;

namespace BarterItemsStacksClient.Patches
{
    internal class ConvertOperationResultToOperationPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(TraderControllerClass).GetMethod(nameof(TraderControllerClass.ConvertOperationResultToOperation));
        }

        [PatchPrefix]
        public static bool Prefix(TraderControllerClass __instance, IRaiseEvents operationResult, ref BaseInventoryOperationClass __result)
        {
            if (operationResult is RemoveOneFromStack.RemoveOneFromStackResult removeResult)
            {
                __result = new RemoveOneFromStack.RemoveOneFromStackOperation(__instance.method_12(), __instance, removeResult);
                return false;
            }

            return true;
        }
    }
}

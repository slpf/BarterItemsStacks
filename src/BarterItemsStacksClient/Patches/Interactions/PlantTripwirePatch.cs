using BarterItemsStacksClient.PlantOneTripwire;
using Comfort.Common;
using Diz.LanguageExtensions;
using EFT;
using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;
using BarterItemsStacksClient.RemoveOneFromStack;
using UnityEngine;

namespace BarterItemsStacksClient.Patches.Interactions
{
    public class PlantTripwirePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Player.PlayerInventoryController), nameof(Player.PlayerInventoryController.PlantTripwire));
        }

        [PatchPrefix]
        public static bool Prefix(Player.PlayerInventoryController __instance,
            ThrowWeap grenade,
            PlantingKit plantingKit,
            Vector3 fromPosition,
            Vector3 toPosition,
            Callback callback)
        {
            if (plantingKit == null || plantingKit.StackObjectsCount <= 1)
                return true;

            ThrowWeap beacon = grenade.CloneItem(__instance);
            beacon.StackObjectsCount = 1;

            OperationResult<RemoveOneFromStackResult> grenadeConsume = InteractionsHandlerClassExtensions.RemoveOneFromStack(grenade, __instance, true);
            if (grenadeConsume.Failed)
            {
                callback?.Invoke(grenadeConsume.ToResult());
                return false;
            }

            OperationResult<RemoveOneFromStackResult> kitConsume = InteractionsHandlerClassExtensions.RemoveOneFromStack(plantingKit, __instance, true);
            if (kitConsume.Failed)
            {
                callback?.Invoke(kitConsume.ToResult());
                return false;
            }

            PlantTripwireResult result = new PlantTripwireResult(grenade, plantingKit, new CombinedOperationResult(grenadeConsume.Value, kitConsume.Value));

            PlantOneTripwireOperation op = new PlantOneTripwireOperation(
                __instance.GetAndIncrementNextOperationId(),
                __instance,
                result,
                fromPosition,
                toPosition,
                __instance.Player,
                beacon);

            __instance.Execute(op, callback);
            return false;
        }
    }
}

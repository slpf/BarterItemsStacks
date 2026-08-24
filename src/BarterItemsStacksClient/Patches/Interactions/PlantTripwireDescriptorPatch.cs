using BarterItemsStacksClient.PlantOneTripwire;
using BarterItemsStacksClient.RemoveOneFromStack;
using Diz.LanguageExtensions;
using EFT;
using EFT.InventoryLogic;
using EFT.InventoryLogic.Operations;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;

namespace BarterItemsStacksClient.Patches.Interactions
{
    public class PlantTripwireDescriptorPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(
                typeof(PlantTripwireOperationDescriptor),
                nameof(PlantTripwireOperationDescriptor.ToInventoryOperation),
                new[] { typeof(IPlayer) });
        }

        [PatchPostfix]
        public static void Postfix(ref OperationCreationResult<AbstractOperation> __result)
        {
            if (__result.Failed || __result.Value is not PlantTripwireOperation originalOperation)
            {
                return;
            }

            ThrowWeap grenade = originalOperation.Tripwire;
            PlantingKit plantingKit = originalOperation._plantingKit;

            if (plantingKit == null ||
                grenade.StackObjectsCount <= 1 && plantingKit.StackObjectsCount <= 1)
            {
                return;
            }

            originalOperation.Status = EOperationStatus.Failed;
            originalOperation.Dispose();

            OperationResult<RemoveOneFromStackResult> grenadeConsume =
                InteractionsHandlerClassExtensions.RemoveOneFromStack(
                    grenade,
                    originalOperation.Controller,
                    true);

            if (grenadeConsume.Failed)
            {
                __result = grenadeConsume.Error;
                return;
            }

            OperationResult<RemoveOneFromStackResult> kitConsume =
                InteractionsHandlerClassExtensions.RemoveOneFromStack(
                    plantingKit,
                    originalOperation.Controller,
                    true);

            if (kitConsume.Failed)
            {
                __result = kitConsume.Error;
                return;
            }

            ThrowWeap beacon = grenade.CloneItem(originalOperation.Controller);
            beacon.StackObjectsCount = 1;

            PlantTripwireResult result = new PlantTripwireResult(
                grenade,
                plantingKit,
                new CombinedOperationResult(grenadeConsume.Value, kitConsume.Value));

            __result = new PlantOneTripwireOperation(
                originalOperation.Id,
                originalOperation.Controller,
                result,
                originalOperation._fromPosition,
                originalOperation._toPosition,
                originalOperation._player,
                beacon);
        }
    }
}

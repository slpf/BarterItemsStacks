using BarterItemsStacksClient.PlantOneTripwire;
using BarterItemsStacksClient.RemoveOneFromStack;
using Comfort.Common;
using Diz.LanguageExtensions;
using EFT;
using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;
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
            ThrowWeapItemClass grenade,
            PlantingKitsItemClass plantingKit,
            Vector3 fromPosition,
            Vector3 toPosition,
            Callback callback)
        {
            if (plantingKit == null || plantingKit.StackObjectsCount <= 1)
                return true;

            ThrowWeapItemClass beacon = grenade.CloneItem(__instance);
            beacon.StackObjectsCount = 1;

            GStruct154<RemoveOneFromStackResult> grenadeConsume = InteractionsHandlerClassExtensions.RemoveOneFromStack(grenade, __instance, true);
            if (grenadeConsume.Failed)
            {
                callback?.Invoke(grenadeConsume.ToResult());
                return false;
            }

            GStruct154<RemoveOneFromStackResult> kitConsume = InteractionsHandlerClassExtensions.RemoveOneFromStack(plantingKit, __instance, true);
            if (kitConsume.Failed)
            {
                callback?.Invoke(kitConsume.ToResult());
                return false;
            }

            GClass3407 result = new GClass3407(grenade, plantingKit, new GClass3400(grenadeConsume.Value, kitConsume.Value));

            PlantOneTripwireOperation op = new PlantOneTripwireOperation(
                __instance.method_12(),
                __instance,
                result,
                fromPosition,
                toPosition,
                __instance.Player_0,
                beacon);

            __instance.vmethod_1(op, callback);
            return false;
        }
    }
}

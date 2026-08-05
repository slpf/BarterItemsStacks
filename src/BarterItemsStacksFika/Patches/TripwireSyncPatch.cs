using BarterItemsStacksFika.Packets;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using Fika.Core.Main.Utils;
using Fika.Core.Networking;
using Fika.Core.Networking.LiteNetLib;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;

namespace BarterItemsStacksFika.Patches;

public class TripwireSyncPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(Player.PlayerInventoryController), nameof(Player.PlayerInventoryController.PlantTripwire));
    }

    [PatchPostfix]
    public static void Postfix(Player.PlayerInventoryController __instance,
        ThrowWeap grenade,
        PlantingKit plantingKit,
        Vector3 fromPosition,
        Vector3 toPosition)
    {
        if (plantingKit == null || plantingKit.StackObjectsCount <= 1)
        {
            return;
        }

        if (!FikaBackendUtils.IsClient)
        {
            return;
        }

        IFikaNetworkManager net = Singleton<IFikaNetworkManager>.Instance;
        if (net == null)
        {
            return;
        }

        TripwirePlantRequestPacket packet = new()
        {
            GrenadeTemplate = grenade.TemplateId.ToString(),
            FromPosition = fromPosition,
            ToPosition = toPosition,
            ProfileId = __instance.Player.ProfileId
        };

        net.SendData(ref packet, DeliveryMethod.ReliableOrdered, false);
    }
}

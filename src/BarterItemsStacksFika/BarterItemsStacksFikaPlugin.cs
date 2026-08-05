using BarterItemsStacksFika.Packets;
using BarterItemsStacksFika.Patches;
using BepInEx;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using Fika.Core.Main.Utils;
using Fika.Core.Modding;
using Fika.Core.Modding.Events;
using Fika.Core.Networking;

namespace BarterItemsStacksFika;

[BepInPlugin("com.slpf.barteritemsstacks.fika", "BarterItemsStacksFika", "1.4.0")]
[BepInDependency("com.fika.core", BepInDependency.DependencyFlags.HardDependency)]
[BepInDependency("com.slpf.barteritemsstacks", BepInDependency.DependencyFlags.HardDependency)]
public class BarterItemsStacksFikaPlugin : BaseUnityPlugin
{
    private void Awake()
    {
        new TripwireSyncPatch().Enable();
        FikaEventDispatcher.SubscribeEvent<FikaNetworkManagerCreatedEvent>(OnNetworkManagerCreated);
    }

    private static void OnNetworkManagerCreated(FikaNetworkManagerCreatedEvent e)
    {
        e.Manager.RegisterPacket<TripwirePlantRequestPacket>(OnTripwirePlantRequest);
    }

    private static void OnTripwirePlantRequest(TripwirePlantRequestPacket packet)
    {
        if (!FikaBackendUtils.IsServer)
        {
            return;
        }

        if (!Singleton<IGameLevel>.Instantiated)
        {
            return;
        }

        ItemFactory factory = Singleton<ItemFactory>.Instance;
        if (factory == null)
        {
            return;
        }

        if (factory.CreateItem(factory.NextId, packet.GrenadeTemplate, null) is not ThrowWeap grenade)
        {
            return;
        }

        Singleton<IGameLevel>.Instance.PlantTripwire(grenade, packet.ProfileId, packet.FromPosition, packet.ToPosition);
    }
}

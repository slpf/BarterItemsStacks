using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using EFT.InventoryLogic.Operations;
using UnityEngine;

namespace BarterItemsStacksClient.PlantOneTripwire
{
    public class PlantOneTripwireOperation(
        ushort id,
        ItemController controller,
        PlantTripwireResult result,
        Vector3 fromPosition,
        Vector3 toPosition,
        IPlayer player,
        ThrowWeap beacon)
        : PlantTripwireOperation(id, controller, result, fromPosition, toPosition, player)
    {
        private readonly ThrowWeap _beacon = beacon;

        public override void Dispose()
        {
            FinalizeOperation();

            if (base.Status == EOperationStatus.Succeeded && Singleton<IGameLevel>.Instantiated)
            {
                Singleton<IGameLevel>.Instance.PlantTripwire(_beacon, _player.ProfileId, _fromPosition, _toPosition);
            }
        }

        public override BaseInventoryCommand ToBaseInventoryCommand(string ownerId)
        {
            return new PlantOneTripwireModel(Tripwire.Id, _plantingKit?.Id);
        }
    }
}

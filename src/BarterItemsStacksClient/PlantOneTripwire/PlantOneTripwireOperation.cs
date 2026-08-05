using System.Threading.Tasks;
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
        Item beacon)
        : AbstractAsyncOperation<PlantTripwireResult>(id, controller, result)
    {
        private readonly ThrowWeap _tripwire = result.Tripwire;
        private readonly PlantingKit _plantingKit = result.PlantingKit;

        public override Task<IResult> ExecuteInternal()
        {
            return Task.FromResult<IResult>(ExecuteAndFinish());
        }

        public override void Dispose()
        {
            base.Dispose();

            if (base.Status == EOperationStatus.Succeeded && Singleton<IGameLevel>.Instantiated)
            {
                Singleton<IGameLevel>.Instance.PlantTripwire(beacon, player.ProfileId, fromPosition, toPosition);
            }
        }

        public override InventoryOperationDescriptor ToDescriptor()
        {
            throw new ClientServerOnlyOperationException(this);
        }

        public override BaseInventoryCommand ToBaseInventoryCommand(string ownerId)
        {
            return new PlantOneTripwireModel(_tripwire.Id, _plantingKit?.Id);
        }
    }
}

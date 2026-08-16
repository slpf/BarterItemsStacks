using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using System.Threading.Tasks;
using UnityEngine;

namespace BarterItemsStacksClient.PlantOneTripwire
{
    public class PlantOneTripwireOperation : GClass3475<GClass3407>
    {
        private readonly ThrowWeapItemClass _tripwire;
        private readonly PlantingKitsItemClass _plantingKit;
        private readonly IPlayer _player;
        private readonly Vector3 _fromPosition;
        private readonly Vector3 _toPosition;
        private readonly Item _beacon;

        public PlantOneTripwireOperation(ushort id, TraderControllerClass controller, GClass3407 result, Vector3 fromPosition, Vector3 toPosition, IPlayer player, Item beacon)
            : base(id, controller, result)
        {
            _tripwire = result.Tripwire;
            _plantingKit = result.PlantingKit;
            _player = player;
            _fromPosition = fromPosition;
            _toPosition = toPosition;
            _beacon = beacon;
        }

        public override Task<IResult> ExecuteInternal()
        {
            return Task.FromResult<IResult>(method_6());
        }

        public override void Dispose()
        {
            base.Dispose();

            if (base.Status == EOperationStatus.Succeeded && Singleton<GInterface169>.Instantiated)
            {
                Singleton<GInterface169>.Instance.PlantTripwire(_beacon, _player.ProfileId, _fromPosition, _toPosition);
            }
        }

        public override BaseDescriptorClass ToDescriptor()
        {
            throw new GException23(this);
        }

        public override GClass3471 ToBaseInventoryCommand(string ownerId)
        {
            return new PlantOneTripwireModel(_tripwire.Id, _plantingKit?.Id);
        }
    }
}

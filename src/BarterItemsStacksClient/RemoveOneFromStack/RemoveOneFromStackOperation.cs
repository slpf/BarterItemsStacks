using Comfort.Common;
using EFT.InventoryLogic;
using System.Threading.Tasks;

namespace BarterItemsStacksClient.RemoveOneFromStack
{
    public class RemoveOneFromStackOperation : GClass3475<RemoveOneFromStackResult> 
    {
        public Item Item;
        public ItemAddress Address;

        public RemoveOneFromStackOperation(ushort id, TraderControllerClass controller, RemoveOneFromStackResult result) 
            : base(id, controller, result) 
        {
            Item = result.Item;
            Address = Item.Parent;
        }

        public override async Task<IResult> ExecuteInternal()
        {
            if (Item.StackObjectsCount == 1)
            {
                await method_3(Item, Address, null, null);
                return method_6();
            }

            await method_3(Item, Address, Address, null);
            Execute();
            await method_4(Item, Address, null);
            return method_5();
        }

        public override GClass3471 ToBaseInventoryCommand(string ownerId)
        {
            return Gstruct156_0.Value.ToRemoveOneFromStackModel();
        }

        public override BaseDescriptorClass ToDescriptor()
        {
            return new RemoveOneFromStackDescriptor
            {
                Operation = this,
                OwnerId = OwnerId,
                OperationId = Id,
                Item = Item.Id
            };
        }

        public override string ToString()
        {
            return $"RemoveOneFromStack {Item.ToFullString()}";
        }

    }
}

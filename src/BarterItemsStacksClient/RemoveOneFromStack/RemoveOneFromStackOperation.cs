using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using System.Threading.Tasks;
using EFT.InventoryLogic.Operations;

namespace BarterItemsStacksClient.RemoveOneFromStack
{
    public class RemoveOneFromStackOperation : AbstractAsyncOperation<RemoveOneFromStackResult> 
    {
        public Item Item;
        public ItemAddress Address;
        private readonly bool _removesItem;

        public RemoveOneFromStackOperation(ushort id, ItemController controller, RemoveOneFromStackResult result) 
            : base(id, controller, result) 
        {
            Item = result.Item;
            Address = Item.Parent;
            _removesItem = result.IsDiscard;
        }

        public override async Task<IResult> ExecuteInternal()
        {
            if (Item.StackObjectsCount == 1)
            {
                await OutProcess(Item, Address, null, null);
                return ExecuteAndFinish();
            }

            await OutProcess(Item, Address, Address, null);
            Execute();
            await InProcess(Item, Address, null);
            return FinishExecution();
        }

        public override BaseInventoryCommand ToBaseInventoryCommand(string ownerId)
        {
            return _executableResult.Value.ToRemoveOneFromStackModel();
        }

        public override EFT.InventoryOperationDescriptor ToDescriptor()
        {
            if (!_removesItem)
            {
                return new SplitToNowhereDescriptor
                {
                    Operation = this,
                    ItemId = Item.Id,
                    Count = 1
                };
            }

            return new RemoveOperationDescriptor
            {
                Operation = this,
                ItemId = Item.Id
            };
        }

        public override string ToString()
        {
            return $"RemoveOneFromStack {Item.ToFullString()}";
        }

    }
}

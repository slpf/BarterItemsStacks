using EFT;

namespace BarterItemsStacksClient.RemoveOneFromStack
{
    public class RemoveOneFromStackDescriptor : BaseDescriptorClass
    {
        public string Item;

        public override GStruct152<BaseInventoryOperationClass> ToInventoryOperation(IPlayer player)
        {
            var itemResult = player.FindItemById(Item);

            if (itemResult.Failed)
            {
                return itemResult.Error;
            }        

            var result = InteractionsHandlerClassExtensions.RemoveOneFromStack(itemResult.Value, player.InventoryController,simulate: true);

            if (result.Failed)
            {
                return result.Error;
            }
                

            return new RemoveOneFromStackOperation(OperationId, player.InventoryController, result.Value);
        }

        public override string ToString() => $"Item: {Item}";
    }
}

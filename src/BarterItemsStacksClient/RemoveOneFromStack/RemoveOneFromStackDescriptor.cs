using Diz.LanguageExtensions;
using EFT;
using EFT.InventoryLogic;
using EFT.InventoryLogic.Operations;

namespace BarterItemsStacksClient.RemoveOneFromStack
{
    public class RemoveOneFromStackDescriptor : InventoryOperationDescriptor
    {
        public string Item;

        public override OperationCreationResult<AbstractOperation> ToInventoryOperation(IPlayer player)
        {
            Option<Item> itemResult = player.FindItemById(Item);

            if (itemResult.Failed)
            {
                return itemResult.Error;
            }        

            OperationResult<RemoveOneFromStackResult> result = InteractionsHandlerClassExtensions.RemoveOneFromStack(itemResult.Value, player.InventoryController,simulate: true);

            if (result.Failed)
            {
                return result.Error;
            }
                

            return new RemoveOneFromStackOperation(OperationId, player.InventoryController, result.Value);
        }

        public override string ToString() => $"Item: {Item}";
    }
}

using BarterItemsStacksClient.RemoveOneFromStack;
using Diz.LanguageExtensions;
using EFT.InventoryLogic;

namespace BarterItemsStacksClient
{
    internal static class InteractionsHandlerClassExtensions
    {
        public static OperationResult<RemoveOneFromStackResult> RemoveOneFromStack(
        Item item,
        ItemController itemController,
        bool simulate)
        {
            if (item == null)
            {
                return new StringError("Item is null");
            }
                
            ItemAddress from = item.CurrentAddress;

            int originalCount = item.StackObjectsCount;

            if (originalCount <= 0)
            {
                return new StringError("Invalid StackObjectsCount");
            }
                
            OperationResult<DiscardResult> discard = default;

            if (originalCount == 1)
            {
                discard = ItemManipulator.Discard(item, itemController, false);
                if (!discard.Succeeded)
                {
                    return discard.Error;
                }  
            }
            else
            {
                item.StackObjectsCount = originalCount - 1;
            }

            if (simulate)
            {
                if (discard.Succeeded && discard.Value != null)
                {
                    discard.Value.RollBack();
                }
                else
                {
                    item.StackObjectsCount = originalCount;
                }
            }

            return new RemoveOneFromStackResult(item, from, discard, itemController);
        }
    }
}

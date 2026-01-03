using EFT.InventoryLogic;

namespace BarterItemsStacksClient
{
    internal class InteractionsHandlerClassExtensions
    {
        public static GStruct154<RemoveOneFromStack.RemoveOneFromStackResult> RemoveOneFromStack(
        Item item,
        TraderControllerClass itemController,
        bool simulate)
        {
            if (item == null)
            {
                return new GClass1522("Item is null");
            }
                
            var from = item.CurrentAddress;

            var originalCount = item.StackObjectsCount;

            if (originalCount <= 0)
            {
                return new GClass1522("Invalid StackObjectsCount");
            }
                
            GStruct154<GClass3408> discard = default;

            if (originalCount == 1)
            {
                discard = InteractionsHandlerClass.Discard(item, itemController, false);
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

            return new RemoveOneFromStack.RemoveOneFromStackResult(item, from, discard, itemController);
        }
    }
}

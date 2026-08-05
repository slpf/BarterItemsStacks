using Diz.LanguageExtensions;
using EFT.InventoryLogic;

namespace BarterItemsStacksClient.RemoveOneFromStack
{
    public class RemoveOneFromStackResult(
        Item item,
        ItemAddress from,
        OperationResult<DiscardResult> discard,
        ItemController itemController)
        : IItemOperationResult, ISyncOperationResult
    {
        public Item Item => item;
        public Item ResultItem => item;
        public bool IsDiscard => discard.Succeeded && discard.Value != null;

        public ItemAddress From { get; } = from;

        public ItemController ItemController { get; } = itemController;

        public bool CanExecute(ItemController itemController)
        {
            return item != null && item.StackObjectsCount > 0;
        }

        public OperationResult Execute()
        {
            return InteractionsHandlerClassExtensions.RemoveOneFromStack(item, ItemController, simulate: false);
        }

        public void RaiseEvents(IItemOwner controller, CommandStatus status)
        {
            if (discard.Succeeded && discard.Value != null)
            {
                discard.Value.RaiseEvents(controller, status);
                return;
            }

            item?.RaiseRefreshEvent(false, true);
        }

        public void RollBack()
        {
            if (discard.Succeeded && discard.Value != null)
            {
                discard.Value.RollBack();
                return;
            }

            if (item == null) return;

            item.StackObjectsCount = item.StackObjectsCount + 1;
            item.RaiseRefreshEvent(false, true);
        }

        public RemoveOneFromStackModel ToRemoveOneFromStackModel()
        {
            return new RemoveOneFromStackModel(item.Id);
        }
    }
}

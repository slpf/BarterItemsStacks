using EFT.InventoryLogic;

namespace BarterItemsStacksClient.RemoveOneFromStack
{
    public class RemoveOneFromStackResult : IExecute, IRaiseEvents, IRollback, GInterface424, GInterface433
    {
        private readonly Item _item;
        private readonly GStruct154<GClass3408> _discard;

        public RemoveOneFromStackResult(Item item, ItemAddress from, GStruct154<GClass3408> discard, TraderControllerClass itemController)
        {
            _item = item;
            From = from;
            _discard = discard;
            ItemController = itemController;
        }

        public Item Item => _item;
        public Item ResultItem => _item;
        public bool IsDiscard => _discard.Succeeded && _discard.Value != null;

        public ItemAddress From { get; }

        public TraderControllerClass ItemController { get; }

        public bool CanExecute(TraderControllerClass itemController)
        {
            return _item != null && _item.StackObjectsCount > 0;
        }

        public GStruct153 Execute()
        {
            return InteractionsHandlerClassExtensions.RemoveOneFromStack(_item, ItemController, simulate: false);
        }

        public void RaiseEvents(IItemOwner controller, CommandStatus status)
        {
            if (_discard.Succeeded && _discard.Value != null)
            {
                _discard.Value.RaiseEvents(controller, status);
                return;
            }

            _item?.RaiseRefreshEvent(false, true);
        }

        public void RollBack()
        {
            if (_discard.Succeeded && _discard.Value != null)
            {
                _discard.Value.RollBack();
                return;
            }

            if (_item == null) return;

            _item.StackObjectsCount = _item.StackObjectsCount + 1;
            _item.RaiseRefreshEvent(false, true);
        }

        public RemoveOneFromStackModel ToRemoveOneFromStackModel()
        {
            return new RemoveOneFromStackModel(_item.Id);
        }
    }
}

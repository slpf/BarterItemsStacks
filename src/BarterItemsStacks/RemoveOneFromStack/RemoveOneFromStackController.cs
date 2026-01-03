using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;
using SPTarkov.Server.Core.Routers;
using SPTarkov.Server.Core.Utils;

namespace BarterItemsStacks.RemoveOneFromStack
{
    [Injectable]
    public class RemoveOneFromStackController(EventOutputHolder eventOutputHolder, InventoryHelper inventoryHelper, HttpResponseUtil httpResponseUtil)
    {
        public async ValueTask<ItemEventRouterResponse> RemoveOneFromStack(PmcData pmcData, RemoveOneFromStackModel body, string sessionId)
        {
            var output = eventOutputHolder.GetOutput(sessionId);

            if (body == null || body.Item == null)
            {
                return httpResponseUtil.AppendErrorToOutput(output, "Missing data in body");
            }

            var itemId = body.Item.Value;

            var item = pmcData.Inventory?.Items?.FirstOrDefault(i => i.Id == itemId);

            if (item == null)
            {
                return httpResponseUtil.AppendErrorToOutput(output, "Item not found: " + itemId);
            }

            item.AddUpd();

            var cur = item.Upd != null ? item.Upd.StackObjectsCount : 1;

            if (cur > 1)
            {
                item.Upd.StackObjectsCount = cur - 1;

                return output;
            }

            inventoryHelper.RemoveItem(pmcData, itemId, sessionId, output);

            return output;
        }
    }
}

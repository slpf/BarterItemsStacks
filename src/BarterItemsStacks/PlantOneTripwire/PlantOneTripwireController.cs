using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Helpers.Profile;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;
using SPTarkov.Server.Core.Routers;
using SPTarkov.Server.Core.Utils;

namespace BarterItemsStacks.PlantOneTripwire
{
    [Injectable]
    public class PlantOneTripwireController(EventOutputHolder eventOutputHolder, InventoryHelper inventoryHelper, HttpResponseUtil httpResponseUtil)
    {
        public async ValueTask<ItemEventRouterResponse> PlantOneTripwire(PmcData pmcData, PlantOneTripwireModel body, string sessionId, CancellationToken cancellationToken)
        {
            var output = eventOutputHolder.GetOutput(sessionId);

            if (body == null || body.Tripwire == null)
            {
                return httpResponseUtil.AppendErrorToOutput(output, "Missing data in body");
            }

            ConsumeOne(pmcData, body.Tripwire.Value, sessionId, output);

            if (body.PlantingKit != null)
            {
                ConsumeOne(pmcData, body.PlantingKit.Value, sessionId, output);
            }

            return output;
        }

        private void ConsumeOne(PmcData pmcData, MongoId itemId, string sessionId, ItemEventRouterResponse output)
        {
            var item = pmcData.Inventory?.Items?.FirstOrDefault(i => i.Id == itemId);
            if (item == null)
            {
                return;
            }

            item.AddUpd();

            var cur = item.Upd != null ? item.Upd.StackObjectsCount : 1;

            if (cur > 1)
            {
                item.Upd.StackObjectsCount = cur - 1;
            }
            else
            {
                inventoryHelper.RemoveItem(pmcData, itemId, sessionId, output);
            }
        }
    }
}

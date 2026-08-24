using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Helpers.Profile;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;
using SPTarkov.Server.Core.Routers;
using SPTarkov.Server.Core.Utils;

namespace BarterItemsStacks.PlantOneTripwire
{
    [Injectable]
    public class PlantOneTripwireController(EventOutputHolder eventOutputHolder, InventoryHelper inventoryHelper, HttpResponseUtil httpResponseUtil)
    {
        public ValueTask<ItemEventRouterResponse> PlantOneTripwire(PmcData pmcData, PlantOneTripwireModel body, string sessionId, CancellationToken cancellationToken)
        {
            ItemEventRouterResponse output = eventOutputHolder.GetOutput(sessionId);

            if (body == null || body.Tripwire == null)
            {
                return ValueTask.FromResult(httpResponseUtil.AppendErrorToOutput(output, "Missing data in body"));
            }

            Item? tripwire = pmcData.Inventory?.Items?.FirstOrDefault(item => item.Id == body.Tripwire.Value);
            if (tripwire == null)
            {
                return ValueTask.FromResult(httpResponseUtil.AppendErrorToOutput(output, $"Tripwire item not found: {body.Tripwire.Value}"));
            }

            Item? plantingKit = null;

            if (body.PlantingKit is MongoId plantingKitId)
            {
                if (plantingKitId == tripwire.Id)
                {
                    return ValueTask.FromResult(httpResponseUtil.AppendErrorToOutput(output, "Tripwire and planting kit must be different items"));
                }

                plantingKit = pmcData.Inventory?.Items?.FirstOrDefault(item => item.Id == plantingKitId);
                if (plantingKit == null)
                {
                    return ValueTask.FromResult(httpResponseUtil.AppendErrorToOutput(output, $"Planting kit not found: {plantingKitId}"));
                }
            }

            ConsumeOne(pmcData, tripwire, sessionId, output);

            if (plantingKit != null)
            {
                ConsumeOne(pmcData, plantingKit, sessionId, output);
            }

            return ValueTask.FromResult(output);
        }

        private void ConsumeOne(PmcData pmcData, Item item, string sessionId, ItemEventRouterResponse output)
        {
            item.AddUpd();

            Upd itemUpdate = item.Upd!;
            double currentCount = itemUpdate.StackObjectsCount ?? 1;

            if (currentCount > 1)
            {
                itemUpdate.StackObjectsCount = currentCount - 1;
            }
            else
            {
                inventoryHelper.RemoveItem(pmcData, item.Id, sessionId, output);
            }
        }
    }
}

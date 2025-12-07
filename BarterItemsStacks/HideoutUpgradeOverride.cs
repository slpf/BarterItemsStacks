using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Controllers;
using SPTarkov.Server.Core.Generators;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Hideout;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Routers;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Cloners;

namespace SLPF.BarterItemsStacks
{
    [Injectable]
    public class HideoutUpgradeOverride(
    ISptLogger<HideoutController> logger,
    TimeUtil timeUtil,
    DatabaseService databaseService,
    InventoryHelper inventoryHelper,
    ItemHelper itemHelper,
    SaveServer saveServer,
    PresetHelper presetHelper,
    PaymentHelper paymentHelper,
    EventOutputHolder eventOutputHolder,
    HttpResponseUtil httpResponseUtil,
    ProfileHelper profileHelper,
    HideoutHelper hideoutHelper,
    ScavCaseRewardGenerator scavCaseRewardGenerator,
    ServerLocalisationService serverLocalisationService,
    ProfileActivityService profileActivityService,
    FenceService fenceService,
    CircleOfCultistService circleOfCultistService,
    ICloner cloner,
    ConfigServer configServer) : HideoutController(logger, timeUtil, databaseService, inventoryHelper, itemHelper,
        saveServer, presetHelper, paymentHelper, eventOutputHolder, httpResponseUtil, profileHelper, hideoutHelper,
        scavCaseRewardGenerator, serverLocalisationService, profileActivityService, fenceService, circleOfCultistService, cloner, configServer)
    {
        public override void StartUpgrade(PmcData pmcData, HideoutUpgradeRequestData request, MongoId sessionID, ItemEventRouterResponse output)
        {
            var items = request
                .Items.Select(reqItem =>
                {
                    var item = pmcData.Inventory.Items.FirstOrDefault(invItem => invItem.Id == reqItem.Id);
                    return new { inventoryItem = item, requestedItem = reqItem };
                })
                .ToList();

            foreach (var item in items)
            {
                if (item.inventoryItem is null)
                {
                    logger.Error(serverLocalisationService.GetText("hideout-unable_to_find_item_in_inventory", item.requestedItem.Id));
                    httpResponseUtil.AppendErrorToOutput(output);

                    return;
                }

                if (
                    paymentHelper.IsMoneyTpl(item.inventoryItem.Template)
                    && item.inventoryItem.Upd is not null
                    && item.inventoryItem.Upd.StackObjectsCount is not null
                    && item.inventoryItem.Upd.StackObjectsCount > item.requestedItem.Count
                )
                {
                    item.inventoryItem.Upd.StackObjectsCount -= item.requestedItem.Count;
                }
                else if (
                    item.inventoryItem.Upd is not null
                    && itemHelper.GetItem(item.inventoryItem.Template) is { Key: true, Value: { Properties: { StackMaxSize: > 1 } } }
                    && item.inventoryItem.Upd.StackObjectsCount is not null
                    && item.inventoryItem.Upd.StackObjectsCount > item.requestedItem.Count)
                {
                    item.inventoryItem.Upd.StackObjectsCount -= item.requestedItem.Count;
                }
                else
                {
                    inventoryHelper.RemoveItem(pmcData, item.inventoryItem.Id, sessionID, output);
                }
            }

            var profileHideoutArea = pmcData.Hideout.Areas.FirstOrDefault(area => area.Type == request.AreaType);
            if (profileHideoutArea is null)
            {
                logger.Error(serverLocalisationService.GetText("hideout-unable_to_find_area", request.AreaType));
                httpResponseUtil.AppendErrorToOutput(output);

                return;
            }

            var hideoutDataDb = databaseService.GetTables().Hideout.Areas.FirstOrDefault(area => area.Type == request.AreaType);
            if (hideoutDataDb is null)
            {
                logger.Error(serverLocalisationService.GetText("hideout-unable_to_find_area_in_database", request.AreaType));
                httpResponseUtil.AppendErrorToOutput(output);

                return;
            }

            var ctime = hideoutDataDb.Stages[(profileHideoutArea.Level + 1).ToString()].ConstructionTime;
            if (ctime > 0)
            {
                if (profileHelper.IsDeveloperAccount(sessionID))
                {
                    ctime = 40;
                }

                var timestamp = timeUtil.GetTimeStamp();

                profileHideoutArea.CompleteTime = (int)Math.Round(timestamp + ctime.Value);
                profileHideoutArea.Constructing = true;
            }
        }
    }
}

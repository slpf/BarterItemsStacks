using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Controllers;
using SPTarkov.Server.Core.Generators;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Hideout;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Routers;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Cloners;

namespace BarterItemsStacks
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

        public override ItemEventRouterResponse PutItemsInAreaSlots(PmcData pmcData, HideoutPutItemInRequestData addItemToHideoutRequest, MongoId sessionID)
        {
            var output = eventOutputHolder.GetOutput(sessionID);

            var itemsToAdd = addItemToHideoutRequest.Items.Select(kvp =>
            {
                var item = pmcData.Inventory.Items.FirstOrDefault(invItem => invItem.Id == kvp.Value.Id);
                return new
                {
                    inventoryItem = item,
                    requestedItem = kvp.Value,
                    slot = kvp.Key,
                };
            });

            var hideoutArea = pmcData.Hideout.Areas.FirstOrDefault(area => area.Type == addItemToHideoutRequest.AreaType);
            if (hideoutArea is null)
            {
                logger.Error(serverLocalisationService.GetText("hideout-unable_to_find_area_in_database", addItemToHideoutRequest.AreaType));

                return httpResponseUtil.AppendErrorToOutput(output);
            }

            foreach (var item in itemsToAdd)
            {
                if (item.inventoryItem is null)
                {
                    logger.Error(
                        serverLocalisationService.GetText(
                            "hideout-unable_to_find_item_in_inventory",
                            new { itemId = item.requestedItem.Id, area = hideoutArea.Type }
                        )
                    );
                    return httpResponseUtil.AppendErrorToOutput(output);
                }

                var destinationLocationIndex = int.Parse(item.slot);
                var hideoutSlotIndex = hideoutArea.Slots.FindIndex(slot => slot.LocationIndex == destinationLocationIndex);
                if (hideoutSlotIndex == -1)
                {
                    logger.Error(
                        $"Unable to put item: {item.requestedItem.Id} into slot as slot cannot be found for area: {addItemToHideoutRequest.AreaType}, skipping"
                    );
                    continue;
                }

                if (item.inventoryItem.Upd is not null
                    && itemHelper.GetItem(item.inventoryItem.Template) is { Key: true, Value: { Properties: { StackMaxSize: > 1 } } }
                    && item.inventoryItem.Upd.StackObjectsCount is not null
                    && item.inventoryItem.Upd.StackObjectsCount > item.requestedItem.Count)
                {
                    var upd = System.Text.Json.JsonSerializer.Deserialize<Upd>(System.Text.Json.JsonSerializer.Serialize(item.inventoryItem.Upd));
                    upd.StackObjectsCount = 1;

                    hideoutArea.Slots[hideoutSlotIndex].Items =
                    [
                        new HideoutItem
                            {
                                Id = new MongoId(),
                                Template = item.inventoryItem.Template,
                                Upd = upd,
                            },
                    ];

                    item.inventoryItem.Upd.StackObjectsCount -= item.requestedItem.Count;
                    output.ProfileChanges[sessionID].Items.ChangedItems.Add(item.inventoryItem);
                }
                else
                {
                    hideoutArea.Slots[hideoutSlotIndex].Items =
                    [
                        new HideoutItem
                            {
                                Id = item.inventoryItem.Id,
                                Template = item.inventoryItem.Template,
                                Upd = item.inventoryItem.Upd,
                            },
                    ];

                    inventoryHelper.RemoveItem(pmcData, item.inventoryItem.Id, sessionID, output);
                }
            }

            hideoutHelper.UpdatePlayerHideout(sessionID);

            return output;
        }
    }
}

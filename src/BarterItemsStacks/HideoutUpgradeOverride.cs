using System.Reflection;
using System.Text.Json;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Reflection.Patching;
using SPTarkov.Server.Core.Controllers;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers.Commerce;
using SPTarkov.Server.Core.Helpers.Items;
using SPTarkov.Server.Core.Helpers.Profile;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Hideout;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;
using SPTarkov.Server.Core.Models.Spt.Tables;
using SPTarkov.Server.Core.Routers;
using SPTarkov.Server.Core.Services.Locales;
using SPTarkov.Server.Core.Utils;

namespace BarterItemsStacks
{
    [Injectable]
    public class StartUpgradePatch : AbstractPatch
    {
        private static ISptLogger<StartUpgradePatch> _logger = default!;
        private static ServerLocalisationService _serverLocalisationService = default!;
        private static HttpResponseUtil _httpResponseUtil = default!;
        private static PaymentHelper _paymentHelper = default!;
        private static ItemHelper _itemHelper = default!;
        private static InventoryHelper _inventoryHelper = default!;
        private static HideoutTable _hideoutTable = default!;
        private static TimeUtil _timeUtil = default!;
        private static ProfileHelper _profileHelper = default!;

        public StartUpgradePatch(
            ISptLogger<StartUpgradePatch> logger,
            ServerLocalisationService serverLocalisationService,
            HttpResponseUtil httpResponseUtil,
            PaymentHelper paymentHelper,
            ItemHelper itemHelper,
            InventoryHelper inventoryHelper,
            HideoutTable hideoutTable,
            TimeUtil timeUtil,
            ProfileHelper profileHelper) : base()
        {
            _logger = logger;
            _serverLocalisationService = serverLocalisationService;
            _httpResponseUtil = httpResponseUtil;
            _paymentHelper = paymentHelper;
            _itemHelper = itemHelper;
            _inventoryHelper = inventoryHelper;
            _hideoutTable = hideoutTable;
            _timeUtil = timeUtil;
            _profileHelper = profileHelper;
        }

        protected override MethodBase? GetTargetMethod()
        {
            return typeof(HideoutController).GetMethod(nameof(HideoutController.StartUpgrade));
        }

        [PatchPrefix]
        public static bool Prefix(PmcData pmcData, HideoutUpgradeRequestData request, MongoId sessionID, ItemEventRouterResponse output)
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
                    _logger.Error(_serverLocalisationService.GetText("hideout-unable_to_find_item_in_inventory", item.requestedItem.Id));
                    _httpResponseUtil.AppendErrorToOutput(output);

                    return false;
                }

                if (
                    _paymentHelper.IsMoneyTpl(item.inventoryItem.Template)
                    && item.inventoryItem.Upd is not null
                    && item.inventoryItem.Upd.StackObjectsCount is not null
                    && item.inventoryItem.Upd.StackObjectsCount > item.requestedItem.Count
                )
                {
                    item.inventoryItem.Upd.StackObjectsCount -= item.requestedItem.Count;
                }
                else if (
                    item.inventoryItem.Upd is not null
                    && _itemHelper.GetItem(item.inventoryItem.Template) is { Key: true, Value: { Properties: { StackMaxSize: > 1 } } }
                    && item.inventoryItem.Upd.StackObjectsCount is not null
                    && item.inventoryItem.Upd.StackObjectsCount > item.requestedItem.Count)
                {
                    item.inventoryItem.Upd.StackObjectsCount -= item.requestedItem.Count;
                }
                else
                {
                    _inventoryHelper.RemoveItem(pmcData, item.inventoryItem.Id, sessionID, output);
                }
            }

            var profileHideoutArea = pmcData.Hideout.Areas.FirstOrDefault(area => area.Type == request.AreaType);
            if (profileHideoutArea is null)
            {
                _logger.Error(_serverLocalisationService.GetText("hideout-unable_to_find_area", request.AreaType));
                _httpResponseUtil.AppendErrorToOutput(output);

                return false;
            }

            var hideoutDataDb = _hideoutTable.Areas.FirstOrDefault(area => area.Type == request.AreaType);
            if (hideoutDataDb is null)
            {
                _logger.Error(_serverLocalisationService.GetText("hideout-unable_to_find_area_in_database", request.AreaType));
                _httpResponseUtil.AppendErrorToOutput(output);

                return false;
            }

            var ctime = hideoutDataDb.Stages[(profileHideoutArea.Level + 1).ToString()].ConstructionTime;
            if (ctime > 0)
            {
                if (_profileHelper.IsDeveloperAccount(sessionID))
                {
                    ctime = 40;
                }

                var timestamp = _timeUtil.GetTimeStamp();

                profileHideoutArea.CompleteTime = (int)Math.Round(timestamp + ctime.Value);
                profileHideoutArea.Constructing = true;
            }

            return false;
        }
    }

    [Injectable]
    public class PutItemsInAreaSlotsPatch : AbstractPatch
    {
        private static ISptLogger<PutItemsInAreaSlotsPatch> _logger = default!;
        private static ServerLocalisationService _serverLocalisationService = default!;
        private static HttpResponseUtil _httpResponseUtil = default!;
        private static ItemHelper _itemHelper = default!;
        private static InventoryHelper _inventoryHelper = default!;
        private static HideoutHelper _hideoutHelper = default!;
        private static EventOutputHolder _eventOutputHolder = default!;

        public PutItemsInAreaSlotsPatch(
            ISptLogger<PutItemsInAreaSlotsPatch> logger,
            ServerLocalisationService serverLocalisationService,
            HttpResponseUtil httpResponseUtil,
            ItemHelper itemHelper,
            InventoryHelper inventoryHelper,
            HideoutHelper hideoutHelper,
            EventOutputHolder eventOutputHolder) : base()
        {
            _logger = logger;
            _serverLocalisationService = serverLocalisationService;
            _httpResponseUtil = httpResponseUtil;
            _itemHelper = itemHelper;
            _inventoryHelper = inventoryHelper;
            _hideoutHelper = hideoutHelper;
            _eventOutputHolder = eventOutputHolder;
        }

        protected override MethodBase? GetTargetMethod()
        {
            return typeof(HideoutController).GetMethod(nameof(HideoutController.PutItemsInAreaSlots));
        }

        [PatchPrefix]
        public static bool Prefix(PmcData pmcData, HideoutPutItemInRequestData addItemToHideoutRequest, MongoId sessionID, ref ItemEventRouterResponse __result)
        {
            var output = _eventOutputHolder.GetOutput(sessionID);

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
                _logger.Error(_serverLocalisationService.GetText("hideout-unable_to_find_area_in_database", addItemToHideoutRequest.AreaType));

                __result = _httpResponseUtil.AppendErrorToOutput(output);
                return false;
            }

            foreach (var item in itemsToAdd)
            {
                if (item.inventoryItem is null)
                {
                    _logger.Error(
                        _serverLocalisationService.GetText(
                            "hideout-unable_to_find_item_in_inventory",
                            new { itemId = item.requestedItem.Id, area = hideoutArea.Type }
                        )
                    );
                    __result = _httpResponseUtil.AppendErrorToOutput(output);
                    return false;
                }

                var destinationLocationIndex = int.Parse(item.slot);
                var hideoutSlotIndex = hideoutArea.Slots.FindIndex(slot => slot.LocationIndex == destinationLocationIndex);
                if (hideoutSlotIndex == -1)
                {
                    _logger.Error(
                        $"Unable to put item: {item.requestedItem.Id} into slot as slot cannot be found for area: {addItemToHideoutRequest.AreaType}, skipping"
                    );
                    continue;
                }

                if (item.inventoryItem.Upd is not null
                    && _itemHelper.GetItem(item.inventoryItem.Template) is { Key: true, Value: { Properties: { StackMaxSize: > 1 } } }
                    && item.inventoryItem.Upd.StackObjectsCount is not null
                    && item.inventoryItem.Upd.StackObjectsCount > item.requestedItem.Count)
                {
                    var upd = JsonSerializer.Deserialize<Upd>(JsonSerializer.Serialize(item.inventoryItem.Upd));
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

                    _inventoryHelper.RemoveItem(pmcData, item.inventoryItem.Id, sessionID, output);
                }
            }

            _hideoutHelper.UpdatePlayerHideout(sessionID);

            __result = output;
            return false;
        }
    }

    [Injectable(TypePriority = OnLoadOrder.Preload + 1)]
    public class HideoutPatchesEnabler(IEnumerable<IRuntimePatch> patches) : IOnLoad
    {
        public Task OnLoadAsync(CancellationToken cancellationToken)
        {
            foreach (var patch in patches)
            {
                patch.Enable();
            }

            return Task.CompletedTask;
        }
    }
}

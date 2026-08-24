using System.Reflection;
using SPTarkov.DI.Annotations;
using SPTarkov.Reflection.Patching;
using SPTarkov.Server.Core.Controllers;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Helpers.Profile;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Inventory;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;
using SPTarkov.Server.Core.Models.Spt.Inventory;

namespace BarterItemsStacks;

[Injectable]
public sealed class TransferItemFirPatch : AbstractPatch
{
    private static InventoryHelper _inventoryHelper = default!;

    public TransferItemFirPatch(InventoryHelper inventoryHelper)
    {
        _inventoryHelper = inventoryHelper;
    }

    protected override MethodBase? GetTargetMethod()
    {
        return typeof(InventoryController).GetMethod(
            nameof(InventoryController.TransferItem),
            [
                typeof(PmcData),
                typeof(InventoryTransferRequestData),
                typeof(MongoId),
                typeof(ItemEventRouterResponse),
            ]);
    }

    [PatchPrefix]
    public static void Prefix(InventoryTransferRequestData __1, MongoId __2, out Item? __state)
    {
        OwnerInventoryItems inventories = _inventoryHelper.GetOwnerInventoryItems(__1, __1.Item, __2);
        Item? source = inventories.From?.FirstOrDefault(item => item.Id == __1.Item);
        Item? target = inventories.To?.FirstOrDefault(item => item.Id == __1.With);

        if (source == null || target == null)
        {
            __state = null;
            return;
        }

        bool sourceFir = source.Upd?.SpawnedInSession ?? false;
        bool targetFir = target.Upd?.SpawnedInSession ?? false;
        __state = sourceFir != targetFir ? target : null;
    }

    [PatchPostfix]
    public static void Postfix(Item? __state)
    {
        if (__state == null)
        {
            return;
        }

        __state.AddUpd();
        __state.Upd!.SpawnedInSession = false;
    }
}

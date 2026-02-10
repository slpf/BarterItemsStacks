using System;
using System.Collections.Generic;
using System.Reflection;
using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace BarterItemsStacksClient.Patches.UIGridItemView;

public class SortComparatorPatch : ModulePatch
{
    private static readonly Dictionary<string, int> RarityOrder = new(StringComparer.OrdinalIgnoreCase)
    {
        { "#10a43a", 0 },   // UNKNOWN - dark green
        { "#ca741f", 1 },   // CUSTOM2 - orange
        { "#ca1f2b", 2 },   // CUSTOM - dark red
        { "#FF4040", 3 },   // OVERPOWERED - red (banned)
        { "#39FF14", 4 },   // UNOBTAINIUM - green
        { "#FFD700", 5 },   // UBER - gold
        { "#f6f15d", 6 },   // LEGENDARY - yellow
        { "#9F5ACF", 7 },   // EPIC - violet
        { "#2694da", 8 },   // RARE - blue
        { "#FFFFFF", 9 },   // COMMON - white
    };
    
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(AccessTools.TypeByName("GClass3381+Class2438"), "Compare");
    }
    
    [PatchPrefix]
    private static bool Prefix(Item x, Item y, ref int __result)
    {
        int num;
        
        int rarityX = GetRarityOrder(x);
        int rarityY = GetRarityOrder(y);
        num = rarityX.CompareTo(rarityY);
        if (num != 0)
        {
            __result = num;
            return false;
        }
        
        if (x.Template is AmmoTemplate ammoX && y.Template is AmmoTemplate ammoY)
        {
            num = string.Compare(ammoX.Caliber, ammoY.Caliber, StringComparison.OrdinalIgnoreCase);
            if (num != 0)
            {
                __result = num;
                return false;
            }
        }
        
        string nameX = x.ShortName.Localized();
        string nameY = y.ShortName.Localized();
        num = string.Compare(nameX, nameY, StringComparison.OrdinalIgnoreCase);
        if (num != 0)
        {
            __result = num;
            return false;
        }
        
        num = y.StackObjectsCount.CompareTo(x.StackObjectsCount);
        if (num != 0)
        {
            __result = num;
            return false;
        }
        
        float xResource = GetResourcePercent(x);
        float yResource = GetResourcePercent(y);
        num = yResource.CompareTo(xResource);
        if (num != 0)
        {
            __result = num;
            return false;
        }
        
        if (x.TryGetItemComponent<DogtagComponent>(out var dogtagX) && 
            y.TryGetItemComponent<DogtagComponent>(out var dogtagY))
        {
            num = dogtagY.Level.CompareTo(dogtagX.Level);
            if (num != 0)
            {
                __result = num;
                return false;
            }
            
            num = dogtagY.Time.CompareTo(dogtagX.Time);
            if (num != 0)
            {
                __result = num;
                return false;
            }
        }
        
        __result = string.Compare(x.Id, y.Id, StringComparison.Ordinal);
        return false;
    }

    private static int GetRarityOrder(Item item)
    {
        try
        {
            if (RarityOrder.TryGetValue(item.BackgroundColor.ToString(), out int order))
            {
                return order;
            }
        }
        catch
        {
            // ignore
        }
        
        return 10;
    }

    private static float GetResourcePercent(Item item)
    {
        var resource = item.GetItemComponent<ResourceComponent>();
        if (resource != null && resource.MaxResource > 0)
        {
            return resource.Value / resource.MaxResource;
        }

        var medkit = item.GetItemComponent<MedKitComponent>();
        if (medkit != null && medkit.MaxHpResource > 0)
        {
            return medkit.HpResource / medkit.MaxHpResource;
        }

        var food = item.GetItemComponent<FoodDrinkComponent>();
        if (food != null && food.MaxResource > 0)
        {
            return food.HpPercent / food.MaxResource;
        }

        var repair = item.GetItemComponent<RepairKitComponent>();
        if (repair != null)
        {
            var max = ((RepairKitsTemplateClass)item.Template).MaxRepairResource;
            if (max > 0)
            {
                return repair.Resource / max;
            }
        }
        
        return 1f;
    }
}
using System;
using System.Collections.Generic;
using System.Reflection;
using EFT.InventoryLogic;
using HarmonyLib;
using JsonType;
using SPT.Reflection.Patching;

namespace BarterItemsStacksClient.Patches.UIGridItemView;

public class SortComparatorPatch : ModulePatch
{
    private const int TaxonomyColorOffset = 12;
    
    private static readonly Dictionary<string, int> RarityOrder = new()
    {
        { "1090630", 0 },    // UNKNOWN
        { "13268011", 1 },   // CUSTOM2
        { "13246263", 2 },   // CUSTOM
        { "16728140", 3 },   // OVERPOWERED
        { "3800864", 4 },    // UNOBTAINIUM
        { "16766732", 5 },   // UBER
        { "16183657", 6 },   // LEGENDARY
        { "10443483", 7 },   // EPIC
        { "2528486", 8 },    // RARE
        { "16777227", 9 },   // COMMON
    };
    
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(AccessTools.TypeByName("GClass3381+Class2438"), "Compare");
    }
    
    [PatchPrefix]
    private static bool Prefix(Item x, Item y, ref int __result)
    {
        int num;
        
        if (x.Template is AmmoTemplate ammoX && y.Template is AmmoTemplate ammoY)
        {
            num = string.Compare(ammoX.Caliber, ammoY.Caliber, StringComparison.OrdinalIgnoreCase);
            if (num != 0)
            {
                __result = num;
                return false;
            }
        }
        
        int rarityX = GetRarityOrder(x);
        int rarityY = GetRarityOrder(y);
        num = rarityX.CompareTo(rarityY);
        if (num != 0)
        {
            __result = num;
            return false;
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
        if (RarityOrder.TryGetValue(item.BackgroundColor.ToString(), out int order))
        {
            return order;
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
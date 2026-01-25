using System.Reflection;
using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace BarterItemsStacksClient.Patches.UIGridItemView;

public class SortComparatorPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(AccessTools.TypeByName("GClass3381+Class2438"), "Compare");
    }
    
    [PatchPrefix]
    private static bool Prefix(Item x, Item y, ref int __result)
    {
        int num = string.Compare(x.TemplateId, y.TemplateId, System.StringComparison.Ordinal);
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
        
        return true;
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
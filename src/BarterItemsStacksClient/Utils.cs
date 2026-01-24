using EFT.InventoryLogic;
using System;

namespace BarterItemsStacksClient
{
    internal class Utils
    {
        internal static bool TryGetResource(Item item, out float cur, out float max)
        {
            cur = 0f;
            max = 0f;

            var resource = item.GetItemComponent<ResourceComponent>();
            if (resource != null)
            {
                cur = resource.Value;
                max = resource.MaxResource;
                return true;
            }

            var medkit = item.GetItemComponent<MedKitComponent>();
            if (medkit != null)
            {
                cur = medkit.HpResource;
                max = medkit.MaxHpResource;
                return true;
            }

            var food = item.GetItemComponent<FoodDrinkComponent>();
            if (food != null)
            {
                cur = food.HpPercent;
                max = food.MaxResource;
                return true;
            }

            var repair = item.GetItemComponent<RepairKitComponent>();
            if (repair != null)
            {
                cur = repair.Resource;
                max = ((RepairKitsTemplateClass)item.Template).MaxRepairResource;
                return true;
            }

            return false;
        }
        
        internal static bool IsFullResource(Item item)
        {
            if (!TryGetResource(item, out var cur, out var max))
            {
                return true;
            }

            return cur >= max - 0.5f;
        }
        
        internal static bool CanMergeResources(Item item, Item targetItem)
        {
            bool itemHasResource = TryGetResource(item, out var aCur, out var aMax);
            bool targetHasResource = TryGetResource(targetItem, out var bCur, out var bMax);

            if (!itemHasResource || !targetHasResource)
            {
                return true;
            }

            return aCur >= aMax - 0.5f && bCur >= bMax - 0.5f;
        }
        
        internal static bool CanIgnoreFirStatus(Item item, Item targetItem)
        {
            if (Settings.FirStackableResources.Value && item is BarterItemItemClass && targetItem is BarterItemItemClass)
            {
                return true;
            }

            if (Settings.FirStackableMed.Value && item is MedsItemClass && targetItem is MedsItemClass)
            {
                return true;
            }

            if (Settings.FirStackableFoodDrinks.Value && item is FoodDrinkItemClass && targetItem is FoodDrinkItemClass)
            {
                return true;
            }

            if (Settings.FirStackableRepairKits.Value && item is RepairKitsItemClass && targetItem is RepairKitsItemClass)
            {
                return true;
            }

            return false;
        }
    }
}

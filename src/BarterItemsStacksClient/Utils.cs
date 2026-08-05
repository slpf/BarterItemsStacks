using EFT.InventoryLogic;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BarterItemsStacksClient
{
    internal static class Utils
    {
        private static bool TryGetResource(Item item, out float cur, out float max)
        {
            cur = 0f;
            max = 0f;

            ResourceComponent resource = item.GetItemComponent<ResourceComponent>();
            if (resource != null)
            {
                cur = resource.Value;
                max = resource.MaxResource;
                return true;
            }

            MedKitComponent medkit = item.GetItemComponent<MedKitComponent>();
            if (medkit != null)
            {
                cur = medkit.HpResource;
                max = medkit.MaxHpResource;
                return true;
            }

            FoodDrinkComponent food = item.GetItemComponent<FoodDrinkComponent>();
            if (food != null)
            {
                cur = food.HpPercent;
                max = food.MaxResource;
                return true;
            }

            RepairKitComponent repair = item.GetItemComponent<RepairKitComponent>();
            if (repair != null)
            {
                cur = repair.Resource;
                max = ((RepairKitTemplate) item.Template).MaxRepairResource;
                return true;
            }

            return false;
        }
        
        internal static bool IsFullResource(Item item)
        {
            if (!TryGetResource(item, out float cur, out float max))
            {
                return true;
            }

            return cur >= max - 0.5f;
        }
        
        internal static bool CanMergeResources(Item item, Item targetItem)
        {
            bool itemHasResource = TryGetResource(item, out float aCur, out float aMax);
            bool targetHasResource = TryGetResource(targetItem, out float bCur, out float bMax);

            if (!itemHasResource || !targetHasResource)
            {
                return true;
            }

            return aCur >= aMax - 0.5f && bCur >= bMax - 0.5f;
        }
        
        internal static bool CanIgnoreFirStatus(Item item, Item targetItem)
        {
            if (Settings.FirStackableResources.Value && item is BarterItem && targetItem is BarterItem)
            {
                return true;
            }

            if (Settings.FirStackableMed.Value && item is Meds && targetItem is Meds)
            {
                return true;
            }

            if (Settings.FirStackableFoodDrinks.Value && item is FoodDrink && targetItem is FoodDrink)
            {
                return true;
            }

            if (Settings.FirStackableRepairKits.Value && item is RepairKit && targetItem is RepairKit)
            {
                return true;
            }

            return false;
        }
        
        public static bool FindStackForMerge(IEnumerable<IContainer> containers, Item itemToMerge, out Item mergeableItem,  int minimumStackSpace = 0)
        {
            bool ignoreFir = CanIgnoreFirStatus(itemToMerge, itemToMerge);

            mergeableItem = containers.SelectMany(x => x.Items)
                .Where(x => x != itemToMerge)
                .Where(x => x.TemplateId == itemToMerge.TemplateId)
                .Where(x => ignoreFir || x.SpawnedInSession == itemToMerge.SpawnedInSession)
                .Where(x => x.StackObjectsCount < x.StackMaxSize)
                .Where(IsFullResource)
                .OrderByDescending(x => x.StackObjectsCount)
                .FirstOrDefault(x => minimumStackSpace <= x.StackMaxSize - x.StackObjectsCount);

            return mergeableItem != null;
        }
    }
}

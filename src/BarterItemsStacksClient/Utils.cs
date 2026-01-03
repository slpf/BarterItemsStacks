using EFT.InventoryLogic;
using System;

namespace BarterItemsStacksClient
{
    internal class Utils
    {
        internal static bool IsFull(float cur, float max)
        {
            const float t = 0.5f;
            return cur >= max - t;
        }

        internal static bool BothFull(float irCur, float irmax, float trCur, float trMax)
        {
            return IsFull(irCur, irmax) && IsFull(trCur, trMax);
        }

        internal static bool TryGet<T>(Item item, out float cur, out float max) where T : IItemComponent
        {
            cur = 0f;
            max = 0f;

            if (typeof(T) == typeof(ResourceComponent))
            {
                var c = item.GetItemComponent<ResourceComponent>();
                if (c == null) 
                    return false;

                cur = c.Value;
                max = c.MaxResource;
                return true;
            }

            if (typeof(T) == typeof(MedKitComponent))
            {
                var c = item.GetItemComponent<MedKitComponent>();
                if (c == null) 
                    return false;

                cur = c.HpResource;
                max = c.MaxHpResource;

                return true;
            }

            if (typeof(T) == typeof(FoodDrinkComponent))
            {
                var c = item.GetItemComponent<FoodDrinkComponent>();
                if (c == null) 
                    return false;

                cur = c.HpPercent;       
                max = c.MaxResource;

                return true;
            }

            if (typeof(T) == typeof(RepairKitComponent))
            {
                var c = item.GetItemComponent<RepairKitComponent>();
                if (c == null) 
                    return false;

                cur = c.Resource;      
                max = ((RepairKitsTemplateClass)item.Template).MaxRepairResource;

                return true;
            }

            throw new NotSupportedException($"TryGet<{typeof(T).Name}> is not supported.");
        }

        internal static bool CheckBothItems<T>(Item item, Item targetItem) where T : IItemComponent
        {
            if (!TryGet<T>(item, out var aCur, out var aMax) ||
                !TryGet<T>(targetItem, out var bCur, out var bMax))
                return true;

            if (BothFull(aCur, aMax, bCur, bMax))
                return true;

            return false;
        }

        internal static bool ShouldSkip<T>(Item item, Item target) where T : IItemComponent
        {
            if (!TryGet<T>(item, out var itemCur, out var itemMax))
                return false;

            if (!TryGet<T>(target, out var targetCur, out var targetMax))
                return false;

            return BothFull(itemCur, itemMax, targetCur, targetMax);
        }
    }
}

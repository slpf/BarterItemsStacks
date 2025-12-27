using System.Reflection;
using System.Runtime.CompilerServices;
using EFT.UI.DragAndDrop;
using HarmonyLib;
using SPT.Reflection.Patching;
using TMPro;
using UnityEngine;

namespace BarterItemsStacksClient.Patches.UIGridItemView
{
    public class CheckmarkPositionPatch : ModulePatch
    {
        private static readonly FieldInfo CaptionField = AccessTools.Field(typeof(GridItemView), "Caption");
        
        private static readonly ConditionalWeakTable<GridItemView, Flag> Done =
            new ConditionalWeakTable<GridItemView, Flag>();
        
        private sealed class Flag { public bool Value; }
        
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(ItemView), nameof(ItemView.Init));
        }
        
        [PatchPostfix]
        public static void Postfix(ItemView __instance)
        {
            var grid = __instance as GridItemView;

            if (grid == null)
            {
                return;
            }
            
            var flag = Done.GetOrCreateValue(grid);

            if (flag.Value)
            {
                return;
            }

            var caption = CaptionField?.GetValue(grid) as TextMeshProUGUI;
            var panel = grid.QuestItemViewPanel_0;

            if (caption == null || panel == null)
            {
                return;
            }

            var panelRect =  panel.transform as RectTransform;
            var captionRect = caption.transform as RectTransform;
            var targetRect = captionRect.parent as RectTransform;
            
            panelRect.SetParent(targetRect, false);
            
            panelRect.anchorMin = new Vector2(1f, 1f);
            panelRect.anchorMax = new Vector2(1f, 1f);
            panelRect.pivot     = new Vector2(1f, 1f);
                
            var capPos = captionRect.anchoredPosition;
                
            const float paddingX = 2f;
            
            panelRect.anchoredPosition = new Vector2(
                capPos.x - paddingX,
                capPos.y - captionRect.rect.height);
            
            flag.Value = true;
        }
    }
}
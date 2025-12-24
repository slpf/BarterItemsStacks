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
        
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GridItemView), nameof(GridItemView.Update));
        }
        
        [PatchPostfix]
        [HarmonyPriority(Priority.Last)]
        public static void Postfix(GridItemView __instance)
        {
            if (__instance == null)
            {
                return;
            }

            var caption = CaptionField?.GetValue(__instance) as TextMeshProUGUI;
            var panel = __instance.QuestItemViewPanel_0;

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
        }
    }
}
using EFT.UI.DragAndDrop;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;

namespace BarterItemsStacksClient.Patches.UIGridItemView
{
    internal class UpdateItemViewPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GridItemView), nameof(GridItemView.UpdateItemValue));
        }

        [PatchPrefix]
        public static void Prefix(GridItemView __instance, ref string newValue)
        {
            int currentStack = __instance.Item.StackObjectsCount;

            if (currentStack < 2 || CheckSlash(newValue) == 0)
                return;

            newValue = $"<color=#b6c1c7>{currentStack}</color>\n{newValue}";
        }

        private static int CheckSlash(string s)
        {
            bool inTag = false;
            int count = 0;

            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];

                if (c == '<') { inTag = true; continue; }
                if (inTag)
                {
                    if (c == '>') inTag = false;
                    continue;
                }

                if (c == '/') count++;
            }

            return count;
        }
    }
}

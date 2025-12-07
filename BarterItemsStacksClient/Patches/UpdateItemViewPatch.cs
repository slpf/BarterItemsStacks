using SPT.Reflection.Patching;
using HarmonyLib;
using System.Reflection;
using EFT.UI.DragAndDrop;
using System.Linq;

namespace BarterItemsStacksClient.Patches
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

            if (currentStack < 2 || newValue.Count(c => c == '/') < 2)
                return;

            newValue = $"{newValue} <color=#b6c1c7>({currentStack})</color>";
        }
    }
}

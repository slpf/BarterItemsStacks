using Diz.LanguageExtensions;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;
using System.Threading.Tasks;

namespace BarterItemsStacksClient.Patches.Compatibility
{
    internal class UIFixesStackAllPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(AccessTools.TypeByName("UIFixes.SortPatches+StackFirstPatch"), "StackAll");
        }

        [PatchPostfix]
        private static void Postfix(ref Task<Error> __result)
        {
            __result = IgnorePreStackErrors(__result);
        }

        private static async Task<Error> IgnorePreStackErrors(Task<Error> original)
        {
            try
            {
                await original.ConfigureAwait(false);
            }
            catch
            {
                // ignore
            }

            return null;
        }
    }
}

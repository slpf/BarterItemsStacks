using Comfort.Common;
using Diz.LanguageExtensions;
using EFT;
using EFT.Interactive;
using EFT.InventoryLogic;
using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Collections;
using System.Reflection;
using BarterItemsStacksClient.RemoveOneFromStack;
using UnityEngine;

namespace BarterItemsStacksClient.Patches.Quest
{
    public class PlaceItemProtectPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Player.PlayerInventoryController), nameof(Player.PlayerInventoryController.SetupItem));
        }

        [PatchPrefix]
        public static bool Prefix(Player.PlayerInventoryController __instance,
            Item item, string zone, Vector3 position, Quaternion rotation, float setupTime, Callback callback)
        {
            if (item.StackObjectsCount <= 1) return true;

            OperationResult<RemoveOneFromStackResult> removeOne = InteractionsHandlerClassExtensions.RemoveOneFromStack(item, __instance, simulate: true);
            if (removeOne.Failed)
            {
                callback?.Invoke(removeOne.ToResult());
                return false;
            }

            __instance.TryRunNetworkTransaction(removeOne, result =>
            {
                if (result.Succeed)
                    new BeaconArm(__instance.Player, item, zone, position, rotation).Start(setupTime);
                callback?.Invoke(result);
            });

            return false;
        }

        private class BeaconArm(Player player, Item stackItem, string zone, Vector3 position, Quaternion rotation)
        {
            private Item _beacon;
            private IEnumerator _timer;

            public void Start(float setupTime)
            {
                if (!Singleton<IGameLevel>.Instantiated) return;
                _beacon = stackItem.CloneItem(player.InventoryController);
                _beacon.StackObjectsCount = 1;
                IGameLevel level = Singleton<IGameLevel>.Instance;
                level.SetupItem(_beacon, player, position, rotation);
                level.OnLootItemDestroyed += OnLootItemDestroyed;
                _timer = StaticManager.Instance.StartBehaviourTimer(setupTime, OnCompleted);
            }

            private void OnCompleted()
            {
                if (Singleton<IGameLevel>.Instantiated)
                    Singleton<IGameLevel>.Instance.DestroyLoot(_beacon.Id);
                if (player != null)
                {
                    player.PlantItemLocalOnly(_beacon, zone);
                    player.UpdateInteractionCast();
                }
            }

            private void OnLootItemDestroyed(IKillable killable)
            {
                if (killable is LootItem lootItem && _beacon.Id.Equals(lootItem.ItemId))
                {
                    StaticManager.Instance.StopBehaviourTimer(ref _timer);
                    if (Singleton<IGameLevel>.Instantiated)
                        Singleton<IGameLevel>.Instance.OnLootItemDestroyed -= OnLootItemDestroyed;
                    IPlayer lastOwner = lootItem.LastOwner;
                    if (lastOwner != null
                        && lastOwner.ProfileId == GamePlayerOwner.MyPlayer.ProfileId
                        && MonoBehaviourSingleton<GameUI>.Instantiated)
                        MonoBehaviourSingleton<GameUI>.Instance.BattleUiPanelExtraction.Close();
                }
            }
        }
    }
}

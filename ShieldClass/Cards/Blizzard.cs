using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnboundLib;
using UnboundLib.Cards;
using ShieldClassNamespace.Interfaces;
using CardChoiceSpawnUniqueCardPatch.CustomCategories;
using UnityEngine;
using ShieldClassNamespace.MonoBehaviours;

namespace ShieldClassNamespace.Cards
{
    class Blizzard : CustomCard
    {
        private static GameObject _blizzardSpawn = null;

        public static CardInfo card = null;
        public static GameObject blizzardSpawn
        {
            get
            {
                if (_blizzardSpawn)
                {
                    return _blizzardSpawn;
                }

                _blizzardSpawn = ShieldClass.instance.shieldHeroAssets.LoadAsset<GameObject>("E_Blizzard");

                _blizzardSpawn.GetOrAddComponent<BlizzardStorm_Mono>();

                return _blizzardSpawn;
            }
        }
        public override void SetupCard(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers, Block block)
        {
            cardInfo.categories = new CardCategory[] { ShieldHero.ShieldHeroClass };
            block.cdAdd = 0.25f;
            cardInfo.allowMultiple = false;
            ShieldClass.instance.DebugLog($"[{ShieldClass.ModInitials}][Card] {GetTitle()} Built");
        }
        public override void OnAddCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            var blizzardObject = Instantiate(ShieldClass.instance.shieldHeroAssets.LoadAsset<GameObject>("A_Blizzard"), player.transform);
            blizzardObject.transform.localPosition = Vector3.zero;

            var blockTrigger = blizzardObject.GetComponent<BlockTrigger>();

            ShieldClass.instance.ExecuteAfterFrames(5, () => 
            { 
                block.SuperFirstBlockAction = (Action<BlockTrigger.BlockTriggerType>)Delegate.Remove(block.SuperFirstBlockAction, new Action<BlockTrigger.BlockTriggerType>(blockTrigger.DoSuperFirstBlock));
                block.FirstBlockActionThatDelaysOthers = (Action<BlockTrigger.BlockTriggerType>)Delegate.Remove(block.FirstBlockActionThatDelaysOthers, new Action<BlockTrigger.BlockTriggerType>(blockTrigger.DoFirstBlockThatDelaysOthers));
                block.BlockActionEarly = (Action<BlockTrigger.BlockTriggerType>)Delegate.Remove(block.BlockActionEarly, new Action<BlockTrigger.BlockTriggerType>(blockTrigger.DoBlockEarly));
                block.BlockProjectileAction = (Action<GameObject, Vector3, Vector3>)Delegate.Remove(block.BlockProjectileAction, new Action<GameObject, Vector3, Vector3>(blockTrigger.DoBlockedProjectile));
                block.BlockRechargeAction = (Action)Delegate.Remove(block.BlockRechargeAction, new Action(blockTrigger.DoBlockRecharge));
            });

            characterStats.objectsAddedToPlayer.Add(blizzardObject);

            blizzardObject.GetComponent<SpawnObjects>().objectToSpawn[0] = blizzardSpawn;

            var upgrader = player.GetComponentInChildren<ShieldHeroUpgrader>();
            if (upgrader)
            {
                upgrader.upgradeAction += new Action<int>((level) =>
                {
                    var attackLevel = blizzardObject.GetComponent<AttackLevel>();
                    attackLevel.attackLevel = level;
                    attackLevel.LevelUp();
                });
            }

            var attackLevel = blizzardObject.GetComponent<AttackLevel>();
            attackLevel.attackLevel = upgrader.currentUpgradeLevel;
            attackLevel.LevelUp();

            ShieldClass.instance.DebugLog($"[{ShieldClass.ModInitials}][Card] {GetTitle()} Added to Player {player.playerID}");
        }

        public override void OnRemoveCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {

            ShieldClass.instance.DebugLog($"[{ShieldClass.ModInitials}][Card] {GetTitle()} removed from Player {player.playerID}");
        }

        protected override string GetTitle()
        {
            return "Blizzard";
        }
        protected override string GetDescription()
        {
            return "Unleash a powerful storm of cold when you block. Enemies who pass through are slowed and damaged by the cold, potentially freezing if they stay too long.";
        }
        protected override GameObject GetCardArt()
        {
            GameObject art;

            try
            {
                art = ShieldClass.instance.shieldHeroAssets.LoadAsset<GameObject>("C_Blizzard");
            }
            catch
            {
                art = null;
            }

            return art;
        }
        protected override CardInfo.Rarity GetRarity()
        {
            return CardInfo.Rarity.Common;
        }
        protected override CardInfoStat[] GetStats()
        {
            return new CardInfoStat[]
            {
                new CardInfoStat()
                {
                    positive = false,
                    stat = "Block CD",
                    amount = "+0.25s",
                    simepleAmount = CardInfoStat.SimpleAmount.notAssigned
                },
                new CardInfoStat()
                {
                    positive = false,
                    stat = "Ability CD",
                    amount = "0.25s",
                    simepleAmount = CardInfoStat.SimpleAmount.notAssigned
                }
            };
        }
        protected override CardThemeColor.CardThemeColorType GetTheme()
        {
            return CardThemeColor.CardThemeColorType.ColdBlue;
        }
        public override string GetModName()
        {
            return ShieldClass.ModInitials;
        }
        public override bool GetEnabled()
        {
            return true;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnboundLib;
using UnboundLib.Cards;
using ShieldClassNamespace.Interfaces;
using ShieldClassNamespace.MonoBehaviours;
using CardChoiceSpawnUniqueCardPatch.CustomCategories;
using UnityEngine;

namespace ShieldClassNamespace.Cards
{
    class ElectricFury : CustomCard
    {
        public static CardInfo card = null;
        public override void SetupCard(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers, Block block)
        {
            cardInfo.categories = new CardCategory[] { ShieldHero.ShieldHeroClass };
            //block.cdAdd = 0.25f;
            cardInfo.allowMultiple = false;
            ShieldClass.instance.DebugLog($"[{ShieldClass.ModInitials}][Card] {GetTitle()} Built");
        }
        public override void OnAddCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            GameObject electric = Instantiate(ShieldClass.instance.shieldHeroAssets.LoadAsset<GameObject>("E_ElectricFury"), player.transform);
            var electricMono = electric.AddComponent<ElectricField_Mono>();

            var upgrader = player.GetComponentInChildren<ShieldHeroUpgrader>();
            if (upgrader)
            {
                upgrader.upgradeAction += new Action<int>((level) =>
                {
                    electricMono.level = level;
                });
            }

            characterStats.objectsAddedToPlayer.Add(electric);

            ShieldClass.instance.DebugLog($"[{ShieldClass.ModInitials}][Card] {GetTitle()} Added to Player {player.playerID}");
        }
        public override void OnRemoveCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            ShieldClass.instance.DebugLog($"[{ShieldClass.ModInitials}][Card] {GetTitle()} removed from Player {player.playerID}");
        }

        protected override string GetTitle()
        {
            return "Electric Fury";
        }
        protected override string GetDescription()
        {
            return "Harness the power of lightning when you block, creating a field of thunder that damages and silences nearby foes while increasing the power of your bullets.";
        }
        protected override GameObject GetCardArt()
        {
            GameObject art;

            try
            {
                art = ShieldClass.instance.shieldHeroAssets.LoadAsset<GameObject>("C_ElectricFury");
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
                //new CardInfoStat()
                //{
                //    positive = false,
                //    stat = "Block CD",
                //    amount = "+0.25s",
                //    simepleAmount = CardInfoStat.SimpleAmount.notAssigned
                //}
            };
        }
        protected override CardThemeColor.CardThemeColorType GetTheme()
        {
            return CardThemeColor.CardThemeColorType.NatureBrown;
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

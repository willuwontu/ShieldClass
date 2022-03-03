using System;
using System.Collections.Generic;
using System.Linq;
using UnboundLib;
using UnboundLib.Cards;
using UnboundLib.Utils;
using ShieldClassNamespace.Interfaces;
using ShieldClassNamespace.MonoBehaviours;
using CardChoiceSpawnUniqueCardPatch.CustomCategories;
using UnityEngine;
using TMPro;

namespace ShieldClassNamespace.Cards
{
    class ShieldHero : CustomCard
    {
        public static CardInfo card = null;

        public const string ShieldHeroClassName = "Shield Hero";

        public static CardCategory ShieldHeroClass = CustomCardCategories.instance.CardCategory("ShieldHero");

        public static void ShieldHeroAddClassStuff(CharacterStatModifiers characterStats)
        {
            ModdingUtils.Extensions.CharacterStatModifiersExtension.GetAdditionalData(characterStats).blacklistedCategories.Add(CustomCardCategories.instance.CardCategory("Class"));
            ModdingUtils.Extensions.CharacterStatModifiersExtension.GetAdditionalData(characterStats).blacklistedCategories.RemoveAll((category) => category == ShieldHeroClass);
            ModdingUtils.Extensions.CharacterStatModifiersExtension.GetAdditionalData(characterStats).blacklistedCategories.RemoveAll((category) => category == CustomCardCategories.instance.CardCategory("Default"));
        }

        public static void ShieldHeroRemoveClassStuff(CharacterStatModifiers characterStats)
        {
            ModdingUtils.Extensions.CharacterStatModifiersExtension.GetAdditionalData(characterStats).blacklistedCategories.RemoveAll((category) => category == CustomCardCategories.instance.CardCategory("Class"));
            ModdingUtils.Extensions.CharacterStatModifiersExtension.GetAdditionalData(characterStats).blacklistedCategories.Add(ShieldHeroClass);
        }

        public override void SetupCard(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers, Block block)
        {
            //block.cdAdd = 0.25f;

            cardInfo.allowMultiple = false;
            cardInfo.categories = new CardCategory[] { CustomCardCategories.instance.CardCategory("Class") };
            ShieldClass.instance.DebugLog($"[{ShieldClass.ModInitials}][Card] {GetTitle()} Built");
        }
        public override void OnAddCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            ShieldHeroAddClassStuff(characterStats);

            var abyssalCard = CardManager.cards.Values.Select(card => card.cardInfo).First(c => c.name.Equals("AbyssalCountdown"));
            var statMods = abyssalCard.gameObject.GetComponentInChildren<CharacterStatModifiers>();
            var abyssalObj = statMods.AddObjectToPlayer;

            var obj2 = Instantiate(abyssalObj);

            var shieldHeroObj = Instantiate(ShieldClass.instance.shieldHeroAssets.LoadAsset<GameObject>("A_ShieldHero"), player.transform);
            shieldHeroObj.name = "A_ShieldHeroUpgrader";
            shieldHeroObj.transform.localPosition = Vector3.zero;

            var abyssal = obj2.GetComponent<AbyssalCountdown>();

            var upgrader = shieldHeroObj.AddComponent<ShieldHeroUpgrader>();
            upgrader.soundUpgradeChargeLoop = abyssal.soundAbyssalChargeLoop;

            UnityEngine.GameObject.Destroy(obj2);

            abyssal = shieldHeroObj.GetComponent<AbyssalCountdown>();
            abyssal.soundAbyssalChargeLoop = upgrader.soundUpgradeChargeLoop;

            upgrader.counter = 0;
            upgrader.upgradeTime = 9f;
            upgrader.timeToEmpty = 6f;
            upgrader.upgradeCooldown = 12f;
            upgrader.outerRing = abyssal.outerRing;
            upgrader.fill = abyssal.fill;
            upgrader.rotator = abyssal.rotator;
            upgrader.still = abyssal.still;
            upgrader.blockModifier.additionalBlocks_add = 1;


            ShieldClass.instance.ExecuteAfterFrames(5, () =>
            {
                UnityEngine.GameObject.Destroy(abyssal);
            });

            characterStats.objectsAddedToPlayer.Add(shieldHeroObj);

            ShieldClass.instance.DebugLog($"[{ShieldClass.ModInitials}][Card] {GetTitle()} Added to Player {player.playerID}");
        }
        public override void OnRemoveCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            ShieldHeroRemoveClassStuff(characterStats);
            ShieldClass.instance.DebugLog($"[{ShieldClass.ModInitials}][Card] {GetTitle()} removed from Player {player.playerID}");
        }

        protected override string GetTitle()
        {
            return "Shield Hero";
        }
        protected override string GetDescription()
        {
            return "Use the power of your shield to take down foes. Gain levels by standing still or blocking enemy bullets.";
        }
        protected override GameObject GetCardArt()
        {
            GameObject art;

            try
            {
                art = ShieldClass.instance.shieldHeroAssets.LoadAsset<GameObject>("C_ShieldHero");
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
                    positive = true,
                    stat = "Block per Upgrade",
                    amount = "+1",
                    simepleAmount = CardInfoStat.SimpleAmount.notAssigned
                }
                //,
                //new CardInfoStat()
                //{
                //    positive = false,
                //    stat = "Block Cooldown",
                //    amount = "+0.25s",
                //    simepleAmount = CardInfoStat.SimpleAmount.notAssigned
                //}
            };
        }
        protected override CardThemeColor.CardThemeColorType GetTheme()
        {
            return CardThemeColor.CardThemeColorType.DefensiveBlue;
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

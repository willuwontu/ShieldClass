﻿using System;
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
    class Fireball : CustomCard
    {
        public override void SetupCard(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers, Block block)
        {
            cardInfo.categories = new CardCategory[] { ShieldHero.ShieldHeroClass };
            block.cdAdd = 0.25f;

            ShieldClass.instance.DebugLog($"[{ShieldClass.ModInitials}][Card] {GetTitle()} Built");
        }
        public override void OnAddCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            var fireballSpawner = new GameObject("A_FireballSpawner");
            fireballSpawner.transform.SetParent(player.transform);
            fireballSpawner.transform.localScale = Vector3.one;
            fireballSpawner.transform.localPosition = Vector3.zero;

            var spawnMono = fireballSpawner.AddComponent<FireballSpawner_Mono>();

            characterStats.objectsAddedToPlayer.Add(fireballSpawner);

            var upgrader = player.GetComponentInChildren<ShieldHeroUpgrader>();
            if (upgrader)
            {
                upgrader.upgradeAction += spawnMono.OnUpgrade;
            }

            ShieldClass.instance.DebugLog($"[{ShieldClass.ModInitials}][Card] {GetTitle()} Added to Player {player.playerID}");
        }
        public override void OnRemoveCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            //Drives me crazy
            ShieldClass.instance.DebugLog($"[{ShieldClass.ModInitials}][Card] {GetTitle()} removed from Player {player.playerID}");
        }

        protected override string GetTitle()
        {
            return "Fireball";
        }
        protected override string GetDescription()
        {
            return "Launch a fireball towards foes when you block.";
        }
        protected override GameObject GetCardArt()
        {
            return null;
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
            return CardThemeColor.CardThemeColorType.DestructiveRed;
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

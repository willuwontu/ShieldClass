using BepInEx;
using CardChoiceSpawnUniqueCardPatch.CustomCategories;
using HarmonyLib;
using ShieldClassNamespace.Interfaces;
using UnboundLib;
using UnboundLib.Cards;
using Jotunn.Utils;
using UnityEngine;
using ShieldClassNamespace.Cards;
using UnboundLib.GameModes;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

namespace ShieldClassNamespace
{
    // These are the mods required for our mod to work
    [BepInDependency("com.willis.rounds.unbound", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("pykess.rounds.plugins.moddingutils", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("pykess.rounds.plugins.cardchoicespawnuniquecardpatch", BepInDependency.DependencyFlags.HardDependency)]
    // Declares our mod to Bepin
    [BepInPlugin(ModId, ModName, Version)]
    // The game our mod is associated with
    [BepInProcess("Rounds.exe")]
    public class ShieldClass : BaseUnityPlugin
    {
        private const string ModId = "com.SSundee.cards.ShieldHero";
        private const string ModName = "Shield Hero";
        public const string Version = "1.0.0"; // What version are we on (major.minor.patch)?

        internal const string ModInitials = "SH";

        public static ShieldClass instance { get; private set; }

        public AssetBundle shieldHeroAssets { get; private set; }

        private const bool debug = false;

        void Awake()
        {

        }
        void Start()
        {
            Unbound.RegisterCredits(ModName, new string[] { "SSundee", "Willuwontu" }, new string[] { "Youtube", "github", "Ko-Fi" }, new string[] { "https://www.youtube.com/channel/UCke6I9N4KfC968-yRcd5YRg", "https://github.com/willuwontu/wills-wacky-cards", "https://ko-fi.com/willuwontu" });

            instance = this;

            // Use this to call any harmony patch files your mod may have
            var harmony = new Harmony(ModId);
            harmony.PatchAll();

            gameObject.AddComponent<InterfaceGameModeHooksManager>();

            shieldHeroAssets = AssetUtils.LoadAssetBundleFromResources("shieldheroassets", typeof(ShieldClass).Assembly);

            CustomCard.BuildCard<ShieldHero>(card => { ShieldHero.card = card; });
            CustomCard.BuildCard<Fireball>(card => { Fireball.card = card; });
            CustomCard.BuildCard<Blizzard>(card => { Blizzard.card = card; });
            CustomCard.BuildCard<ElectricFury>(card => { ElectricFury.card = card; });

            GameModeManager.AddHook(GameModeHooks.HookGameStart, OnGameStart);
            GameModeManager.AddHook(GameModeHooks.HookGameStart, OnPlayerPickStart);

        }

        private IEnumerator OnGameStart(IGameModeHandler gm)
        {
            foreach (var player in PlayerManager.instance.players)
            {
                if (!ModdingUtils.Extensions.CharacterStatModifiersExtension.GetAdditionalData(player.data.stats).blacklistedCategories.Contains(ShieldHero.ShieldHeroClass))
                {
                    ModdingUtils.Extensions.CharacterStatModifiersExtension.GetAdditionalData(player.data.stats).blacklistedCategories.Add(ShieldHero.ShieldHeroClass);
                }
            }

            yield break;
        }

        private IEnumerator OnPlayerPickStart(IGameModeHandler gm)
        {
            try
            {
                foreach (var player in PlayerManager.instance.players)
                {
                    var cards = player.data.currentCards.ToArray();
                    if ((cards.Where(card => card.categories.Contains(ShieldHero.ShieldHeroClass)).ToArray().Length > 0) && (!cards.Select(card => card.cardName.ToLower()).ToArray().Contains("Shield Hero".ToLower())))
                    {
                        var heroIndeces = Enumerable.Range(0, player.data.currentCards.Count()).Where((index) => player.data.currentCards[index].categories.Contains(ShieldHero.ShieldHeroClass)).ToArray();
                        var earliest = heroIndeces.Min();

                        ShieldClass.instance.StartCoroutine(ReplaceCard(player, ShieldHero.card, earliest));
                    }
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
            }

            yield return null;
            yield break;
        }

        private IEnumerator ReplaceCard(Player player, CardInfo card, int index)
        {
            yield return ModdingUtils.Utils.Cards.instance.ReplaceCard(player, index, card, "", 2f, 2f, true);
            yield return null;
            yield break;
        }

        public void DebugLog(object message)
        {
            if (debug)
            {
                UnityEngine.Debug.Log(message);
            }
        }
    }
}

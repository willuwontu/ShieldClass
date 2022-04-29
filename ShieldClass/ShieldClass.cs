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
    [BepInDependency("root.classes.manager.reborn", BepInDependency.DependencyFlags.HardDependency)]
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

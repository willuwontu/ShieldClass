using HarmonyLib;
using UnityEngine;
using UnboundLib;
using UnboundLib.Utils;
using System.Collections.Generic;
using System.Linq;
using ShieldClassNamespace.Cards;

namespace ShieldClassNamespace.Patches
{
    [HarmonyPatch(typeof(CardChoice))]
    class CardChoice_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch("Spawn")]
        [HarmonyPriority(Priority.Last)]
        static void SpawnClassCardsPre(CardChoice __instance, ref GameObject objToSpawn, ref AdjustedCards __state, int ___pickrID, List<GameObject> ___spawnedCards, Transform[] ___children)
        {
            __state = new AdjustedCards();

            if (__instance.IsPicking)
            {
                //UnityEngine.Debug.Log($"Originally spawning {objToSpawn.GetComponent<CardInfo>().cardName}");

                var player = PlayerManager.instance.GetPlayerWithID(___pickrID);

                CardInfo[] spawnedCards = ___spawnedCards.Select(obj => obj.GetComponent<CardInfo>().sourceCard).ToArray();
                //UnityEngine.Debug.Log($"{spawnedCards.Length}/{___children.Length} cards spawned already.");

                //if (___spawnedCards.Count > 0)
                //{
                //    UnityEngine.Debug.Log($"Already spawned cards:");
                //    foreach (var spawnedCard in spawnedCards)
                //    {
                //        UnityEngine.Debug.Log($"{spawnedCard.cardName}");
                //    }
                //}

                // Force class cards as picks first round
                if (ShieldClass.picks <= 1)
                {
                    // Get all the class cards
                    CardInfo[] classCards = CardManager.cards.Values.Where(card =>
                        card.enabled == true &&
                        card.cardInfo.categories.Contains(CardChoiceSpawnUniqueCardPatch.CustomCategories.CustomCardCategories.instance.CardCategory("Class")) &&
                        !spawnedCards.Contains(card.cardInfo)
                    ).Select(card => card.cardInfo).ToArray();
                    classCards.Shuffle();

                    if (classCards.Length > 0)
                    {
                        objToSpawn = classCards[0].gameObject;

                        __state.adjusted = true;
                        __state.newCard = classCards[0];
                    }

                } // If we have the shield hero class, and don't have all the cards in the class
                else if (player.data.currentCards.Contains(ShieldHero.card) && (player.data.currentCards.Intersect(ShieldClass.heroCards).Count() < ShieldClass.heroCards.Count()))
                {
                    // See if we already have a shield hero card as an option
                    if (spawnedCards.Intersect(ShieldClass.heroCards).Count() < 1)
                    {
                        // If this is the last chance to spawn a card we replace, otherwise 20% chance to replace
                        if ((spawnedCards.Length == (___children.Length - 1)) || (UnityEngine.Random.Range(0f, 1f) < 0.2f))
                        {
                            List<CardInfo> possibleCards = ShieldClass.heroCards.Where(card => !(player.data.currentCards.Contains(card))).ToList();
                            possibleCards.Shuffle();

                            objToSpawn = possibleCards[0].gameObject;

                            __state.adjusted = true;
                            __state.newCard = possibleCards[0];
                        }
                    }
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("Spawn")]
        [HarmonyPriority(Priority.First)]
        static void SpawnClassCardsPost(CardChoice __instance, GameObject __result, AdjustedCards __state)
        {
            if (__state.adjusted)
            {
                ShieldClass.instance.ExecuteAfterFrames(1, () => { __result.GetComponent<CardInfo>().sourceCard = __state.newCard; });
            }
        }

        private class AdjustedCards
        {
            public bool adjusted = false;
            public CardInfo newCard;
        }

        //[HarmonyPrefix]
        //[HarmonyPatch("SomeMethod")]
        //static void MyMethodName()
        //{

        //}

        //[HarmonyPostfix]
        //[HarmonyPatch("SomeMethod")]
        //static void MyMethodName()
        //{

        //}
    }
}
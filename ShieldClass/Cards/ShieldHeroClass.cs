using ClassesManagerReborn;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace ShieldClassNamespace.Cards
{
    public class ShieldHeroClass : ClassHandler
    {
        public static string name = "Shield\nHero";
        public override IEnumerator Init()
        {
            while(!(ShieldHero.card&&Blizzard.card&&ElectricFury.card&&Fireball.card)) yield return null;
            ClassesRegistry.Register(ShieldHero.card, CardType.Entry);
            ClassesRegistry.Register(Blizzard.card, CardType.Card, ShieldHero.card);
            ClassesRegistry.Register(ElectricFury.card, CardType.Card, ShieldHero.card);
            ClassesRegistry.Register(Fireball.card, CardType.Card, ShieldHero.card);
            yield break;
        }
    }
}

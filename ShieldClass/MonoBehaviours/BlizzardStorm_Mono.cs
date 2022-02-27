using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnboundLib;
using ModdingUtils.MonoBehaviours;
using ShieldClassNamespace.Interfaces;
using Photon.Pun;

namespace ShieldClassNamespace.MonoBehaviours
{
    class BlizzardStorm_Mono : MonoBehaviour
    {
        public float damage = 0.05f / 10f;
        public float range = 3f;
        public float slow = 0.2f;

        private SpawnedAttack spawned;

        private void Start()
        {
            this.spawned = base.GetComponent<SpawnedAttack>();

            var delay = this.gameObject.AddComponent<DelayEvent>();
            delay.auto = true;
            delay.repeating = true;
            delay.usedTimeScale = true;
            delay.time = 0.05f;
            delay.delayedEvent = new UnityEngine.Events.UnityEvent();
            delay.delayedEvent.AddListener(() => { Chill(); });
        }

        private void Chill()
        {
            float radius = range * base.transform.localScale.x;

            Collider2D[] hits = Physics2D.OverlapCircleAll(base.transform.position, radius).Where(col => (col.GetComponent<DamagableEvent>() || col.GetComponent<Player>())).ToArray();

            foreach (Collider2D hit in hits)
            {
                if (hit.GetComponent<Player>())
                {
                    var player = hit.GetComponent<Player>();
                    if (this.spawned && player.teamID != this.spawned.spawner.teamID)
                    {
                        player.data.healthHandler.CallTakeDamage(Vector2.up * damage * (1f + (spawned.attackLevel - 1f) * 0.75f), base.transform.position, null, this.spawned.spawner, true);
                        var coldMono = player.gameObject.GetOrAddComponent<PlayerInBlizzard_Mono>();
                        coldMono.wasInStorm = new bool[] { true, true };
                        coldMono.coldPercent += 0.05f / 6f;

                        if (coldMono.coldPercent >= 1f)
                        {
                            player.data.stunHandler.AddStun(0.05f);
                        }

                        player.data.view.RPC("RPCA_AddSlow", RpcTarget.All, new object[] { this.slow, false });
                    }
                }
                else
                {
                    hit.GetComponent<DamagableEvent>().CallTakeDamage(Vector2.up * damage, base.transform.position);
                }
            }
        }
    }

    class PlayerInBlizzard_Mono : MonoBehaviour, IPointEndHookHandler
    {
        public float coldPercent = 0f;

        private ColorEffect colorEffect;
        private float coldLostOver = 5f;

        public bool[] wasInStorm = new bool[] { true, true };

        public void OnPointEnd()
        {
            UnityEngine.GameObject.Destroy(this);
        }

        private void Start()
        {
            InterfaceGameModeHooksManager.instance.RegisterHooks(this);

            colorEffect = this.gameObject.AddComponent<ColorEffect>();
        }

        private void Update()
        {
            if (wasInStorm[1])
            {
                wasInStorm[1] = false;
            }
            else if (wasInStorm[0])
            {
                wasInStorm[0] = false;
            }
            else
            {
                this.coldPercent -= TimeHandler.deltaTime / coldLostOver;
            }

            colorEffect.SetColor(new Color(
                colorEffect.GetOriginalColorMax().r * (1 - coldPercent),
                colorEffect.GetOriginalColorMax().g + (0.7931f - colorEffect.GetOriginalColorMax().g) * coldPercent,
                colorEffect.GetOriginalColorMax().b + (1f - colorEffect.GetOriginalColorMax().b) * coldPercent,
                colorEffect.GetOriginalColorMax().a + (1f - colorEffect.GetOriginalColorMax().a) * coldPercent
                ));
            colorEffect.ApplyColor();

            if (coldPercent <= 0f)
            {
                UnityEngine.GameObject.Destroy(this);
            }
        }

        private void OnDestroy()
        {
            InterfaceGameModeHooksManager.instance.RemoveHooks(this);
            UnityEngine.GameObject.Destroy(colorEffect);
        }
    }
}

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
        public float damage = 0.05f * 2f;
        public float range = 3f;
        public float slow = 0.3f;

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
                    if (this.spawned && player.teamID != this.spawned.spawner.teamID && !player.data.block.IsBlocking())
                    {
                        player.data.healthHandler.CallTakeDamage(Vector2.up * damage * (1f + (spawned.attackLevel - 1f) * 0.5f), base.transform.position, null, this.spawned.spawner, true);
                        var coldMono = player.gameObject.GetOrAddComponent<PlayerInBlizzard_Mono>();
                        coldMono.wasInStorm = new bool[] { true, true };
                        coldMono.coldPercent += 0.05f / 4f;

                        if (coldMono.coldPercent >= 1f)
                        {
                            player.data.stunHandler.AddStun(0.05f);
                        }
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
        private float lastCheck = 0f;
        private Player player;

        public bool[] wasInStorm = new bool[] { true, true };

        public void OnPointEnd()
        {
            UnityEngine.GameObject.Destroy(this);
        }

        private void Start()
        {
            player = this.GetComponent<Player>();
            if (!player)
            {
                UnityEngine.GameObject.Destroy(this);
            }
            InterfaceGameModeHooksManager.instance.RegisterHooks(this);

            colorEffect = this.gameObject.AddComponent<ColorEffect>();
        }

        private void Update()
        {
            if (wasInStorm[1])
            {
                wasInStorm[1] = false;
                lastCheck = Time.time;
            }
            else if (wasInStorm[0] && ((lastCheck + 0.05f) < Time.time))
            {
                wasInStorm[0] = false;
            }
            else if (!wasInStorm[0] && !wasInStorm[1])
            {
                this.coldPercent -= TimeHandler.deltaTime / coldLostOver;
            }

            if (wasInStorm[0] || wasInStorm[1])
            {
                player.data.view.RPC("RPCA_AddSlow", RpcTarget.All, new object[] { Mathf.Max(0.4f, coldPercent), false });
            }

            colorEffect.SetColor(new Color(
                colorEffect.GetOriginalColorMax().r * (1 - coldPercent),
                colorEffect.GetOriginalColorMax().g + (0.7931f - colorEffect.GetOriginalColorMax().g) * coldPercent,
                colorEffect.GetOriginalColorMax().b + (0.8f - colorEffect.GetOriginalColorMax().b) * coldPercent,
                colorEffect.GetOriginalColorMax().a + (0.9f - colorEffect.GetOriginalColorMax().a) * coldPercent
                )); ;
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

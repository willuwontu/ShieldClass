using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnboundLib;
using UnboundLib.Networking;
using ShieldClassNamespace.Interfaces;
using Photon.Pun;

namespace ShieldClassNamespace.MonoBehaviours
{
    class ElectricField_Mono : MonoBehaviour, IPointEndHookHandler
    {
        public int level = 0;
        private float levelScaling = 0.5f;

        private float lastTriggerTime = 0f;
        private float cooldown = 0.25f;

        private float damage = 20f;
        private float stunChance = 0.0125f;
        private float silenceChance = 0.025f;
        private float damageBonus = 0.3f;
        private float duration = 2f;
        private float range = 4.1f;

        private float lastStunSilenceCheck = 0f;
        private float stunSilenceCD = 0.05f;

        private bool active;
        private bool shrinking;
        private ModdingUtils.Extensions.GunStatModifier gunStatModifier = new ModdingUtils.Extensions.GunStatModifier();
        private Player player;
        private Block block;
        private Gun gun;

        private GameObject particles;
        private Coroutine sizeCoroutine;
        private LineEffect lineEffect;

        private void Start()
        {
            player = GetComponentInParent<Player>();
            block = player.data.block;
            gun = player.data.weaponHandler.gun;
            lineEffect = GetComponentInChildren<LineEffect>();
            particles = lineEffect.transform.parent.gameObject;

            block.BlockAction += OnBlock;
            player.data.healthHandler.reviveAction += OnPointEnd;

            base.transform.localScale = Vector3.zero;
            lineEffect.radius = 0f;
            particles.SetActive(false);
        }

        private void Update()
        {
            if (!active)
            {
                return;
            }

            float duration = this.duration + this.duration * levelScaling * level;

            if ((this.lastTriggerTime + duration) < Time.time)
            {
                OnPointEnd();
                return;
            }

            float radius = range * base.transform.root.localScale.x * base.transform.localScale.x;
            float damage = this.damage + this.damage * levelScaling * level;
            float stunChance = this.stunChance + this.stunChance * levelScaling * level;
            float silenceChance = this.silenceChance + this.silenceChance * levelScaling * level;

            Player[] enemiesInRange = PlayerManager.instance.players.Where(p => (p.teamID != player.teamID) && !p.data.dead && (Vector3.Distance(p.transform.position, base.transform.position) < radius)).ToArray();

            foreach (Player person in enemiesInRange)
            {
                if (PlayerManager.instance.CanSeePlayer(base.transform.position, person).canSee)
                {
                    Vector3 dir = (person.transform.position - base.transform.position).normalized;

                    person.data.healthHandler.TakeDamage(damage * TimeHandler.deltaTime * dir, base.transform.position, null, player);

                    if (player.data.view.IsMine)
                    {
                        if ((this.lastStunSilenceCheck + stunSilenceCD < Time.time))
                        {
                            this.lastStunSilenceCheck = Time.time;

                            float roll = UnityEngine.Random.Range(0f, 1f);

                            if (roll < silenceChance)
                            {
                                person.data.view.RPC("RPCA_AddSilence", RpcTarget.All, new object[]
                                {
                                0.1f
                                });
                            }

                            roll = UnityEngine.Random.Range(0f, 1f);

                            if (roll < stunChance)
                            {
                                NetworkingManager.RPC(typeof(ElectricField_Mono), nameof(RPCA_StunPlayer), new object[] { 0.1f, person.playerID });
                            }
                        }
                    }
                }
            }
        }

        [UnboundRPC]
        private static void RPCA_StunPlayer(float duration, int playerID)
        {
            var person = PlayerManager.instance.GetPlayerWithID(playerID);

            person.data.stunHandler.AddStun(duration);
        }

        private void OnBlock(BlockTrigger.BlockTriggerType blockTrigger)
        {
            if (this.lastTriggerTime + this.cooldown > Time.time)
            {
                return;
            }
            this.lastTriggerTime = Time.time;

            if (active)
            {
                gunStatModifier.RemoveGunStatModifier(gun);
                gunStatModifier.damage_mult = 1 + damageBonus + damageBonus * level * levelScaling;
                gunStatModifier.ApplyGunStatModifier(gun);
            }
            else
            {
                active = true;
                gunStatModifier.damage_mult = 1 + damageBonus + damageBonus * level * levelScaling;
                gunStatModifier.ApplyGunStatModifier(gun);
            }

            if (sizeCoroutine != null)
            {
                Unbound.Instance.StopCoroutine(sizeCoroutine);
            }
            sizeCoroutine = Unbound.Instance.StartCoroutine(IChangeSize(false));
        }

        private IEnumerator IChangeSize(bool shrink)
        {
            var startScale = base.transform.localScale;
            var endScale = shrink ? new Vector3(0.05f, 0.05f, 0.05f) : Vector3.one;
            var distance = endScale - startScale;

            shrinking = shrink;

            if (!shrink)
            {
                particles.SetActive(true);
            }

            float t = PlayerManager.instance.playerMoveCurve.keys[PlayerManager.instance.playerMoveCurve.keys.Length - 1].time;
            float c = 0f;
            while (c < t)
            {
                c += Mathf.Clamp(Time.unscaledDeltaTime, 0f, 0.02f);
                base.transform.localScale = startScale + distance * PlayerManager.instance.playerMoveCurve.Evaluate(c);
                lineEffect.radius = range * 0.9f * base.transform.localScale.x;
                yield return null;
            }

            if (shrink)
            {
                particles.SetActive(false);
            }

            yield break;
        }

        public void OnPointEnd()
        {
            if (active)
            {
                active = false;
                gunStatModifier.RemoveGunStatModifier(gun);
            }
            if (sizeCoroutine != null && !shrinking)
            {
                Unbound.Instance.StopCoroutine(sizeCoroutine);
                sizeCoroutine = Unbound.Instance.StartCoroutine(IChangeSize(true));
            }
            if (sizeCoroutine == null)
            {
                sizeCoroutine = Unbound.Instance.StartCoroutine(IChangeSize(true));
            }
        }

        private void OnDestroy()
        {
            block.BlockAction -= OnBlock;
            player.data.healthHandler.reviveAction -= OnPointEnd;
        }
    }
}

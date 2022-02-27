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
    class FireballExplosion_Mono : MonoBehaviour
    {
        public float damage = 55f;
        public float range = 3.5f;

        private SpawnedAttack spawned;
        private void Start()
        {
            this.spawned = base.GetComponent<SpawnedAttack>();
            var delay = this.gameObject.AddComponent<DelayEvent>();
            delay.auto = true;
            delay.time = 0f;
            delay.delayedEvent = new UnityEngine.Events.UnityEvent();
            delay.delayedEvent.AddListener(() => { Explode(); });
        }

        private void Explode()
        {
            Gun gun = spawned.spawner.data.weaponHandler.gun;
            float damageMult = gun.damage * gun.bulletDamageMultiplier;
            damage = damageMult * 55f;
            float radius = range * base.transform.localScale.x;

            RaycastHit2D[] hits = Physics2D.CircleCastAll(base.transform.position, radius, Vector2.up, 0.0f).Where(col => (col.collider.GetComponent<Rigidbody2D>() || col.collider.GetComponent<Player>())).ToArray();

            foreach (RaycastHit2D hit in hits)
            {
                try
                {
                    Collider2D[] initialContactPoints = Physics2D.OverlapPointAll(hit.point);
                    RaycastHit2D[] lines = Physics2D.LinecastAll(base.transform.position, hit.point, ~LayerMask.GetMask("PlayerObjectCollider", "Projectile"));
                    RaycastHit2D[] reverseLines = Physics2D.LinecastAll(hit.point, base.transform.position, ~LayerMask.GetMask("PlayerObjectCollider", "Projectile"));

                    lines = lines.OrderBy(ray => ray.distance).ToArray();
                    reverseLines = reverseLines.OrderBy(ray => ray.distance).ToArray();

                    bool cont = false;

                    for (int i = 0; i < lines.Length; i++)
                    {
                        if (lines[i].collider == hit.collider)
                        {
                            break;
                        }
                        // If it's not connected to a player or rigidbody, then it's ground
                        // We need to check to see if the ground is where the effect is being spawned from or if it is blocking the explosion
                        else if (
                                (!lines[i].collider.GetComponent<Rigidbody2D>() && !lines[i].collider.GetComponent<Player>()) && // Ground amd
                                (
                                lines[i].distance > 0.1f || // we're outside of 0.1 units of the spawn location or
                                (Vector2.Distance(lines[i].point, reverseLines.Where(col => col.collider == lines[i].collider).First().point) > 0.25f) // we're more than 0.25 units thick in the direction of the contact point
                                )
                            )
                        {
                            cont = true;
                            break;
                        }
                    }

                    if (cont)
                    {
                        continue;
                    }

                    RaycastHit2D line = lines.Where(lin => lin.collider == hit.collider).First();

                    Vector2 direction = (hit.point - (Vector2)base.transform.position).normalized;

                    float distance = line ? line.distance : Vector2.Distance(base.transform.position, hit.point);
                    float distanceMult = (1f - distance / radius);

                    Damagable damagable = hit.collider.GetComponent<Damagable>();

                    if (damagable)
                    {
                        damagable.CallTakeDamage(Vector2.ClampMagnitude(direction * damage, damage * distanceMult), hit.point, null, spawned.spawner, true);
                    }

                    Player person = hit.collider.GetComponent<Player>();
                    Rigidbody2D rig = hit.collider.GetComponent<Rigidbody2D>();
                    if (person)
                    {
                        person.data.healthHandler.CallTakeForce(direction * person.data.playerVel.GetMass() * 100f * damageMult * distanceMult, ForceMode2D.Impulse, false, false, 0f);
                    }

                    if (rig)
                    {
                        rig.AddForceAtPosition(direction * 2250f * damageMult * distanceMult * rig.mass, hit.point);

                        // Boxes take double damage, because I want them to.
                        if (damagable)
                        {
                            damagable.CallTakeDamage(Vector2.ClampMagnitude(direction * damage, damage * distanceMult), hit.point, null, spawned.spawner, true);
                        }
                    }
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogException(e);
                }

            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using System.Linq;
using UnboundLib;
using UnityEngine;

namespace ShieldClassNamespace.MonoBehaviours
{
    class FireballSpawner_Mono : MonoBehaviour
    {
        int spawnCount = 1;

        float lastTriggerTime = 0f;
        float cooldown = 0.25f;
        private Player player;

        private void Start()
        {
            player = this.GetComponentInParent<Player>();

            if (!player)
            {
                UnityEngine.GameObject.DestroyImmediate(this);
                return;
            }

            player.data.block.BlockAction += OnBlock;
        }

        public static GameObject fireballEffect
        {
            get
            {
                var fireballSpawner = ShieldClass.instance.shieldHeroAssets.LoadAsset<GameObject>("E_Fireball");

                fireballSpawner.GetOrAddComponent<FireballExplosion_Mono>();

                return fireballSpawner;
            }
        }

        private void OnBlock(BlockTrigger.BlockTriggerType blockTrigger)
        {
            if (this.lastTriggerTime + this.cooldown > Time.time)
            {
                return;
            }
            this.lastTriggerTime = Time.time;

            var fireballLauncher = player.gameObject.GetOrAddComponent<FireballGun>();
            fireballLauncher.CopyGunStats(player.data.weaponHandler.gun);
            fireballLauncher.bursts = spawnCount;
            fireballLauncher.timeBetweenBullets = 0.025f;
            fireballLauncher.destroyBulletAfter = 300f;
            fireballLauncher.gravity = 0f;
            fireballLauncher.projectielSimulatonSpeed *= 0.8f;
            fireballLauncher.bulletDamageMultiplier = 0f;
            fireballLauncher.reflects = 0;
            fireballLauncher.spread = Mathf.Max(0.025f * spawnCount, fireballLauncher.spread);

            GameObject effect = ShieldClass.instance.shieldHeroAssets.LoadAsset<GameObject>("A_Fireball");
            effect.GetComponent<SpawnObjects>().objectToSpawn[0] = fireballEffect;

            fireballLauncher.objectsToSpawn = new ObjectsToSpawn[] { new ObjectsToSpawn { effect = effect, AddToProjectile = ShieldClass.instance.shieldHeroAssets.LoadAsset<GameObject>("V_Fireball") } };

            Vector3 shootDir = MainCam.instance.cam.ScreenToWorldPoint(Input.mousePosition) - base.transform.position;
            shootDir.z = 0f;
            shootDir = shootDir.normalized;

            fireballLauncher.SetFieldValue("forceShootDir", (Vector3)shootDir);
            fireballLauncher.Attack(0f, true);
        }

        public void OnUpgrade(int level)
        {
            spawnCount = Mathf.CeilToInt((level + 1f)/2f);
        }

        private void OnDestroy()
        {
            player.data.block.BlockAction -= OnBlock;
        }
    }

    class FireballGun : Gun
    {

    }
}

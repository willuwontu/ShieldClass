using System;
using Sonigon;
using Sonigon.Internal;
using UnityEngine;
using TMPro;
using UnityEngine.UI.ProceduralImage;
using UnboundLib;
using ShieldClassNamespace.Interfaces;
using ShieldClassNamespace.Extensions;
using ModdingUtils.Extensions;
using System.Linq;

namespace ShieldClassNamespace.MonoBehaviours
{
	public class ShieldHeroUpgrader : MonoBehaviour, IRoundEndHookHandler, IPointStartHookHandler
	{
		public void Start()
		{
			this.soundCounterLast = this.counter;
			this.data = base.GetComponentInParent<CharacterData>();
			this.data.block.BlockProjectileAction += OnBlockProjectile;
			this.data.block.BlockAction += ResetCounterOnEcho;
			HealthHandler healthHandler = this.data.healthHandler;
			healthHandler.reviveAction += OnRevive;
			base.GetComponentInParent<ChildRPC>().childRPCs.Add("ShieldHeroUpgrade", new Action(this.RPCA_Upgrade));
			InterfaceGameModeHooksManager.instance.RegisterHooks(this);

            { // Level Frame
				var wobble = player.transform.Find("WobbleObjects");

				var upgradeFrame = Instantiate(player.transform.Find("WobbleObjects/Healthbar"), player.transform.Find("WobbleObjects")).gameObject;
				upgradeFrame.name = "Shield Hero Level";
				upgradeFrame.transform.localScale = Vector3.one;
				upgradeFrame.transform.localPosition = new Vector3(0, 0.851f, 0);

				UnityEngine.GameObject.Destroy(upgradeFrame.GetComponent<HealthBar>());

				var upgradeCanvas = upgradeFrame.transform.Find("Canvas").gameObject;

				var COs = upgradeCanvas.GetComponentsInChildren<Transform>().Where(child => child.parent == upgradeCanvas.transform).Select(child => child.gameObject).ToArray();
				foreach (var CO in COs)
				{
					UnityEngine.GameObject.Destroy(CO);
				}

				RectTransform rect = null;

				var levelFrame = Instantiate<GameObject>(ShieldClass.instance.shieldHeroAssets.LoadAsset<GameObject>("LevelFrame"), upgradeCanvas.transform);
				rect = levelFrame.GetComponent<RectTransform>();
				rect.localScale = Vector3.one;
				rect.localPosition = new Vector3(-375, 0, 0);

				this.levelFrame = upgradeFrame;
				this.levelText = levelFrame.transform.Find("Ring/Level").gameObject.GetComponent<TextMeshProUGUI>();
				this.levelText.text = $"{this.upgradeLevel}";
			}

			rotateImage = rotator.gameObject.GetComponentInChildren<ProceduralImage>();
			topImage = still.gameObject.GetComponentInChildren<ProceduralImage>();
			backRing = this.gameObject.transform.Find("Canvas/Size/BackRing").GetComponent<ProceduralImage>();
		}

		public void OnDestroy()
		{
			this.data.block.BlockProjectileAction -= OnBlockProjectile;
			this.data.block.BlockAction -= ResetCounterOnEcho;
			HealthHandler healthHandler = this.data.healthHandler;
			healthHandler.reviveAction -= OnRevive;
			base.GetComponentInParent<ChildRPC>().childRPCs.Remove("ShieldHeroUpgrade");
			this.SoundStop();
			InterfaceGameModeHooksManager.instance.RemoveHooks(this);

			if (levelFrame)
            {
				UnityEngine.GameObject.Destroy(levelFrame);
            }
		}

		private void OnRevive()
        {
			this.remainingDuration = upgradeCooldown;
			this.isUpgrading = true;
		}

		public void OnDisable()
		{
			this.SoundStop();
		}

		private void SoundPlay()
		{
			if (!this.soundChargeIsPlaying)
			{
				this.soundChargeIsPlaying = true;
				SoundManager.Instance.Play(this.soundUpgradeChargeLoop, base.transform, new SoundParameterBase[]
				{
				this.soundParameterIntensity
				});
			}
		}

		private void SoundStop()
		{
			if (this.soundChargeIsPlaying)
			{
				this.soundChargeIsPlaying = false;
				SoundManager.Instance.Stop(this.soundUpgradeChargeLoop, base.transform, true);
			}
		}

		public void OnRoundEnd()
        {
			//this.ResetStuff();
        }

		private void ResetStuff()
		{
			this.SoundStop();
			this.remainingDuration = 0f;
			this.counter = 0f;
			this.upgradeLevel = 0;
			this.currentUpgradeLevel = 0;
			this.levelText.text = $"{upgradeLevel}";
			if (this.isUpgrading)
			{
                for (int i = 0; i < this.upgradeObjects.Length; i++)
                {
                    this.upgradeObjects[i].gameObject.SetActive(false);
                }
                this.rotator.gameObject.SetActive(false);
                this.still.gameObject.SetActive(false);
				this.isUpgrading = false;
			}
			this.SoundStop();

			if (upgradeAction != null)
			{
				upgradeAction(upgradeLevel);
			}
		}

		private void RPCA_Upgrade()
		{
			var upgrade = this.data.player.gameObject.AddComponent<ShieldHeroUpgrade>();
			this.remainingDuration = this.upgradeCooldown;
			this.upgradeLevel++;
			this.levelText.text = $"{upgradeLevel}";

			if (upgradeAction != null)
            {
				upgradeAction(upgradeLevel);
            }
		}

		public void OnPointStart()
        {
			this.remainingDuration = 0;
			this.isUpgrading = true;
			this.counter = 0f;
		}

		private void Update()
		{
			AdjustColors(isUpgrading);

			if (this.soundCounterLast < this.counter)
			{
				this.SoundPlay();
			}
			else
			{
				this.SoundStop();
			}
			this.soundCounterLast = this.counter;
			this.soundParameterIntensity.intensity = this.counter;
			this.outerRing.fillAmount = this.counter;
			this.fill.fillAmount = this.counter;
			this.rotator.transform.localEulerAngles = new Vector3(0f, 0f, -Mathf.Lerp(0f, 360f, this.counter));
			if (!((bool)this.data.playerVel.GetFieldValue("simulated")))
			{
				this.startCounter = 1f;
				return;
			}
			this.startCounter -= TimeHandler.deltaTime;
			if (this.startCounter > 0f)
			{
				return;
			}
			if (this.remainingDuration > 0f)
			{
				if (!this.isUpgrading)
				{
					currentUpgradeLevel = upgradeLevel;
					this.isUpgrading = true;
					//for (int i = 0; i < this.abyssalObjects.Length; i++)
					//{
					//	this.abyssalObjects[i].gameObject.SetActive(true);
					//}
					//this.data.maxHealth *= this.hpMultiplier;
					//this.data.health *= this.hpMultiplier;
					//this.data.stats.ConfigureMassAndSize();
					//this.isAbyssalForm = true;
				}
				this.remainingDuration -= TimeHandler.deltaTime;
				this.counter = this.remainingDuration / this.upgradeCooldown;
				return;
			}
			if (this.isUpgrading)
			{
				this.isUpgrading = false;
				//for (int j = 0; j < this.abyssalObjects.Length; j++)
				//{
				//	this.abyssalObjects[j].gameObject.SetActive(false);
				//}
				//this.data.maxHealth /= this.hpMultiplier;
				//this.data.health /= this.hpMultiplier;
				//this.data.stats.ConfigureMassAndSize();
				//this.isAbyssalForm = false;
			}
            try
            {
                if (this.data.input.direction == Vector3.zero || this.data.input.direction == Vector3.down)
                {
                    this.counter += TimeHandler.deltaTime / this.upgradeTime;
                }
                else
                {
                    this.counter -= TimeHandler.deltaTime / this.timeToEmpty;
                }
            }
			catch (Exception e)
			{
				UnityEngine.Debug.Log("First Catch");
				UnityEngine.Debug.LogException(e);
			}
            try
            {
                this.counter = Mathf.Clamp(this.counter, -0.1f / this.upgradeTime, 1f);
                if (this.counter >= 1f && this.data.view.IsMine)
                {
                    this.remainingDuration = this.upgradeCooldown;
                    base.GetComponentInParent<ChildRPC>().CallFunction("ShieldHeroUpgrade");
                }
            }
			catch (Exception e)
			{
				UnityEngine.Debug.Log("Second Catch");
				UnityEngine.Debug.LogException(e);
			}
			try
            {
                if (this.counter <= 0f)
                {
                    this.rotator.gameObject.SetActive(false);
                    this.still.gameObject.SetActive(false);
                    return;
                }
                this.rotator.gameObject.SetActive(true);
                this.still.gameObject.SetActive(true);
            }
			catch (Exception e)
			{
				UnityEngine.Debug.Log("Last Catch");
				UnityEngine.Debug.LogException(e);
			}
		}

		private void ResetCounterOnEcho(BlockTrigger.BlockTriggerType blockTrigger)
		{
			if (blockTrigger == BlockTrigger.BlockTriggerType.Echo)
            {
				this.data.block.counter = 0f;
            }
        }

		private void OnBlockProjectile(GameObject projectile, Vector3 forward, Vector3 hitpos)
        {
			base.GetComponentInParent<ChildRPC>().CallFunction("ShieldHeroUpgrade");
		}

		private void AdjustColors(bool onCooldown)
        {
			int index = onCooldown ? 1 : 0;

			backRing.color = backRingColors[index];
			rotateImage.color = ringColors[index];
			topImage.color = ringColors[index];
			fill.color = backgroundColors[index];
			outerRing.color = ringColors[index];
		}

		private Color[] backRingColors = new Color[] 
		{
			new Color32(52, 122, 161, 29),
			new Color32(161, 56, 52, 29)
		};

		private Color[] ringColors = new Color[] 
		{
			new Color32(0, 191, 255, 255),
			new Color32(255, 69, 0, 255)
		};

		private Color[] backgroundColors = new Color[] 
		{
			new Color32(0, 84, 255, 5),
			new Color32(255, 41, 0, 5)
		};

		private ProceduralImage backRing;

		private ProceduralImage rotateImage;

		private ProceduralImage topImage;

		public SoundEvent soundUpgradeChargeLoop;

		private bool soundChargeIsPlaying;

		private float soundCounterLast;

		private SoundParameterIntensity soundParameterIntensity = new SoundParameterIntensity(0f, UpdateMode.Continuous);

		[Range(0f, 1f)]
		public float counter;

		public float upgradeTime = 5f;

		public float timeToEmpty = 1f;

		public float upgradeCooldown = 1;

		public float hpMultiplier = 2f;

		public int upgradeLevel = 0;

		public int currentUpgradeLevel = 0;

		public ProceduralImage outerRing;

		public ProceduralImage fill;

		public Transform rotator;

		public Transform still;

		private CharacterData data;

		public Player player
        {
			get
            {
				return data.player;
            }
        }
        public GameObject[] upgradeObjects = new GameObject[] { };

		private float remainingDuration;

		private bool isUpgrading;

		private float startCounter;

		public GameObject levelFrame = null;

		public TextMeshProUGUI levelText = null;

		public float extraBlockTime = 0f;
		public GunStatModifier gunStatModifier = new GunStatModifier();
		public GunAmmoStatModifier gunAmmoStatModifier = new GunAmmoStatModifier();
		public CharacterDataModifier characterDataModifier = new CharacterDataModifier();
		public CharacterStatModifiersModifier characterStatModifiersModifier = new CharacterStatModifiersModifier();
		public GravityModifier gravityModifier = new GravityModifier();
		public BlockModifier blockModifier = new BlockModifier();

		public Action<int> upgradeAction;
	}

	public class ShieldHeroUpgrade : ReversibleEffect, IRoundEndHookHandler
    {
		private float extraBlockTime = 0f;

        public override void OnStart()
        {
			InterfaceGameModeHooksManager.instance.RegisterHooks(this);
			applyImmediately = true;
			this.SetLivesToEffect(int.MaxValue);

			var upgrader = player.GetComponentInChildren<ShieldHeroUpgrader>();
			this.gunStatModifier = upgrader.gunStatModifier.Copy();
			this.characterDataModifier = upgrader.characterDataModifier.Copy();
			this.blockModifier = upgrader.blockModifier.Copy();
			this.characterStatModifiersModifier = upgrader.characterStatModifiersModifier.Copy();
			this.gunAmmoStatModifier = upgrader.gunAmmoStatModifier.Copy();
			this.gravityModifier = upgrader.gravityModifier.Copy();
			extraBlockTime = upgrader.extraBlockTime;
			ShieldClassNamespace.Extensions.CharacterStatModifiersExtension.GetAdditionalData(stats).extraBlockTime += extraBlockTime;
			block.UpdateParticleDuration();
		}

		public void OnRoundEnd()
        {
			//UnityEngine.GameObject.Destroy(this);
        }

        public override void OnOnDestroy()
        {
			InterfaceGameModeHooksManager.instance.RemoveHooks(this);
			ShieldClassNamespace.Extensions.CharacterStatModifiersExtension.GetAdditionalData(stats).extraBlockTime -= extraBlockTime;
		}
    }
}

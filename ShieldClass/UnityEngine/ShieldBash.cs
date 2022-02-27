using System;
using System.Collections;
using System.Collections.Generic;
using Sonigon;
using UnityEngine;
using UnboundLib;

public class ShieldCharge : MonoBehaviour
{
	private void Start()
	{
		this.level = base.GetComponent<AttackLevel>();
		this.data = base.GetComponentInParent<CharacterData>();
		PlayerCollision component = this.data.GetComponent<PlayerCollision>();
		component.collideWithPlayerAction = (Action<Vector2, Vector2, Player>)Delegate.Combine(component.collideWithPlayerAction, new Action<Vector2, Vector2, Player>(this.Collide));
		base.GetComponentInParent<ChildRPC>().childRPCsVector2Vector2Int.Add("ShieldBashCollide", new Action<Vector2, Vector2, int>(this.RPCA_Collide));
		Block componentInParent = base.GetComponentInParent<Block>();
		componentInParent.SuperFirstBlockAction = (Action<BlockTrigger.BlockTriggerType>)Delegate.Combine(componentInParent.SuperFirstBlockAction, new Action<BlockTrigger.BlockTriggerType>(this.DoBlock));
	}

	private void OnDestroy()
	{
		PlayerCollision component = this.data.GetComponent<PlayerCollision>();
		component.collideWithPlayerAction = (Action<Vector2, Vector2, Player>)Delegate.Remove(component.collideWithPlayerAction, new Action<Vector2, Vector2, Player>(this.Collide));
		base.GetComponentInParent<ChildRPC>().childRPCsVector2Vector2Int.Remove("ShieldBashCollide");
		Block componentInParent = base.GetComponentInParent<Block>();
		componentInParent.SuperFirstBlockAction = (Action<BlockTrigger.BlockTriggerType>)Delegate.Remove(componentInParent.SuperFirstBlockAction, new Action<BlockTrigger.BlockTriggerType>(this.DoBlock));
	}

	private void Update()
	{
		this.blockTime -= TimeHandler.deltaTime;
	}

	public void DoBlock(BlockTrigger.BlockTriggerType trigger)
	{
		if (trigger != BlockTrigger.BlockTriggerType.ShieldCharge)
		{
			this.Charge(trigger);
		}
	}

	public void Charge(BlockTrigger.BlockTriggerType trigger)
	{
		base.StartCoroutine(this.DoCharge(trigger));
	}

	private IEnumerator DoCharge(BlockTrigger.BlockTriggerType trigger)
	{
		SoundManager.Instance.Play(this.soundShieldCharge, base.transform);
		this.cancelForce = false;
		this.hitDatas.Clear();
		if (trigger == BlockTrigger.BlockTriggerType.Empower)
		{
			Vector3 currentPos = base.transform.position;
			yield return new WaitForSeconds(0f);
			this.dir = (currentPos - base.transform.position).normalized;
			currentPos = default(Vector3);
		}
		else
		{
			this.dir = this.data.aimDirection;
		}
		float usedTime = this.time + (float)this.level.LevelsUp() * this.timePerLevel;
		this.blockTime = usedTime;
		float num = this.time * 0.1f + (float)this.level.LevelsUp() * this.time * 0.15f;
		for (int i = 0; i < this.level.LevelsUp(); i++)
		{
			float num2 = this.time / (float)this.level.attackLevel;
			float num3 = this.time;
			num += num2;
			base.StartCoroutine(this.DelayBlock(num));
		}
		float c = 0f;
		while (c < 1f)
		{
			c += Time.fixedDeltaTime / usedTime;
			if (!this.cancelForce)
			{
				this.data.healthHandler.TakeForce(this.dir * this.forceCurve.Evaluate(c) * (this.force + (float)this.level.LevelsUp() * this.forcePerLevel), ForceMode2D.Force, true, true, 0f);
				this.data.healthHandler.TakeForce(-this.data.playerVel.GetVelocity() * this.drag * Time.fixedDeltaTime, ForceMode2D.Force, true, true, 0f);
			}
			this.data.sinceGrounded = 0f;
			yield return new WaitForFixedUpdate();
		}
		yield break;
	}

	private IEnumerator DelayBlock(float delay)
	{
		yield return new WaitForSeconds(delay);
		this.data.block.RPCA_DoBlock(false, true, BlockTrigger.BlockTriggerType.ShieldCharge, default(Vector3), false);
		yield break;
	}

	public void RPCA_Collide(Vector2 pos, Vector2 colDir, int playerID)
	{
		CharacterData componentInParent = PlayerManager.instance.GetPlayerWithID(playerID).gameObject.GetComponentInParent<CharacterData>();
		if (componentInParent)
		{
			this.cancelForce = true;
			this.hitPart.transform.rotation = Quaternion.LookRotation(this.dir);
			this.hitPart.Play();
			componentInParent.healthHandler.TakeDamage(this.dir * (this.damage + (float)this.level.LevelsUp() * this.damagePerLevel), base.transform.position, null, this.data.player, true, false);
			componentInParent.healthHandler.TakeForce(this.dir * (this.knockBack + (float)this.level.LevelsUp() * this.knockBackPerLevel), ForceMode2D.Impulse, false, false, 0f);
			this.data.healthHandler.TakeForce(-this.dir * this.knockBack, ForceMode2D.Impulse, false, true, 0f);
			this.data.healthHandler.TakeForce(-this.dir * this.stopForce, ForceMode2D.Impulse, true, true, 0f);
			this.data.block.RPCA_DoBlock(false, true, BlockTrigger.BlockTriggerType.ShieldCharge, default(Vector3), false);
			GamefeelManager.GameFeel(this.dir * this.shake);
		}
	}

	public void Collide(Vector2 pos, Vector2 colDir, Player player)
	{
		if (!this.data.view.IsMine)
		{
			return;
		}
		if (this.blockTime < 0f)
		{
			return;
		}
		CharacterData componentInParent = player.gameObject.GetComponentInParent<CharacterData>();
		if (this.hitDatas.Contains(componentInParent))
		{
			return;
		}
		this.hitDatas.Add(componentInParent);
		if (componentInParent)
		{
			base.GetComponentInParent<ChildRPC>().CallFunction("ShieldBashCollide", pos, colDir, player.playerID);
		}
	}

	[Header("Sounds")]
	public SoundEvent soundShieldCharge;

	[Header("Settings")]
	public float damagePerLevel;

	[Header("Settings")]
	public float knockBackPerLevel;

	public float forcePerLevel;

	public float timePerLevel;

	public ParticleSystem hitPart;

	public float shake;

	public float damage;

	public float knockBack;

	public float stopForce;

	public AnimationCurve forceCurve;

	public float force;

	public float drag;

	public float time;

	private CharacterData data;

	private AttackLevel level;

	private Vector3 dir;

	private bool cancelForce;

	private List<CharacterData> hitDatas = new List<CharacterData>();

	private float blockTime;
}
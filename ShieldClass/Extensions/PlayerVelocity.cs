using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnboundLib;

public static class PlayerVelocityExtension
{
	public static void AddForce(this PlayerVelocity playerVelocity, Vector2 force, ForceMode2D forceMode)
	{
		//typeof(PlayerVelocity).InvokeMember("GetPlayerWithID",
		//	BindingFlags.Instance | BindingFlags.InvokeMethod |
		//	BindingFlags.NonPublic, null, playerVelocity, new object[] { force, forceMode });

		if (forceMode == ForceMode2D.Force)
		{
			force *= 0.02f;
		}
		else
		{
			force *= 1f;
		}

		playerVelocity.SetFieldValue("velocity", (Vector2)playerVelocity.GetFieldValue("velocity") + (force / (float)playerVelocity.GetFieldValue("mass")));
	}

	public static Vector2 GetVelocity(this PlayerVelocity playerVelocity)
	{
		return (Vector2)playerVelocity.GetFieldValue("velocity");

	}

	public static float GetMass(this PlayerVelocity playerVelocity)
	{
		return (float)playerVelocity.GetFieldValue("mass");

	}
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using ArtemCore;
using HarmonyLib;

namespace SynchronizedStamina.InteractionWithOtherMods
{
	[HarmonyPatch]
	public class SynchronizedStaminaCinematicCombatInteraction
	{
		private static IEnumerable<MethodBase> TargetMethods()
		{
			yield return AccessTools.Method(AccessTools.TypeByName("CinematicCombatMissionLogic"), "CinematicCombatDodgeLogic");
			yield return AccessTools.Method(AccessTools.TypeByName("CCDodgeBehavior"), "TryDodge");
		}

		// Check whether Artem's Cinematic Combat is loaded.
		private static bool Prepare() => TargetMethods().All(method => method != null);

		private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			List<CodeInstruction> codes = instructions.ToList(), codesToModify = new List<CodeInstruction>();

			for (int i = 0; i < codes.Count; i++)
			{
				if (codes[i].operand is FieldInfo field && (field == AccessTools.Field(typeof(ArtemCore.ArtemCore), "_playerStamina") || field == AccessTools.Field(typeof(ArtemCoreAgentComponent), "_aiStamina")) && codes[i + 1].opcode == OpCodes.Ldc_R4)
				{
					codesToModify.Add(codes[i + 1]);
				}
			}

			foreach (CodeInstruction code in codesToModify.Skip(1))
			{
				codes.Insert(codes.IndexOf(code) + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(SynchronizedStaminaCinematicCombatInteraction), "GetStaminaDrain", new Type[] { typeof(float) })));
			}

			return codes;
		}

		// Multiply the stamina drain in Artem's mods to match the same scale as RBM.
		private static float GetStaminaDrain(float oldValue) => oldValue * SynchronizedStaminaSettings.Instance.DodgingStaminaDrainMultiplier;
	}
}

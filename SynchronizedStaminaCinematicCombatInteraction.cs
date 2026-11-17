using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using ArtemCore;
using HarmonyLib;
using TaleWorlds.MountAndBlade;

namespace SynchronizedStamina
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
			List<CodeInstruction> codes = instructions.ToList(), codesToReplace = new List<CodeInstruction>(), codesToRemove = new List<CodeInstruction>(), codesToInsert = new List<CodeInstruction>();
			int index;

			for (int i = 0; i < codes.Count; i++)
			{
				if (codes[i].operand is FieldInfo field && (field == AccessTools.Field(typeof(ArtemCore.ArtemCore), "_playerStamina") || field == AccessTools.Field(typeof(ArtemCoreAgentComponent), "_aiStamina")))
				{
					codesToReplace.Add(codes[i]);
				}
				else if (codes[i].operand is MethodInfo method && method == AccessTools.Method(typeof(Agent), "GetComponent", generics: new Type[] { typeof(ArtemCoreAgentComponent) }))
				{
					codesToRemove.Add(codes[i]);
				}
			}

			// Skip the first match for _playerStamina and _aiStamina.
			foreach (CodeInstruction code in codesToReplace.Skip(1))
			{
				// Route the stamina in Artem's mods to the stamina in RBM.
				codesToInsert.Clear();

				if (code.opcode == OpCodes.Ldsfld || code.opcode == OpCodes.Stsfld)
				{
					codesToInsert.Add(new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(Agent), "Main")));
				}

				if (code.opcode == OpCodes.Ldsfld || code.opcode == OpCodes.Ldfld)
				{
					codesToInsert.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(SynchronizedStaminaCinematicChargesInteraction), "GetStamina", new Type[] { typeof(Agent) })));
				}
				else if (code.opcode == OpCodes.Stsfld || code.opcode == OpCodes.Stfld)
				{
					codesToInsert.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(SynchronizedStaminaCinematicChargesInteraction), "SetStamina", new Type[] { typeof(float), typeof(Agent) })));
				}

				index = codes.IndexOf(code);

				if (codes[index + 1].opcode == OpCodes.Ldc_R4)
				{
					codes.Insert(index + 2, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(SynchronizedStaminaCinematicCombatInteraction), "GetStaminaDrain", new Type[] { typeof(float) })));
				}

				codes[index].opcode = OpCodes.Nop;
				codes.InsertRange(index + 1, codesToInsert);
			}

			// Skip the first match for GetComponent<ArtemCoreAgentComponent>.
			foreach (CodeInstruction code in codesToRemove.Skip(1))
			{
				codes.Remove(code);
			}

			return codes;
		}

		// Multiply the stamina drain in Artem's mods to match the same scale as RBM.
		private static float GetStaminaDrain(float initialAmount) => initialAmount * SynchronizedStaminaSettings.Instance.DodgingStaminaDrainMultiplier;
	}
}

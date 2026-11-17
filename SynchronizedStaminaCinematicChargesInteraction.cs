using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using ArtemCore;
using ArtemsCinematicCharges;
using HarmonyLib;
using RBMStance;
using TaleWorlds.MountAndBlade;

namespace SynchronizedStamina
{
	public class SynchronizedStaminaCinematicChargesInteraction
	{
		[HarmonyPatch]
		public class SynchronizedStaminaCinematicChargesInteractionSprinting
		{
			private static IEnumerable<MethodBase> TargetMethods()
			{
				yield return AccessTools.Method(typeof(SprintBehavior), "StartSprint");
				yield return AccessTools.Method(typeof(SprintAgentComponent), "OnTickParallel");
			}

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
					else if (codes[i].operand is MethodInfo method && method == AccessTools.Method(typeof(Agent), "GetComponent", generics: new Type[] { typeof(ArtemCoreAgentComponent) }) && codes[i - 1].opcode == OpCodes.Ldfld)
					{
						codesToRemove.Add(codes[i - 2]);
						codesToRemove.Add(codes[i - 1]);
						codesToRemove.Add(codes[i]);
					}
				}

				// Skip the first 2 matches for _playerStamina and _aiStamina.
				foreach (CodeInstruction code in codesToReplace.Skip(2))
				{
					// Route the stamina in Artem's mods to the stamina in RBM.
					codesToInsert.Clear();
					codesToInsert.Add(new CodeInstruction(OpCodes.Ldloc_0));

					if (code.opcode == OpCodes.Ldsfld || code.opcode == OpCodes.Ldfld)
					{
						codesToInsert.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(SynchronizedStaminaCinematicChargesInteraction), "GetStamina", new Type[] { typeof(Agent) })));
					}
					else if (code.opcode == OpCodes.Stsfld || code.opcode == OpCodes.Stfld)
					{
						codesToInsert.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(SynchronizedStaminaCinematicChargesInteraction), "SetStamina", new Type[] { typeof(float), typeof(Agent) })));
					}

					index = codes.IndexOf(code);
					codes[index].opcode = OpCodes.Nop;
					codes.InsertRange(index + 1, codesToInsert);
				}

				// Skip the first 6 matches for this.Agent.GetComponent<ArtemCoreAgentComponent>.
				foreach (CodeInstruction code in codesToRemove.Skip(6))
				{
					codes.Remove(code);
				}

				return codes;
			}
		}

		[HarmonyPatch]
		public class SynchronizedStaminaCinematicChargesInteractionStamina
		{
			private static IEnumerable<MethodBase> TargetMethods()
			{
				yield return AccessTools.Method(typeof(SprintBehavior), "GetMaxStamina");
				yield return AccessTools.Method(typeof(SprintAgentComponent), "GetMaxStamina");
			}

			public static void Postfix(ref float __result, Agent agent)
			{
				if (agent != null && AgentStances.values.TryGetValue(agent, out Stance stance))
				{
					// Route the max stamina in Artem's mods to the max stamina in RBM.
					__result = stance.maxStamina;
				}
			}
		}

		[HarmonyPatch]
		public class SynchronizedStaminaCinematicChargesInteractionStaminaDrain
		{
			private static IEnumerable<MethodBase> TargetMethods()
			{
				yield return AccessTools.Method(typeof(SprintBehavior), "GetDrainPerSecond");
				yield return AccessTools.Method(typeof(SprintAgentComponent), "GetDrainPerSecond");
			}

			// Multiply the stamina drain in Artem's mods to match the same scale as RBM.
			private static void Postfix(ref float __result) => __result *= SynchronizedStaminaSettings.Instance.SprintingStaminaDrainMultiplier;
		}

		[HarmonyPatch(typeof(SprintMixin))]
		public class SynchronizedStaminaCinematicChargesInteractionMixin
		{
			[HarmonyPatch("SprintFloat", MethodType.Setter)]
			public static void Prefix(ref int value) => value = (int)GetStamina(Agent.Main);

			[HarmonyPatch("ShowSprintFloat", MethodType.Getter)]
			public static void Postfix(ref bool __result)
			{
				if (SynchronizedStaminaSettings.Instance.StaminaBarToShow.SelectedIndex == 0)
				{
					// Hide Artem's stamina bar.
					__result = false;
				}
			}
		}

		private static float GetStamina(Agent agent)
		{
			if (agent != null && AgentStances.values.TryGetValue(agent, out Stance stance))
			{
				return stance.stamina;
			}

			return 0;
		}

		private static void SetStamina(float amount, Agent agent)
		{
			if (agent != null && AgentStances.values.TryGetValue(agent, out Stance stance))
			{
				stance.stamina = amount;
			}
		}
	}
}

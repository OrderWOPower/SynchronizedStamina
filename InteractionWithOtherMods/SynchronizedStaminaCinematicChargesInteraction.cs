using System.Collections.Generic;
using System.Reflection;
using ArtemsCinematicCharges;
using HarmonyLib;
using RBMStance;
using TaleWorlds.MountAndBlade;

namespace SynchronizedStamina.InteractionWithOtherMods
{
	public class SynchronizedStaminaCinematicChargesInteraction
	{
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

		[HarmonyPatch]
		public class SynchronizedStaminaCinematicChargesInteractionStaminaRegen
		{
			private static IEnumerable<MethodBase> TargetMethods()
			{
				yield return AccessTools.Method(typeof(SprintBehavior), "RegenStamina");
				yield return AccessTools.Method(typeof(SprintAgentComponent), "RegenStamina");
			}

			// Skip Artem's stamina regeneration.
			private static bool Prefix() => false;
		}

		[HarmonyPatch(typeof(SprintMixin), "ShowSprintFloat", MethodType.Getter)]
		public class SynchronizedStaminaCinematicChargesInteractionMixin
		{
			public static void Postfix(ref bool __result)
			{
				if (SynchronizedStaminaSettings.Instance.StaminaBarToShow.SelectedIndex == 0)
				{
					// Hide Artem's stamina bar.
					__result = false;
				}
			}
		}
	}
}

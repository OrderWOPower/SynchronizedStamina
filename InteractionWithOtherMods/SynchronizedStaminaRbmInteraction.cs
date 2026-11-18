using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RBMStance;

namespace SynchronizedStamina.InteractionWithOtherMods
{
	[HarmonyPatch]
	public class SynchronizedStaminaRbmInteraction
	{
		private static IEnumerable<MethodBase> TargetMethods()
		{
			yield return AccessTools.PropertyGetter(typeof(StanceVisualVM), "ShowPlayerStaminaBar");
			yield return AccessTools.PropertyGetter(typeof(StanceVisualVM), "ShowPlayerStaminaBarVertical");
		}

		public static void Postfix(ref bool __result)
		{
			if (SynchronizedStaminaSettings.Instance.StaminaBarToShow.SelectedIndex == 1)
			{
				// Hide RBM's stamina bar.
				__result = false;
			}
		}
	}
}

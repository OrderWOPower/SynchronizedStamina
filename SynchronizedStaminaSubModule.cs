using HarmonyLib;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace SynchronizedStamina
{
	// This mod makes RBM and Artem's mods share the same stamina pool.
	public class SynchronizedStaminaSubModule : MBSubModuleBase
	{
		private Harmony _harmony;

		protected override void OnSubModuleLoad() => _harmony = new Harmony("mod.bannerlord.synchronizedstamina");

		protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
		{
			if (RBMConfig.RBMConfig.rbmStanceEnabled && RBMConfig.RBMConfig.staminaEnabled)
			{
				_harmony.PatchAll();
			}
		}

		public override void OnGameEnd(Game game) => _harmony.UnpatchAll("mod.bannerlord.synchronizedstamina");
	}
}

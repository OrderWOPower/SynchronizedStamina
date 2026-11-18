using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace SynchronizedStamina
{
	public class SynchronizedStaminaMissionBehavior : MissionBehavior
	{
		public override MissionBehaviorType BehaviorType => MissionBehaviorType.Other;

		public override void OnAgentBuild(Agent agent, Banner banner)
		{
			if (agent.IsHuman)
			{
				agent.AddComponent(new SynchronizedStaminaAgentComponent(agent));
			}
		}
	}
}

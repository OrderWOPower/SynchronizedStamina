using ArtemCore;
using RBMStance;
using TaleWorlds.MountAndBlade;

namespace SynchronizedStamina
{
	public class SynchronizedStaminaAgentComponent : AgentComponent
	{
		private readonly Stance _stance;
		private readonly ArtemCoreAgentComponent _artemCoreAgentComponent;
		private float _currentValue;

		public SynchronizedStaminaAgentComponent(Agent agent) : base(agent)
		{
			if (AgentStances.values.TryGetValue(agent, out Stance stance))
			{
				_stance = stance;
			}

			_artemCoreAgentComponent = agent.GetComponent<ArtemCoreAgentComponent>();
			_currentValue = float.NegativeInfinity;
		}

		public override void OnTick(float dt)
		{
			if (Agent.IsMainAgent)
			{
				if (ArtemCore.ArtemCore._playerStamina < _currentValue)
				{
					_stance.stamina = ArtemCore.ArtemCore._playerStamina;
				}

				ArtemCore.ArtemCore._playerStamina = _stance.stamina;
				_currentValue = ArtemCore.ArtemCore._playerStamina;
			}
			else
			{
				if (_artemCoreAgentComponent._aiStamina < _currentValue)
				{
					_stance.stamina = _artemCoreAgentComponent._aiStamina;
				}

				_artemCoreAgentComponent._aiStamina = _stance.stamina;
				_currentValue = _artemCoreAgentComponent._aiStamina;
			}
		}
	}
}

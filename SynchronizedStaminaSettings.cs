using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Base.Global;
using MCM.Common;
using TaleWorlds.Localization;

namespace SynchronizedStamina
{
	public class SynchronizedStaminaSettings : AttributeGlobalSettings<SynchronizedStaminaSettings>
	{
		public override string Id => "SynchronizedStamina";

		public override string DisplayName => "Synchronized Stamina";

		public override string FolderName => "SynchronizedStamina";

		public override string FormatType => "json2";

		[SettingPropertyDropdown("Stamina Bar", Order = 0, RequireRestart = false, HintText = "Which mod's stamina bar to show. Default is RBM.")]
		[SettingPropertyGroup("Visuals", GroupOrder = 0)]
		public Dropdown<TextObject> StaminaBarToShow { get; set; } = new Dropdown<TextObject>(new TextObject[] { new TextObject("RBM"), new TextObject("Artem's"), new TextObject("Both") }, 0);

		[SettingPropertyInteger("Sprinting", 1, 100, "0", Order = 0, RequireRestart = false, HintText = "Multiplier for stamina drain when sprinting. Default is 10.")]
		[SettingPropertyGroup("Stamina Drain Multipliers", GroupOrder = 1)]
		public int SprintingStaminaDrainMultiplier { get; set; } = 10;

		[SettingPropertyInteger("Dodging", 1, 100, "0", Order = 1, RequireRestart = false, HintText = "Multiplier for stamina drain when dodging. Default is 10.")]
		[SettingPropertyGroup("Stamina Drain Multipliers", GroupOrder = 1)]
		public int DodgingStaminaDrainMultiplier { get; set; } = 10;
	}
}

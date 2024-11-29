namespace Scabine.App;

using System.Text.Json.Serialization;

internal class EngineMatchDefinition
{
	public EngineInfo[] EngineInfos;
	public string[] PresetNames;
	public ThinkingLimit[] ThinkingLimits;

	[JsonConstructor]
	public EngineMatchDefinition(EngineInfo[] engineInfos, string[] presetNames, ThinkingLimit[] thinkingLimits)
	{
		EngineInfos = engineInfos;
		PresetNames = presetNames;
		ThinkingLimits = thinkingLimits;
	}

	public EngineMatchDefinition(EngineInfo whiteEngineInfo, string whitePresetName, ThinkingLimit whiteLimit, EngineInfo blackEngineInfo, string blackPresetName, ThinkingLimit blackLimit)
	{
		EngineInfos = new EngineInfo[2] { whiteEngineInfo, blackEngineInfo };
		PresetNames = new string[2] { whitePresetName, blackPresetName };
		ThinkingLimits = new ThinkingLimit[2] { whiteLimit, blackLimit };
	}
}

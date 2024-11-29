namespace Scabine.App;

using System.Text.Json.Serialization;

internal class PlayerMatchDefinition
{
	public bool PlayerUnlimited;
	public int PlayerTime;
	public int PlayerIncrement;
	public int PlayerSide;
	public EngineInfo EngineInfo;
	public string PresetName;
	public ThinkingLimit ThinkingLimit;

	[JsonConstructor]
	public PlayerMatchDefinition(bool playerUnlimited, int playerTime, int playerIncrement, int playerSide, EngineInfo engineInfo, string presetName, ThinkingLimit thinkingLimit)
	{
		PlayerUnlimited = playerUnlimited;
		PlayerTime = playerTime;
		PlayerIncrement = playerIncrement;
		PlayerSide = playerSide;
		EngineInfo = engineInfo;
		PresetName = presetName;
		ThinkingLimit = thinkingLimit;
	}
}

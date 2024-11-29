namespace Scabine.Engines;

public sealed class SpinOption : UciOption
{
	public readonly int Value;
	public readonly int Min;
	public readonly int Max;

	public SpinOption(string name, int value, int min, int max)
		: base(name)
	{
		Value = value;
		Min = min;
		Max = max;
	}

	public override object GetValue()
	{
		return Value;
	}

	public override string? FormatValue(object value)
	{
		return value is int intValue && intValue >= Min && intValue <= Max ? intValue.ToString() : null;
	}
}

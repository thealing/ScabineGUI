namespace Scabine.Engines;

public sealed class CheckOption : UciOption
{
	public readonly bool Value;

	public CheckOption(string name, bool value) 
		: base(name)
	{
		Value = value;
	}

	public override object GetValue()
	{
		return Value;
	}

	public override string? FormatValue(object value)
	{
		return value is bool boolValue ? (boolValue ? "true" : "false") : null;
	}
}

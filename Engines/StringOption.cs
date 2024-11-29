namespace Scabine.Engines;

public sealed class StringOption : UciOption
{
	public readonly string Value;

	public StringOption(string name, string value)
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
		return value is string stringValue ? stringValue : null;
	}
}

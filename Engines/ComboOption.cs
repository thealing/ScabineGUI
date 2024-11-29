namespace Scabine.Engines;

using System.Linq;

public sealed class ComboOption : UciOption
{
	public readonly string Value;
	public readonly string[] Options;

	public ComboOption(string name, string value, string[] options)
		: base(name)
	{
		Value = value;
		Options = options;
	}

	public override object GetValue()
	{
		return Value;
	}

	public override string? FormatValue(object value)
	{
		return value is string stringValue && Options.Contains(stringValue) ? stringValue : null;
	}
}

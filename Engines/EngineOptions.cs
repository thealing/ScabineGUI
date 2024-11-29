namespace Scabine.Engines;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

public sealed class EngineOptions
{
	public readonly Dictionary<UciOption, object> Options;

	public EngineOptions()
	{
		Options = new Dictionary<UciOption, object>();
	}

	public EngineOptions(IEnumerable<UciOption> options)
	{
		Options = new Dictionary<UciOption, object>();
		foreach (UciOption option in options)
		{
			Options[option] = option.GetValue();
		}
	}

	public EngineOptions(EngineOptions other)
	{
		Options = new Dictionary<UciOption, object>(other.Options);
	}

	public class Converter : JsonConverter<EngineOptions>
	{
		public override EngineOptions Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options)
		{
			List<KeyValuePair<UciOption, JsonElement>>? pairs = JsonSerializer.Deserialize<List<KeyValuePair<UciOption, JsonElement>>>(ref reader, options);
			if (pairs == null)
			{
				return new EngineOptions();
			}
			EngineOptions values = new EngineOptions(pairs.Select(pair => pair.Key));
			foreach (var (key, value) in pairs)
			{
				values.Options[key] = JsonSerializer.Deserialize(value, values.Options[key].GetType(), options) ?? values.Options[key];
			}
			return values;
		}

		public override void Write(Utf8JsonWriter writer, EngineOptions value, JsonSerializerOptions options)
		{
			JsonSerializer.Serialize(writer, new List<KeyValuePair<UciOption, object>>(value.Options), options);
		}
	}
}

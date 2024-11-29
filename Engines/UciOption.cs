namespace Scabine.Engines;

using System.Text.Json.Serialization;
using System.Text.Json;
using System;
using Scabine.App.Prefs;
using System.IO.Pipes;
using System.Reflection;

public abstract class UciOption
{
	public readonly string Name;

	protected UciOption(string name)
	{
		Name = name;
	}

	public abstract object GetValue();

	public abstract string? FormatValue(object value);

	public class Converter : JsonConverter<UciOption>
	{
		public override UciOption? Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options)
		{
			using JsonDocument document = JsonDocument.ParseValue(ref reader);
			JsonElement jsonObject = document.RootElement;
			JsonElement typeElement = jsonObject.GetProperty("$type");
			string? typeName = typeElement.GetString();
			if (typeName == null)
			{
				return null;
			}
			Type? realType = Type.GetType(typeName);
			if (realType == null)
			{
				return null;
			}
			return (UciOption?)JsonSerializer.Deserialize(jsonObject.GetRawText(), realType, options);
		}

		public override void Write(Utf8JsonWriter writer, UciOption value, JsonSerializerOptions options)
		{
			string typeName = value.GetType().FullName ?? "";
			writer.WriteStartObject();
			writer.WriteString("$type", typeName);
			foreach (FieldInfo property in value.GetType().GetFields())
			{
				object propertyValue = property.GetValue(value)!;
				writer.WritePropertyName(property.Name);
				JsonSerializer.Serialize(writer, propertyValue, property.FieldType, options);
			}
			writer.WriteEndObject();
		}
	}
}

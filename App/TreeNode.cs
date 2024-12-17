namespace Scabine.App;

using Scabine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Text.Json.Nodes;

internal class TreeNode
{
	public readonly Move? Move;
	public readonly string Uci;
	public readonly string San;
	public readonly TreeNode? Parent;
	public readonly List<TreeNode> Children;
	public readonly int Rank;
	public readonly int Color;

	internal TreeNode? CurrentChild;
	internal bool IsMainLine;
	internal bool IsCollapsed;
	internal int? Time;
	internal int? Eval;
	internal int? Class;
	internal string? Comment;

	public TreeNode AddChild(Move move, string uci, string san)
	{
		TreeNode? child = Children.Find(node => node.Move == move);
		if (child == null)
		{
			child = new TreeNode(move, uci, san, this, Color);
			child.IsMainLine = IsMainLine && !Children.Any();
			Children.Add(child);
		}
		return child;
	}

	private TreeNode(Move? move, string? uci, string? san, TreeNode? parent, int color)
	{
		Move = move;
		Uci = uci ?? "";
		San = san ?? "";
		Parent = parent;
		Children = new List<TreeNode>();
		Rank = parent == null ? color - 1 : parent.Rank + 1;
		Color = color ^ 1;
	}

	private TreeNode(Move? move, string uci, string san, TreeNode? parent, int color, int rank)
	{
		Move = move;
		Uci = uci;
		San = san;
		Parent = parent;
		Children = new List<TreeNode>();
		Color = color;
		Rank = rank;
	}

	public static TreeNode CreateRoot(int color)
	{
		TreeNode root = new TreeNode(null, null, null, null, color);
		root.IsMainLine = true;
		return root;
	}

	public class Converter : JsonConverter<TreeNode>
	{
		public override TreeNode? Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options)
		{
			JsonObject? obj = JsonSerializer.Deserialize<JsonObject>(ref reader, options);
			return obj == null ? null : DeserializeNode(obj, null, options);
		}

		public override void Write(Utf8JsonWriter writer, TreeNode value, JsonSerializerOptions options)
		{
			writer.WriteStartObject();
			writer.WritePropertyName(nameof(Move));
			JsonSerializer.Serialize(writer, value.Move, options);
			writer.WritePropertyName(nameof(Uci));
			writer.WriteStringValue(value.Uci);
			writer.WritePropertyName(nameof(San));
			writer.WriteStringValue(value.San);
			writer.WritePropertyName(nameof(Rank));
			writer.WriteNumberValue(value.Rank);
			writer.WritePropertyName(nameof(Color));
			writer.WriteNumberValue(value.Color);
			writer.WritePropertyName(nameof(IsMainLine));
			writer.WriteBooleanValue(value.IsMainLine);
			writer.WritePropertyName(nameof(IsCollapsed));
			writer.WriteBooleanValue(value.IsCollapsed);
			writer.WritePropertyName(nameof(Time));
			JsonSerializer.Serialize(writer, value.Time, options);
			writer.WritePropertyName(nameof(Eval));
			JsonSerializer.Serialize(writer, value.Eval, options);
			writer.WritePropertyName(nameof(Class));
			JsonSerializer.Serialize(writer, value.Class, options);
			writer.WritePropertyName(nameof(Comment));
			writer.WriteStringValue(value.Comment);
			writer.WritePropertyName(nameof(Children));
			writer.WriteStartArray();
			foreach (TreeNode child in value.Children)
			{
				JsonSerializer.Serialize(writer, child, options);
			}
			writer.WriteEndArray();
			writer.WritePropertyName(nameof(CurrentChild));
			writer.WriteNumberValue(value.CurrentChild == null ? -1 : value.Children.IndexOf(value.CurrentChild));
			writer.WriteEndObject();
		}

		private TreeNode DeserializeNode(JsonNode obj, TreeNode? parent, JsonSerializerOptions options)
		{
			Move? move = obj[nameof(Move)]?.Deserialize<Move>(options);
			string uci = obj[nameof(Uci)]?.ToString() ?? "";
			string san = obj[nameof(San)]?.ToString() ?? "";
			int color = obj[nameof(Color)]?.GetValue<int>() ?? 0;
			int rank = obj[nameof(Rank)]?.GetValue<int>() ?? 0;
			TreeNode node = new TreeNode(move, uci, san, parent, color, rank);
			node.IsMainLine = obj[nameof(IsMainLine)]?.GetValue<bool>() ?? false;
			node.IsCollapsed = obj[nameof(IsCollapsed)]?.GetValue<bool>() ?? false;
			node.Time = obj[nameof(Time)]?.GetValue<int>();
			node.Eval = obj[nameof(Eval)]?.GetValue<int>();
			node.Class = obj[nameof(Class)]?.GetValue<int>();
			node.Comment = obj[nameof(Comment)]?.GetValue<string>();
			JsonNode? childrenNode = obj[nameof(Children)];
			if (childrenNode != null)
			{
				foreach (JsonNode? childNode in childrenNode.AsArray())
				{
					if (childNode != null)
					{
						TreeNode child = DeserializeNode(childNode, node, options);
						node.Children.Add(child);
					}
				}
				if (int.TryParse(obj[nameof(CurrentChild)]?.ToString(), out int currentChildIndex))
				{
					if (currentChildIndex >= 0 && currentChildIndex < node.Children.Count())
					{
						node.CurrentChild = node.Children[currentChildIndex];
					}
				}
			}
			return node;
		}
	}
}

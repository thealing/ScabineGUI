namespace Scabine.App;

using Scabine.Core;
using Scabine.Engines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Text.Json;
using static Scabine.Core.Pieces;
using System.Text.Json.Nodes;

internal class TreeGame : UciGame
{
	public delegate void MoveEventHandler(object sender, Move move);

	public event MoveEventHandler? MovePlayed;
	public event MoveEventHandler? MoveUndone;

	public TreeGame()
		: this(TreeNode.CreateRoot(White))
	{
	}

	public new void SetFen(ReadOnlySpan<char> fen)
	{
		base.SetFen(fen);
		_fen = fen.ToString();
		_root = TreeNode.CreateRoot(GetCurrentColor());
		_node = _root;
		_result = base.GetResult();
	}

	public bool TryPlayMove(Move move)
	{
		if (!base.PlayMove(move))
		{
			return false;
		}
		base.UndoMove(move);
		return true;
	}

	public bool PlayUciMove(string move)
	{
		return PlayMove(ParseMove(move));
	}

	public new bool PlayMove(Move move)
	{
		string uci = FormatMoveToUci(move);
		string san = FormatMoveToSan(move);
		if (base.PlayMove(move))
		{
			_node.CurrentChild = _node.AddChild(move, uci, san);
			_node = _node.CurrentChild;
			if (_node.IsMainLine && _node.Children.Count == 0)
			{
				_result = base.GetResult();
			}
			RaiseMovePlayed(move);
			return true;
		}
		else
		{
			return false;
		}
	}

	public new void UndoMove(Move move)
	{
		if (_node.Parent != null)
		{
			_node = _node.Parent;
			_node.CurrentChild = null;
			base.UndoMove(move);
			RaiseMoveUndone(move);
		}
	}

	public void SetCurrentNode(TreeNode? node)
	{
		List<TreeNode> path = new List<TreeNode>();
		while (node != null && node != _root && node.CurrentChild == null)
		{
			path.Add(node);
			node = node.Parent;
		}
		if (node != null)
		{
			while (_node != node && _node.Move != null)
			{
				UndoMove(_node.Move.Value);
			}
			if (_node != node)
			{
				return;
			}
		}
		path.Reverse();
		foreach (TreeNode item in path)
		{
			if (item.Move == null || !PlayMove(item.Move.Value))
			{
				return;
			}
		}
	}

	public TreeNode GetLastNode()
	{
		TreeNode node = _root;
		while (node.Children.Any())
		{
			node = node.Children[0];
		}
		return node;
	}

	public new Result GetResult()
	{
		return _result;
	}

	public void Clear()
	{
		while (_node.Move != null)
		{
			UndoMove(_node.Move.Value);
		}
		_node.Children.Clear();
		UpdateResult();
	}

	public void PromoteNode(TreeNode node)
	{
		while (node.Parent != null)
		{
			TreeNode parent = node.Parent;
			SetMainLineNode(parent.Children[0], false);
			parent.Children.Remove(node);
			parent.Children.Insert(0, node);
			SetMainLineNode(parent.Children[0], true);
			node = parent;
		}
		UpdateResult();
	}

	public void DeleteNode(TreeNode node)
	{
		if (node.Parent == null)
		{
			return;
		}
		while (node.Parent.CurrentChild != null)
		{
			if (!GameManager.TryUndoMove())
			{
				return;
			}
		}
		node.Parent.Children.Remove(node);
		SetMainLineNode(node.Parent.Children.ElementAtOrDefault(0), node.Parent.IsMainLine);
		UpdateResult();
	}

	public void SwapWithNextNode(TreeNode node)
	{
		SwapWithSiblingNode(node, 1);
	}

	public void SwapWithPreviousNode(TreeNode node)
	{
		SwapWithSiblingNode(node, -1);
	}

	public bool IsEmpty()
	{
		return _root.Children.Count == 0;
	}

	public TreeNode GetRootNode()
	{
		return _root;
	}

	public TreeNode GetCurrentNode()
	{
		return _node;
	}

	public Move? GetLastMove()
	{
		return _node.Move;
	}

	private void UpdateResult()
	{
		Game game = new Game();
		game.Copy(this);
		TreeNode? node = _node;
		while (node != null && !node.IsMainLine)
		{
			if (node.Move != null)
			{
				game.UndoMove(node.Move.Value);
			}
			node = node.Parent;
		}
		while (node != null && node.Children.Count > 0)
		{
			node = node.Children[0];
			if (node.Move != null)
			{
				game.PlayMove(node.Move.Value);
			}
		}
		_result = game.GetResult();
	}

	private void SetMainLineNode(TreeNode? node, bool mainLine)
	{
		while (node != null)
		{
			node.IsMainLine = mainLine;
			node = node.Children.ElementAtOrDefault(0);
		}
	}

	private void SwapWithSiblingNode(TreeNode node, int delta)
	{
		TreeNode? parent = node.Parent;
		if (parent != null)
		{
			int index = parent.Children.IndexOf(node);
			int siblingIndex = index + delta;
			if (siblingIndex >= 0 && siblingIndex < parent.Children.Count)
			{
				TreeNode sibling = parent.Children[siblingIndex];
				if (!sibling.IsMainLine)
				{
					parent.Children[siblingIndex] = node;
					parent.Children[index] = sibling;
				}
			}
		}
	}

	private void RaiseMovePlayed(Move move)
	{
		MovePlayed?.Invoke(this, move);
	}

	private void RaiseMoveUndone(Move move)
	{
		MoveUndone?.Invoke(this, move);
	}

	private TreeGame(TreeNode root)
	{
		_fen = GetFen();
		_root = root;
		_node = root;
		_result = base.GetResult();
	}

	private TreeGame(string fen, TreeNode root)
	{
		base.SetFen(fen);
		_fen = fen;
		_root = root;
		_node = root;
		_result = base.GetResult();
	}

	private TreeGame(string fen, TreeNode root, Result result)
	{
		base.SetFen(fen);
		_fen = fen;
		_root = root;
		_node = root;
		_result = result;
	}

	private string _fen;
	private TreeNode _root;
	private TreeNode _node;
	private Result _result;

	public class Converter : JsonConverter<TreeGame>
	{
		public override TreeGame? Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options)
		{
			JsonObject? obj = JsonSerializer.Deserialize<JsonObject>(ref reader, options);
			string? fenString = obj?[nameof(_fen)]?.ToString();
			if (fenString == null)
			{
				return null;
			}
			string? rootString = obj?[nameof(_root)]?.ToString();
			if (rootString == null)
			{
				return null;
			}
			TreeNode? root = JsonSerializer.Deserialize<TreeNode>(rootString, options);
			if (root == null)
			{
				return null;
			}
			string? pathString = obj?[nameof(_node)]?.ToString();
			if (pathString == null)
			{
				return null;
			}
			Result result = (Result?)obj?[nameof(_result)]?.GetValue<int>() ?? Result.Ongoing;
			TreeGame game = new TreeGame(fenString, root, result);
			game._fen = fenString;
			List<int>? path = JsonSerializer.Deserialize<List<int>>(pathString, options);
			if (path != null)
			{
				TreeNode node = root;
				foreach (int index in path)
				{
					if (index < 0 || index >= node.Children.Count)
					{
						break;
					}
					Move? move = node.Children[index].Move;
					if (move == null)
					{
						break;
					}
					game.PlayMove(move.Value);
					node = node.Children[index];
				}
			}
			return game;
		}

		public override void Write(Utf8JsonWriter writer, TreeGame value, JsonSerializerOptions options)
		{
			List<int> path = new List<int>();
			for (TreeNode node = value._node; node != value._root && node.Parent != null; node = node.Parent)
			{
				path.Add(node.Parent.Children.IndexOf(node));
			}
			path.Reverse();
			writer.WriteStartObject();
			writer.WritePropertyName(nameof(_fen));
			JsonSerializer.Serialize(writer, value._fen, options);
			writer.WritePropertyName(nameof(_result));
			JsonSerializer.Serialize(writer, value._result, options);
			writer.WritePropertyName(nameof(_root));
			JsonSerializer.Serialize(writer, value._root, options);
			writer.WritePropertyName(nameof(_node));
			JsonSerializer.Serialize(writer, path, options);
			writer.WriteEndObject();
		}
	}
}

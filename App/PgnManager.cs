namespace Scabine.App;

using Scabine.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using static Scabine.Core.Pieces;
using static Scabine.Core.Game;
using System.IO;
using System.Linq;
using Scabine.App.Prefs;
using System.Reflection;
using System.Xml.Linq;

internal static class PgnManager
{
	public static void NewGame()
	{
		NewGame(StartFen);
	}

	public static void NewGame(string fen)
	{
		SoundManager.StopAllSounds();
		MatchManager.ClearMatch();
		GameManager.SetPosition(fen);
		_values.Clear();
		SetDefaults();
		SetCurrentTime();
		SetGameFen();
		_dirty = false;
	}

	public static void SetPlayerMatch(PlayerMatchDefinition matchDefinition)
	{
		_values.Clear();
		SetDefaults();
		SetCurrentTime();
		SetGameFen();
		_values["Event"] = "Player against engine";
		_values["TimeControl"] = matchDefinition.PlayerUnlimited ? "Unlimited" : $"{matchDefinition.PlayerTime / 60}:{matchDefinition.PlayerTime % 60}";
		switch (matchDefinition.PlayerSide)
		{
			case White:
				_values["White"] = General.Name;
				_values["Black"] = matchDefinition.EngineInfo.Name;
				break;
			case Black:
				_values["White"] = matchDefinition.EngineInfo.Name;
				_values["Black"] = General.Name;
				break;
		}
		_values["Result"] = "*";
		if (matchDefinition.PlayerUnlimited)
		{
			_values["TimeControl"] = "-";
		}
		else
		{
			int time = matchDefinition.PlayerTime;
			int increment = matchDefinition.PlayerIncrement;
			_values["TimeControl"] = $"{time}+{increment}";
		}
		_values[GetEnginePresetTag(matchDefinition.PlayerSide ^ 1)] = matchDefinition.PresetName;
		_values[GetEngineLimitTag(matchDefinition.PlayerSide ^ 1)] = matchDefinition.ThinkingLimit.ToString();
	}

	public static void SetEngineMatch(EngineMatchDefinition matchDefinition)
	{
		_values.Clear();
		SetDefaults();
		SetCurrentTime();
		SetGameFen();
		_values["Event"] = "Engine match";
		_values["White"] = matchDefinition.EngineInfos[White].Name;
		_values["Black"] = matchDefinition.EngineInfos[Black].Name;
		_values["Result"] = "*";
		if (matchDefinition.ThinkingLimits.All(limit => limit.Mode == ThinkingMode.GameTime))
		{
			int time = (int)matchDefinition.ThinkingLimits.Max(limit => limit.BaseTime);
			int increment = (int)matchDefinition.ThinkingLimits.Max(limit => limit.Increment);
			_values["TimeControl"] = $"{time}+{increment}";
		}
		else
		{
			_values["TimeControl"] = "-";
		}
		for (int color = 0; color < ColorCount; color++)
		{
			_values[GetEnginePresetTag(color)] = matchDefinition.PresetNames[color];
			_values[GetEngineLimitTag(color)] = matchDefinition.ThinkingLimits[color].ToString();
		}
	}

	public static string GetTitle()
	{
		StringBuilder title = new StringBuilder();
		title.Append(GetValue("Event"));
		if (HasValue("White") && HasValue("Black") && HasValue("Result"))
		{
			title.Append("  |  ");
			for (int color = 0; color < ColorCount; color++)
			{
				title.Append(GetValue(_colorNames[color]));
				if (_values.TryGetValue(GetEnginePresetTag(color), out string? enginePreset) && enginePreset != null)
				{
					title.Append(" [");
					title.Append(enginePreset);
					title.Append("]");
				}
				if (color + 1 < ColorCount)
				{
					title.Append(" - ");
				}
			}
			title.Append("  |  ");
			title.Append(GetValue("Result"));
		}
		return title.ToString();
	}

	public static void SetWinner(int winner)
	{
		switch (winner)
		{
			case White:
				_values["Result"] = "1-0";
				break;
			case Black:
				_values["Result"] = "0-1";
				break;
		}
	}

	public static void SetDraw()
	{
		_values["Result"] = "1/2-1/2";
	}

	public static void SetResult(Result result)
	{
		switch (result)
		{
			case Result.WhiteWon:
				_values["Result"] = "1-0";
				break;
			case Result.BlackWon:
				_values["Result"] = "0-1";
				break;
			case Result.Draw:
				_values["Result"] = "1/2-1/2";
				break;
			case Result.Ongoing:
				_values["Result"] = "*";
				break;
		}
	}

	public static void SetDefaults()
	{
		_values["Event"] = "Custom game";
		_values["Site"] = "Scabine GUI";
	}

	public static void SetCurrentTime()
	{
		_values["Date"] = DateTime.Now.ToString("yyyy.MM.dd");
		_values["Time"] = DateTime.Now.ToString("HH:mm:ss");
	}

	public static void SetGameFen()
	{
		string fen = GameManager.GetGame().GetFen();
		if (fen != StartFen)
		{
			_values["FEN"] = fen;
			_values["SetUp"] = "1";
		}
	}

	public static string GetPgn()
	{
		StringBuilder pgn = new StringBuilder();
		FormatHeader(pgn);
		pgn.AppendLine();
		FormatMoves(pgn);
		pgn.AppendLine();
		return pgn.ToString();
	}

	public static void SetPgn(string pgn)
	{
		ParseHeader(pgn);
		if (HasValue("FEN"))
		{
			NewGame(GetValue("FEN"));
		}
		else
		{
			NewGame();
		}
		pgn = ParseHeader(pgn);
		pgn = ParseMoves(pgn);
	}

	public static bool HasValue(string key)
	{
		return _values.ContainsKey(key);
	}

	public static string GetValue(string key)
	{
		return _values.GetValueOrDefault(key, "?");
	}

	public static void SetValue(string key, string value)
	{
		if (value != GetValue(key))
		{
			_dirty = true;
		}
		_values[key] = value;
	}

	public static IDictionary<string, string> GetHeader()
	{
		IDictionary<string, string> orderedValues = new OrderedDictionary<string, string>();
		foreach (string entry in _entries)
		{
			if (_values.ContainsKey(entry))
			{
				orderedValues[entry] = _values[entry];
			}
		}
		foreach (KeyValuePair<string, string> pair in _values)
		{
			if (_entries.Contains(pair.Key))
			{
				continue;
			}
			orderedValues.Add(pair);
		}
		return orderedValues;
	}

	public static void SetHeader(IDictionary<string, string> header)
	{
		_values.Clear();
		SetDefaults();
		foreach (KeyValuePair<string, string> pair in header)
		{
			_values[pair.Key] = pair.Value;
		}
	}

	public static bool IsDirty()
	{
		return _dirty;
	}

	public static bool SaveBackup()
	{
		if (_dirty)
		{
			string localFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			string backupFolder = localFolder + Path.DirectorySeparatorChar + "Scabine";
			Directory.CreateDirectory(backupFolder);
			string MakePath(int number)
			{
				return backupFolder + Path.DirectorySeparatorChar + number.ToString() + ".pgn";
			}
			int low = 1;
			int high = 1000000000;
			while (low < high)
			{
				int middle = (low + high) / 2;
				if (File.Exists(MakePath(middle)))
				{
					low = middle + 1;
				}
				else
				{
					high = middle;
				}
			}
			string content = GetPgn();
			File.WriteAllText(MakePath(low), content);
			_dirty = false;
		}
		return !_dirty;
	}

	public static void Update()
	{
		if (GameManager.IsDirty())
		{
			_dirty = true;
		}
		if (GameManager.GetGame().IsEmpty())
		{
			_dirty = false;
		}
		SaveManager.Save += () =>
		{
			if (SaveManager.Sync("pgn", ref _values) && !_values.Any())
			{
				SetDefaults();
				SetCurrentTime();
			}
		};
	}

	public static void FormatHeader(StringBuilder pgn)
	{
		IDictionary<string, string> header = GetHeader();
		foreach (KeyValuePair<string, string> pair in header)
		{
			pgn.AppendLine($"[{pair.Key} \"{pair.Value}\"]");
		}
	}

	public static void FormatMoves(StringBuilder pgn, bool comments = true)
	{
		TreeNode root = GameManager.GetGame().GetRootNode();
		void AppendNode(TreeNode node, bool start)
		{
			if (start)
			{
				FormatMove(pgn, node, start, comments);
			}
			if (node.Children.Count == 0)
			{
				return;
			}
			if (node != root)
			{
				pgn.Append(' ');
			}
			TreeNode next = node.Children[0];
			FormatMove(pgn, next, next.Color == White, comments);
			for (int i = 1; i < node.Children.Count; i++)
			{
				pgn.Append(' ');
				pgn.Append('(');
				AppendNode(node.Children[i], true);
				pgn.Append(')');
			}
			AppendNode(next, false);
		}
		AppendNode(root, false);
	}

	public static void FormatLine(StringBuilder pgn, TreeNode end, bool comments)
	{
		List<TreeNode> path = new List<TreeNode>();
		while (end?.Parent != null)
		{
			path.Add(end);
			end = end.Parent;
		}
		path.Reverse();
		foreach (TreeNode node in path)
		{
			FormatMove(pgn, node, node.Color == White || node == path.First(), comments);
			if (node != path.Last())
			{
				pgn.Append(' ');
			}
		}
	}

	private static void FormatMove(StringBuilder pgn, TreeNode node, bool number, bool comment)
	{
		if (number)
		{
			pgn.Append(node.Rank / 2 + 1);
			pgn.Append(node.Color == White ? "." : "...");
		}
		pgn.Append(node.San);
		if (comment && node.Time != null)
		{
			pgn.Append(" { ");
			int time = node.Time.Value;
			int hour = time / 3600000;
			int minutes = time / 60000 % 60;
			int seconds = time / 1000 % 60;
			int milliseconds = time % 1000;
			pgn.Append($"[%clk {hour}:{minutes:D2}:{seconds:D2}.{milliseconds:D3}]");
			pgn.Append(" }");
		}
		if (comment && node.Eval != null)
		{
			pgn.Append(" { ");
			pgn.Append($"[%eval {EvalToString(node.Eval.Value)}]");
			pgn.Append(" }");
		}
		if (comment && node.Class != null)
		{
			pgn.Append(" { ");
			pgn.Append($"[%cls {node.Class}]");
			pgn.Append(" }");
		}
		if (comment && node.Comment != null)
		{
			pgn.Append(" { " + node.Comment + " }");
		}
	}

	private static string ParseHeader(string pgn)
	{
		Regex regex = new Regex(@"\[(\w+)\s+""([^""]+)""\]");
		foreach (Match match in regex.Matches(pgn))
		{
			string key = match.Groups[1].Value;
			string value = match.Groups[2].Value;
			_values[key] = value;
		}
		return regex.Replace(pgn, "");
	}

	private static string ParseMoves(string pgn)
	{
		Regex regex = new Regex(@"[()]|\{[^{}]*\}|([a-h][1-8][a-h][1-8][qrbn]?|[NBRQK]?[a-h]?[1-8]?x?[a-h][1-8](=[QRBN])?|O-O-O|O-O)");
		Regex timeRegex = new Regex(@"\[\s*%clk\s+(\d+):(\d+):(\d+)(\.(\d+))?\s*\]");
		Regex evalRegex = new Regex(@"\[\s*%eval\s+(M?-?\d+(\.\d+)?)\s*\]");
		Regex classRegex = new Regex(@"\[\s*%cls\s+(\d+)\s*\]");
		Stack<TreeNode> stack = new Stack<TreeNode>();
		foreach (Match match in regex.Matches(pgn))
		{
			string token = match.Value;
			if (token == "(")
			{
				TreeNode node = GameManager.GetGame().GetCurrentNode();
				if (node.Move != null)
				{
					stack.Push(node);
					GameManager.GetGame().UndoMove(node.Move.Value);
					continue;
				}
			}
			if (token == ")")
			{
				if (stack.Count > 0)
				{
					TreeNode node = stack.Pop();
					GameManager.GetGame().SetCurrentNode(node);
					continue;
				}
			}
			if (token.StartsWith('{') && token.EndsWith('}'))
			{
				TreeNode node = GameManager.GetGame().GetCurrentNode();
				do
				{
					Match timeMatch = timeRegex.Match(token);
					if (timeMatch.Success)
					{
						node.Time = 0;
						if (int.TryParse(timeMatch.Groups[1].Value, out int hours))
						{
							node.Time += hours * 3600000;
						}
						if (int.TryParse(timeMatch.Groups[2].Value, out int minutes))
						{
							node.Time += minutes * 60000;
						}
						if (int.TryParse(timeMatch.Groups[3].Value, out int seconds))
						{
							node.Time += seconds * 1000;
						}
						if (timeMatch.Groups[5].Success && int.TryParse(timeMatch.Groups[5].Value, out int milliseconds))
						{
							node.Time += milliseconds;
						}
						break;
					}
					Match evalMatch = evalRegex.Match(token);
					if (evalMatch.Success)
					{
						node.Eval = StringToEval(evalMatch.Groups[1].Value);
						break;
					}
					Match classMatch = classRegex.Match(token);
					if (classMatch.Success)
					{
						node.Class = int.Parse(classMatch.Groups[1].Value);
						break;
					}
					node.Comment = token.Substring(1, token.Length - 2).Trim();
				}
				while (false);
			}
			Move? move = null;
			ReadOnlySpan<Move> legalMoves = GameManager.GetLegalMoves();
			foreach (Move legalMove in legalMoves)
			{
				string uciMove = GameManager.GetGame().FormatMoveToUci(legalMove);
				if (uciMove.StartsWith(token))
				{
					move = legalMove;
					break;
				}
				string sanMove = GameManager.GetGame().FormatMoveToSan(legalMove);
				if (sanMove.StartsWith(token))
				{
					move = legalMove;
					break;
				}
			}
			if (move != null)
			{
				GameManager.GetGame().PlayMove(move.Value);
			}
		}
		return regex.Replace(pgn, "");
	}

	private static string EvalToString(int score)
	{
		return Scores.IsMateScore(score) ? $"M{Scores.ToMateDistance(score)}" : $"{score / 100m}";
	}

	private static int? StringToEval(string eval)
	{
		if (eval.StartsWith('M'))
		{
			if (int.TryParse(eval.Substring(1), out int result))
			{
				return Scores.ToMateScore(result);
			}
		}
		else
		{
			if (decimal.TryParse(eval, out decimal result))
			{
				return (int)(result * 100);
			}
		}
		return null;
	}

	private static string GetEnginePresetTag(int color)
	{
		return _colorNames[color] + "EnginePreset";
	}

	private static string GetEngineLimitTag(int color)
	{
		return _colorNames[color] + "EngineLimit";
	}

	static PgnManager()
	{
		_colorNames = new string[] { "White", "Black" };
		_entries = new List<string>() { "Event", "Site", "Date", "Round", "White", "Black", "Result" };
		_values = new Dictionary<string, string>();
		_dirty = false;
	}

	private static readonly string[] _colorNames;
	private static readonly List<string> _entries;
	private static Dictionary<string, string> _values;
	private static bool _dirty;
}

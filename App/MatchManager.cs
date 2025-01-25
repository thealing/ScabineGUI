namespace Scabine.App;

using Scabine.App.Menus;
using Scabine.App.Prefs;
using Scabine.Core;
using Scabine.Engines;
using Scabine.Scenes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using static Scabine.Core.Pieces;

internal static class MatchManager
{
	public static void Update()
	{
		double deltaTime = Time.GetTime() - _lastUpdateTime;
		_lastUpdateTime += deltaTime;
		TreeGame game = GameManager.GetGame();
		TreeNode lastNode = game.GetLastNode();
		_boardDisabled = game.IsFinished();
		if (_playing && !_paused)
		{
			int turn = lastNode.Color ^ 1;
			if (game.GetResult() != Result.Ongoing)
			{
				_finished = true;
			}
			else if (lastNode.Rank < _lastRank)
			{
				_finished = false;
			}
			if (_finished || Array.Exists(_times, time => time < 0))
			{
				if (!_wasFinished)
				{
					_wasFinished = true;
					SoundManager.StopAllSounds();
					SoundManager.EnqueueSound(Sounds.GameOver);
				}
				_boardDisabled = true;
				if (lastNode != game.GetCurrentNode())
				{
					ResetMatch();
				}
				else if (_times[White] < 0 ^ _times[Black] < 0)
				{
					int winner = Array.FindIndex(_times, time => time >= 0);
					string[] colorNames = { "White", "Black" };
					if (game.GetCurrentPosition().HasSufficientMaterial(winner))
					{
						lastNode.Comment = colorNames[winner] + " won on time.";
						PgnManager.SetWinner(winner);
					}
					else
					{
						lastNode.Comment = "Draw by timeout vs insufficient material.";
						PgnManager.SetResult(Result.Draw);
					}
				}
				else
				{
					Result result = game.GetResult();
					switch (result)
					{
						case Result.WhiteWon:
							lastNode.Comment = "White won by checkmate.";
							break;
						case Result.BlackWon:
							lastNode.Comment = "Black won by checkmate.";
							break;
						case Result.Draw:
							lastNode.Comment = "Draw.";
							Position position = game.GetCurrentPosition();
							if (position.IsDrawByFiftyMoveRule())
							{
								lastNode.Comment = "Draw by fifty-move rule.";
							}
							if (position.IsDrawByInsufficientMaterial())
							{
								lastNode.Comment = "Draw by insufficient material.";
							}
							if (game.IsDrawByRepetition())
							{
								lastNode.Comment = "Draw by repetition.";
							}
							if (game.IsStalemate())
							{
								lastNode.Comment = "Draw by stalemate.";
							}
							break;
					}
					PgnManager.SetResult(result);
				}
			}
			else
			{
				_wasFinished = false;
				PgnManager.SetResult(Result.Ongoing);
				if (lastNode != game.GetRootNode() && GameManager.IsDirty())
				{
					lastNode.Time = ConvertTime(_times[lastNode.Color]);
				}
				IEngine? engine = _engines[turn];
				ThinkingLimit? limit = _engineLimits[turn];
				bool updateTime = false;
				if (engine != null && limit != null)
				{
					if (_enginesThinking[turn] == false)
					{
						_enginesThinking[turn] = true;
						game.SetCurrentNode(lastNode);
						StartThinking(engine, limit);
					}
					else if (engine.IsThinking() == false)
					{
						_enginesThinking[turn] = false;
						string? engineMove = engine.GetPlayedMove();
						if (engineMove != null)
						{
							game.SetCurrentNode(lastNode);
							GameManager.TryPlayMove(engineMove);
						}
					}
					else
					{
						updateTime = true;
					}
				}
				else
				{
					updateTime = true;
				}
				if (lastNode.Rank < 1)
				{
					updateTime = false;
				}
				if (updateTime)
				{
					_times[turn] += _countingUp[turn] ? deltaTime : -deltaTime;
				}
			}
			if (lastNode.Rank > _lastRank)
			{
				_lastRank = lastNode.Rank;
				_times[lastNode.Color] += _increments[lastNode.Color];
			}
		}
	}

	public static bool StartPlayerMatch(PlayerMatchDefinition matchDefinition)
	{
		bool result = StartPlayerMatch(matchDefinition.PlayerUnlimited, matchDefinition.PlayerTime, matchDefinition.PlayerIncrement, matchDefinition.PlayerSide, matchDefinition.EngineInfo, matchDefinition.PresetName, matchDefinition.ThinkingLimit);
		if (result)
		{
			_lastPlayerMatch = matchDefinition;
			_lastEngineMatch = null;
			PgnManager.SetPlayerMatch(matchDefinition);
		}
		return result;
	}

	public static bool StartEngineMatch(EngineMatchDefinition matchDefinition)
	{
		bool result = StartEngineMatch(matchDefinition.EngineInfos, matchDefinition.PresetNames, matchDefinition.ThinkingLimits);
		if (result)
		{
			_lastPlayerMatch = null;
			_lastEngineMatch = matchDefinition;
			PgnManager.SetEngineMatch(matchDefinition);
		}
		return result;
	}

	public static void ResetMatch()
	{
		SoundManager.StopAllSounds();
		_playing = false;
		_paused = false;
		_finished = false;
		_wasFinished = false;
		_lastEngines.AddRange(_engines);
		Array.Clear(_engines);
		Array.Clear(_engineLimits);
	}

	public static bool HasMatch()
	{
		return _lastPlayerMatch != null || _lastEngineMatch != null;
	}

	public static bool RestartMatch()
	{
		if (_lastPlayerMatch != null)
		{
			return StartPlayerMatch(_lastPlayerMatch);
		}
		if (_lastEngineMatch != null)
		{
			return StartEngineMatch(_lastEngineMatch);
		}
		return false;
	}

	public static bool StartRematch()
	{
		bool result = false;
		if (_lastPlayerMatch != null)
		{
			_lastPlayerMatch.PlayerSide ^= 1;
			result = StartPlayerMatch(_lastPlayerMatch);
		}
		if (_lastEngineMatch != null)
		{
			Array.Reverse(_lastEngineMatch.EngineInfos);
			Array.Reverse(_lastEngineMatch.PresetNames);
			Array.Reverse(_lastEngineMatch.ThinkingLimits);
			result = StartEngineMatch(_lastEngineMatch);
		}
		if (result)
		{
			Board.Flipped ^= true;
		}
		return result;
	}

	public static void ClearMatch()
	{
		ResetMatch();
		_lastPlayerMatch = null;
		_lastEngineMatch = null;
	}

	public static void ReplaceEngine(IEngine oldEngine, IEngine newEngine)
	{
		for (int color = 0; color < ColorCount; color++)
		{
			if (_engines[color] == oldEngine)
			{
				_engines[color] = newEngine;
				_enginesThinking[color] = false;
			}
		}
	}

	public static void SetPaused(bool paused)
	{
		_paused = _playing && paused;
	}

	public static bool IsPlaying()
	{
		return _playing;
	}

	public static bool IsPaused()
	{
		return _paused;
	}

	public static bool IsFinished()
	{
		return _finished;
	}

	public static int GetWhiteClock()
	{
		return ConvertTime(_times[White]);
	}

	public static int GetBlackClock()
	{
		return ConvertTime(_times[Black]);
	}

	public static bool IsEngineThinking(IEngine engine)
	{
		return IsEnginePlayingItself(engine) || IsEnginePlaying(engine) && _enginesThinking[GetEngineSide(engine)];
	}

	public static bool IsEnginePlaying(IEngine engine)
	{
		return _engines.Contains(engine);
	}

	public static bool IsEnginePlayingItself(IEngine engine)
	{
		return _engines[White] == engine && _engines[Black] == engine;
	}

	public static int GetEngineSide(IEngine engine)
	{
		return Array.IndexOf(_engines, engine);
	}

	public static bool IsBoardDisabled()
	{
		return _boardDisabled;
	}

	private static bool StartPlayerMatch(bool playerUnlimited, int playerTime, int playerIncrement, int playerSide, EngineInfo engineInfo, string presetName, ThinkingLimit engineLimit)
	{
		if (!ConfigurePlayerMatch(playerUnlimited, playerTime, playerIncrement, playerSide, engineInfo, presetName, engineLimit))
		{
			return false;
		}
		if (_engines[playerSide ^ 1] != null)
		{
			_lastRank = 1;
			_engines[playerSide ^ 1]?.NewGame();
			GameManager.Restart();
		}
		return true;
	}

	private static bool ConfigurePlayerMatch(bool playerUnlimited, int playerTime, int playerIncrement, int playerSide, EngineInfo engineInfo, string presetName, ThinkingLimit engineLimit)
	{
		ResetMatch();
		IEngine? engine = EngineManager.GetOrStartEngine(engineInfo, presetName);
		if (engine == null)
		{
			return false;
		}
		if (playerUnlimited)
		{
			_times[playerSide] = 0;
			_increments[playerSide] = 0;
			_countingUp[playerSide] = true;
		}
		else
		{
			_times[playerSide] = playerTime;
			_increments[playerSide] = playerIncrement;
			_countingUp[playerSide] = false;
		}
		_engines[playerSide] = null;
		_engineLimits[playerSide] = null;
		int engineSide = playerSide ^ 1;
		if (engineLimit.Mode == ThinkingMode.GameTime)
		{
			_times[engineSide] = (double)engineLimit.BaseTime;
			_increments[engineSide] = (double)engineLimit.Increment;
			_countingUp[engineSide] = false;
		}
		else
		{
			_times[engineSide] = 0;
			_increments[engineSide] = 0;
			_countingUp[engineSide] = true;
		}
		_engines[engineSide] = engine;
		_engineLimits[engineSide] = engineLimit;
		_playing = true;
		_paused = false;
		StopLastEngines();
		Array.Clear(_enginesThinking);
		_lastUpdateTime = Time.GetTime();
		_lastRank = GameManager.GetGame().GetLastNode().Rank;
		return true;
	}

	private static bool StartEngineMatch(EngineInfo[] engineInfos, string[] presetNames, ThinkingLimit[] thinkingLimits)
	{
		if (!ConfigureEngineMatch(engineInfos, presetNames, thinkingLimits))
		{
			return false;
		}
		if (!_engines.Contains(null))
		{
			_lastRank = 1;
			for (int i = 0; i < 2; i++)
			{
				_engines[i]?.NewGame();
			}
			GameManager.Restart();
		}
		return true;
	}

	private static bool ConfigureEngineMatch(EngineInfo[] engineInfos, string[] presetNames, ThinkingLimit[] thinkingLimits)
	{
		ResetMatch();
		IEngine[] engines = new IEngine[2];
		for (int i = 0; i < 2; i++)
		{
			IEngine? engine = EngineManager.GetOrStartEngine(engineInfos[i], presetNames[i]);
			if (engine == null)
			{
				return false;
			}
			engines[i] = engine;
		}
		for (int i = 0; i < 2; i++)
		{
			if (thinkingLimits[i].Mode == ThinkingMode.GameTime)
			{
				_times[i] = (double)thinkingLimits[i].BaseTime;
				_increments[i] = (double)thinkingLimits[i].Increment;
				_countingUp[i] = false;
			}
			else
			{
				_times[i] = 0;
				_increments[i] = 0;
				_countingUp[i] = true;
			}
			_engines[i] = engines[i];
			_engineLimits[i] = thinkingLimits[i];
		}
		_playing = true;
		_paused = false;
		StopLastEngines();
		Array.Clear(_enginesThinking);
		_lastUpdateTime = Time.GetTime();
		return true;
	}

	private static void StopLastEngines()
	{
		foreach (IEngine? engine in _lastEngines)
		{
			if (engine != null && !_engines.Contains(engine))
			{
				EngineManager.StopEngine(engine);
			}
		}
		_lastEngines.Clear();
	}

	private static void StartThinking(IEngine engine, ThinkingLimit limit)
	{
		if (Engines.ResetBeforeEveryMove)
		{
			engine.NewGame();
		}
		UciGame game = GameManager.GetGame();
		engine.SetPosition(game.GetUciPosition(), game.GetUciMoves());
		switch (limit.Mode)
		{
			case ThinkingMode.GameTime:
				engine.StartThinking(0, 0, GetWhiteClock(), GetBlackClock(), ConvertTime(_increments[White]), ConvertTime(_increments[Black]));
				break;
			case ThinkingMode.MoveTime:
				engine.StartThinking(ConvertTime(limit.MoveTime));
				break;
			case ThinkingMode.FixedDepth:
				engine.StartThinking(limit.Depth, 0, 0, 0, 0, 0);
				break;
			case ThinkingMode.FixedNodes:
				engine.StartThinking(0, limit.Nodes, 0, 0, 0, 0);
				break;
		}
	}

	private static int ConvertTime(double time)
	{
		return (int)Math.Floor(time * 1000);
	}

	private static int ConvertTime(decimal time)
	{
		return (int)Math.Floor(time * 1000);
	}

	static MatchManager()
	{
		RuntimeHelpers.RunClassConstructor(typeof(EngineManager).TypeHandle);
		SaveManager.Save += () =>
		{
			SaveManager.Sync(nameof(_times), ref _times);
			SaveManager.Sync(nameof(_playing), ref _playing);
			SaveManager.Sync(nameof(_paused), ref _paused);
			SaveManager.Sync(nameof(_finished), ref _finished);
			SaveManager.Sync(nameof(_wasFinished), ref _wasFinished);
			SaveManager.Sync(nameof(_boardDisabled), ref _boardDisabled);
			SaveManager.Sync(nameof(_lastPlayerMatch), ref _lastPlayerMatch!);
			SaveManager.Sync(nameof(_lastEngineMatch), ref _lastEngineMatch!);
			if (_loadedMatches)
			{
				return;
			}
			_loadedMatches = true;
			if (_lastPlayerMatch != null || _lastEngineMatch != null)
			{
				IEnumerable<EngineInfo> engines = EngineManager.GetInstalledEngines();
				EngineInfo? getEngine(EngineInfo engine) => engines.FirstOrDefault(e => JsonSerializer.Serialize(e, SaveManager.Options) == JsonSerializer.Serialize(engine, SaveManager.Options));
				if (_lastPlayerMatch != null)
				{
					EngineInfo? engineInfo = getEngine(_lastPlayerMatch.EngineInfo);
					if (engineInfo != null)
					{
						_lastPlayerMatch.EngineInfo = engineInfo;
					}
					else
					{
						_lastPlayerMatch = null;
					}
				}
				if (_lastEngineMatch != null)
				{
					for (int color = 0; color < ColorCount; color++)
					{
						EngineInfo? engineInfo = getEngine(_lastEngineMatch.EngineInfos[color]);
						if (engineInfo != null)
						{
							_lastEngineMatch.EngineInfos[color] = engineInfo;
						}
						else
						{
							_lastEngineMatch = null;
							break;
						}
					}
				}
				if (GameManager.GetGame().GetResult() == Result.Ongoing && !Array.Exists(_times, time => time < 0) && (_lastPlayerMatch != null || _lastEngineMatch != null))
				{
					double[] times = new double[2];
					Array.Copy(_times, times, 2);
					if (_lastPlayerMatch != null)
					{
						ConfigurePlayerMatch(_lastPlayerMatch.PlayerUnlimited, _lastPlayerMatch.PlayerTime, _lastPlayerMatch.PlayerIncrement, _lastPlayerMatch.PlayerSide, _lastPlayerMatch.EngineInfo, _lastPlayerMatch.PresetName, _lastPlayerMatch.ThinkingLimit);
					}
					if (_lastEngineMatch != null)
					{
						ConfigureEngineMatch(_lastEngineMatch.EngineInfos, _lastEngineMatch.PresetNames, _lastEngineMatch.ThinkingLimits);
					}
					Array.Copy(times, _times, 2);
					_paused = true;
					GameMenu.UpdatePausedState();
				}
			}
		};
		
	}

	private static readonly double[] _increments = new double[2];
	private static readonly bool[] _countingUp = new bool[2];
	private static readonly IEngine?[] _engines = new IEngine[2];
	private static readonly List<IEngine?> _lastEngines = new List<IEngine?>();
	private static readonly ThinkingLimit?[] _engineLimits = new ThinkingLimit[2];
	private static readonly bool[] _enginesThinking = new bool[2];
	private static PlayerMatchDefinition? _lastPlayerMatch;
	private static EngineMatchDefinition? _lastEngineMatch;
	private static bool _loadedMatches;
	private static double[] _times = new double[2];
	private static bool _playing;
	private static bool _paused;
	private static bool _finished;
	private static bool _wasFinished;
	private static bool _boardDisabled;
	private static double _lastUpdateTime;
	private static int _lastRank;
}

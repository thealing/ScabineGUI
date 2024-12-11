namespace Scabine.Engines;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using Scabine.App;
using static Scabine.Core.Game;
using static Scabine.Core.Scores;

public sealed class ExternalEngine : AbstractEngine
{
	public ExternalEngine(EngineParams engineParams)
	{
		_queue = new ConcurrentQueue<string>();
		_options = new List<UciOption>();
		_name = engineParams.Name;
		_author = engineParams.Author;
		_thinking = false;
		_stopped = false;
		_bestDepth = 0;
		_bestMoves = new string[MaxDepth];
		_bestScores = new int[MaxDepth];
		_process = new Process();
		_process.StartInfo.FileName = engineParams.Path;
		_process.StartInfo.Arguments = engineParams.Arguments;
		_process.StartInfo.CreateNoWindow = true;
		_process.StartInfo.RedirectStandardInput = true;
		_process.StartInfo.RedirectStandardOutput = true;
		try
		{
			_process.Start();
			_process.BeginOutputReadLine();
			bool ready = false;
			_process.OutputDataReceived += (sender, e) =>
			{
				if (string.IsNullOrEmpty(e.Data))
				{
					return;
				}
				if (e.Data.Contains("readyok"))
				{
					ready = true;
				}
				_queue.Enqueue(e.Data);
			};
			SendCommand("uci");
			SendCommand("isready");
			foreach (string command in engineParams.Commands)
			{
				SendCommand(command);
			}
			DateTime startTime = DateTime.Now;
			TimeSpan timeout = TimeSpan.FromMilliseconds(1000);
			while (DateTime.Now - startTime < timeout)
			{
				if (ready)
				{
					_failed = false;
					return;
				}
			}
			_failed = true;
		}
		catch
		{
			_failed = true;
		}
	}

	public override void Dispose()
	{
		if (!IsRunning())
		{
			return;
		}
		_disposed = true;
		SendCommand("quit");
		if (!_process.WaitForExit(200))
		{
			_process.Kill();
		}
		_process.Dispose();
	}

	public override bool IsRunning()
	{
		return !_failed && !_disposed && !_process.HasExited;
	}

	public override bool IsThinking()
	{
		Update();
		return _thinking;
	}

	public override string GetName()
	{
		Update();
		return _name;
	}

	public override string GetAuthor()
	{
		Update();
		return _author;
	}

	public override void NewGame()
	{
		StopThinking();
		SendCommand("ucinewgame");
	}

	public override void SetPosition(string? position, string[] moves)
	{
		StopThinking();
		StringBuilder command = new StringBuilder("position ");
		if (position == null)
		{
			command.Append("startpos");
		}
		else
		{
			command.Append("fen ").Append(position);
		}
		if (moves.Length > 0)
		{
			command.Append(" moves ").Append(string.Join(' ', moves));
		}
		SendCommand(command.ToString());
	}

	public override void StartThinking(int moveTime)
	{
		StartThinking($"go movetime {moveTime}");
	}

	public override void StartThinking(int depthLimit, int nodeLimit, int whiteTimeLeft, int blackTimeLeft, int whiteIncrement, int blackIncrement)
	{
		StringBuilder command = new StringBuilder("go");
		if (depthLimit > 0)
		{
			command.Append($" depth {depthLimit}");
		}
		if (nodeLimit > 0)
		{
			command.Append($" nodes {nodeLimit}");
		}
		if (whiteTimeLeft > 0)
		{
			command.Append($" wtime {whiteTimeLeft}");
		}
		if (blackTimeLeft > 0)
		{
			command.Append($" btime {blackTimeLeft}");
		}
		if (whiteIncrement > 0)
		{
			command.Append($" winc {whiteIncrement}");
		}
		if (blackIncrement > 0)
		{
			command.Append($" binc {blackIncrement}");
		}
		if (command.ToString() == "go")
		{
			command.Append(" infinite");
		}
		StartThinking(command.ToString());
	}

	public override void StopThinking()
	{
		if (!_thinking)
		{
			return;
		}
		_thinking = false;
		_restarted = false;
		_stopped = true;
		SendCommand("stop");
	}

	public override void PauseThinking()
	{
		if (!IsRunning())
		{
			return;
		}
		if (_paused)
		{
			return;
		}
		_paused = true;
		NtSuspendProcess(_process.Handle);
	}

	public override void ResumeThinking()
	{
		if (!IsRunning())
		{
			return;
		}
		if (!_paused)
		{
			return;
		}
		_paused = false;
		NtResumeProcess(_process.Handle);
	}

	public override string? GetPlayedMove()
	{
		Update();
		return _playedMove;
	}

	public override int GetReachedDepth()
	{
		Update();
		return _bestDepth;
	}

	public override string GetBestMoves(int depth)
	{
		Update();
		return _bestMoves[depth].Trim();
	}

	public override int GetBestScore(int depth)
	{
		Update();
		return _bestScores[depth];
	}

	public override UciOption[] GetOptions()
	{
		return _options.ToArray();
	}

	public override void SetOption(UciOption option, object value)
	{
		string? valueString = option.FormatValue(value);
		if (valueString != null)
		{
			SendCommand($"setoption name {option.Name} value {valueString}");
		}
	}

	private void StartThinking(string command)
	{
		ResumeThinking();
		StopThinking();
		_thinking = true;
		_restarted = _stopped;
		_bestDepth = 0;
		Array.Fill(_bestMoves, "");
		Array.Fill(_bestScores, UnknownScore);
		_playedMove = null;
		SendCommand(command);
	}

	private void SendCommand(string command)
	{
		if (!IsRunning())
		{
			return;
		}
		// using WriteLine() occasionally freezes the application in RandomAccess.WriteAtOffset()
		_process.StandardInput.WriteLineAsync(command).Wait();
		_process.StandardInput.FlushAsync().Wait();
	}

	private bool GetResponse(out string? response)
	{
		return _queue.TryDequeue(out response);
	}

	private void Update()
	{
		while (GetResponse(out string? line) && line != null)
		{
			if (line.StartsWith("id"))
			{
				Match nameMatch = Regex.Match(line, @" name (.+)");
				if (nameMatch.Success && _name == "")
				{
					_name = nameMatch.Groups[1].Value;
				}
				Match authorMatch = Regex.Match(line, @" author (.+)");
				if (authorMatch.Success && _author == "")
				{
					_author = authorMatch.Groups[1].Value;
				}
				continue;
			}
			if (line.StartsWith("option"))
			{
				Match match = Regex.Match(line, @"option name\s+(([^\s]+\s+)*[^\s]+)\s+type\s+([^\s]+)");
				if (!match.Success)
				{
					return;
				}
				string name = match.Groups[1].Value;
				string type = match.Groups[3].Value;
				if (type == "check")
				{
					Match defaultMatch = Regex.Match(line, @"default\s+(true|false)");
					if (defaultMatch.Success)
					{
						string defaultValue = defaultMatch.Groups[1].Value;
						_options.Add(new CheckOption(name, bool.Parse(defaultValue)));
					}
				}
				if (type == "spin")
				{
					Match defaultMatch = Regex.Match(line, @"default\s+([+-]?\d+)");
					if (defaultMatch.Success)
					{
						string defaultValue = defaultMatch.Groups[1].Value;
						Match minMatch = Regex.Match(line, @"min\s+([+-]?\d+)");
						Match maxMatch = Regex.Match(line, @"max\s+([+-]?\d+)");
						int minValue = minMatch.Success ? int.Parse(minMatch.Groups[1].Value) : int.MinValue;
						int maxValue = maxMatch.Success ? int.Parse(maxMatch.Groups[1].Value) : int.MaxValue;
						_options.Add(new SpinOption(name, int.Parse(defaultValue), minValue, maxValue));
					}
				}
				if (type == "combo")
				{
					Match defaultMatch = Regex.Match(line, @"default\s+([^\s]+)");
					if (defaultMatch.Success)
					{
						string defaultValue = defaultMatch.Groups[1].Value;
						string[] parts = line.Split(new string[] { " var " }, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
						string[] values = parts[1..];
						_options.Add(new ComboOption(name, defaultValue, values));
					}
				}
				if (type == "string")
				{
					Match defaultMatch = Regex.Match(line, @"default\s+(([^\s]+\s+)*[^\s]+)");
					if (defaultMatch.Success)
					{
						string defaultValue = defaultMatch.Groups[1].Value;
						_options.Add(new StringOption(name, defaultValue));
					}
				}
			}
			if (line.StartsWith("bestmove"))
			{
				_thinking = _restarted;
				_stopped = false;
				_restarted = false;
				Match bestMoveMatch = Regex.Match(line, @"bestmove (\S+)");
				if (bestMoveMatch.Success)
				{
					_playedMove = bestMoveMatch.Groups[1].Value;
				}
				continue;
			}
			if (line.StartsWith("info") && !_restarted)
			{
				Match depthMatch = Regex.Match(line, @" depth (\d+)");
				Match moveMatch = Regex.Match(line, @" pv (.+)");
				if (depthMatch.Success && moveMatch.Success)
				{
					int depth = int.Parse(depthMatch.Groups[1].Value);
					if (depth < MaxDepth)
					{
						_bestDepth = Math.Max(_bestDepth, depth);
						_bestMoves[depth] = moveMatch.Groups[1].Value;
						Match cpMatch = Regex.Match(line, @" cp (-?\d+)");
						if (cpMatch.Success)
						{
							_bestScores[depth] = int.Parse(cpMatch.Groups[1].Value);
						}
						Match mateMatch = Regex.Match(line, @" mate (-?\d+)");
						if (mateMatch.Success)
						{
							int mateDistance = int.Parse(mateMatch.Groups[1].Value);
							_bestScores[depth] = Math.Sign(mateDistance) * MateScore - mateDistance;
						}
					}
				}
				continue;
			}
		}
	}

	private readonly ConcurrentQueue<string> _queue;
	private readonly List<UciOption> _options;
	private readonly Process _process;
	private readonly bool _failed;
	private readonly string[] _bestMoves;
	private readonly int[] _bestScores;
	private string _name;
	private string _author;
	private bool _thinking;
	private bool _stopped;
	private bool _restarted;
	private bool _paused;
	private bool _disposed;
	private string? _playedMove;
	private int _bestDepth;

	[DllImport("ntdll.dll", EntryPoint = "NtSuspendProcess", ExactSpelling = false)]
	private static extern UIntPtr NtSuspendProcess(IntPtr processHandle);

	[DllImport("ntdll.dll", EntryPoint = "NtResumeProcess", ExactSpelling = false)]
	private static extern UIntPtr NtResumeProcess(IntPtr processHandle);
}

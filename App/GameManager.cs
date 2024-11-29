namespace Scabine.App;

using Scabine.Core;
using System;
using static Scabine.Core.Pieces;
using static Scabine.Core.Game;
using System.Linq;
using System.Collections.Generic;
using System.Xml.Linq;
using Scabine.App.Prefs;

internal static class GameManager
{
	public static TreeGame GetGame()
	{
		return _game;
	}

	public static bool IsDirty()
	{
		return _wasDirty;
	}

	public static void Restart()
	{
		_game.Clear();
		_dirty = true;
	}

	public static void SetPosition(string fen)
	{
		_game.Clear();
		_game.SetFen(fen);
		_dirty = true;
	}

	public static ReadOnlySpan<Move> GetLegalMoves()
	{
		Move[] moves = new Move[MaxMoves];
		int moveCount = _game.GenerateMoves(moves);
		for (int i = 0; i < moveCount; i++)
		{
			if (!_game.TryPlayMove(moves[i]))
			{
				moveCount--;
				moves[i] = moves[moveCount];
				i--;
				continue;
			}
		}
		return moves.AsSpan(0, moveCount);
	}

	public static Move? TryPlayMove(ReadOnlySpan<char> move)
	{
		if (move.Length < 4)
		{
			return null;
		}
		foreach (Move legalMove in GetLegalMoves())
		{
			if (move.StartsWith(_game.FormatMoveToUci(legalMove)))
			{
				return PlayMove(legalMove);
			}
		}
		return null;
	}

	public static Move? TryPlayMove(int sourceSquare, int targetSquare, int promotion)
	{
		Move[] moves = new Move[MaxMoves];
		int moveCount = _game.GenerateMoves(moves);
		for (int i = 0; i < moveCount; i++)
		{
			if (!_game.TryPlayMove(moves[i]))
			{
				continue;
			}
			if (moves[i].SourceSquare == sourceSquare && moves[i].TargetSquare == targetSquare && moves[i].PromotionPiece == promotion)
			{
				return PlayMove(moves[i]);
			}
		}
		return null;
	}

	public static Move PlayMove(Move move)
	{
		_game.PlayMove(move);
		PlayMoveSound(move);
		return move;
	}

	public static void PlayMoveSound(Move move)
	{
		if (!General.PlaySounds)
		{
			return;
		}
		if (IsPiece(move.TargetPiece))
		{
			SoundManager.StopAllSounds();
			SoundManager.EnqueueSound(Sounds.Capture);
		}
		else
		{
			SoundManager.EnqueueSound(Sounds.Move);
		}
	}

	public static bool TryUndoMove()
	{
		TreeNode node = _game.GetCurrentNode();
		if (node.Move != null)
		{
			_game.UndoMove(node.Move.Value);
			return true;
		}
		else
		{
			return false;
		}
	}

	public static void StepForward(int amount)
	{
		while (amount > 0)
		{
			TreeNode node = _game.GetCurrentNode();
			if (node.Children.Count == 0)
			{
				break;
			}
			Move? move = node.Children[0].Move;
			if (move == null)
			{
				break;
			}
			if (!_game.PlayMove(move.Value))
			{
				break;
			}
			if (amount == 1)
			{
				PlayMoveSound(move.Value);
			}
			amount--;
		}
		if (amount != 0)
		{
			SoundManager.StopAllSounds();
		}
	}

	public static void StepBackward(int amount)
	{
		while (amount > 0)
		{
			if (!TryUndoMove())
			{
				break;
			}
			amount--;
		}
	}

	public static bool IsAtStart()
	{
		return _game.GetCurrentNode() == _game.GetRootNode();
	}

	public static bool IsAtEnd()
	{
		return _game.GetCurrentNode().Children.Count == 0;
	}

	public static void Update()
	{
		_wasDirty = _dirty;
		_dirty = false;
		SaveManager.Save += () =>
		{
			if (SaveManager.Sync("Game", ref _game))
			{
				_game.MovePlayed += OnMoveChanged;
				_game.MoveUndone += OnMoveChanged;
			}
		};
	}

	private static void OnMoveChanged(object? sender, Move move)
	{
		_dirty = true;
	}

	static GameManager()
	{
		_game = new TreeGame();
		_game.MovePlayed += OnMoveChanged;
		_game.MoveUndone += OnMoveChanged;
		_dirty = true;
		_wasDirty = true;
	}

	private static TreeGame _game;
	private static bool _dirty;
	private static bool _wasDirty;
}

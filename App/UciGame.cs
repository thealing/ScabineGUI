namespace Scabine.App;

using Scabine.Core;
using System;
using System.Collections.Generic;

internal class UciGame : Game
{
	public void Copy(UciGame other)
	{
		base.Copy(other);
		_uciMoves = new List<string>(other._uciMoves);
		_startPosition = other._startPosition;
	}

	public new void SetFen(ReadOnlySpan<char> fen)
	{
		base.SetFen(fen);
		fen = fen.Trim();
		_startPosition = fen.ToString();
		if (_startPosition == StartFen)
		{
			_startPosition = null;
		}
	}

	public new bool PlayMove(Move move)
	{
		string uciMove = FormatMoveToUci(move);
		if (base.PlayMove(move))
		{
			_uciMoves.Add(uciMove);
			return true;
		}
		else
		{
			return false;
		}
	}

	public new void UndoMove(Move move)
	{
		base.UndoMove(move);
		_uciMoves.RemoveAt(_uciMoves.Count - 1);
	}

	public string? GetUciPosition()
	{
		return _startPosition;
	}

	public string[] GetUciMoves()
	{
		return _uciMoves.ToArray();
	}

	private List<string> _uciMoves = new List<string>();
	private string? _startPosition = null;
}

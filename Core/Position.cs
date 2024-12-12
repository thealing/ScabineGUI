namespace Scabine.Core;

using System;
using static Pieces;
using static Squares;
using static Bitboards;
using static Bitmasks;
using static Magics;

public sealed class Position
{
	public const int CastlingWK = 1;
	public const int CastlingWQ = 2;
	public const int CastlingBK = 4;
	public const int CastlingBQ = 8;

	public static readonly ulong[][] PieceSquareHashes;

	public ulong[] Pieces = new ulong[PieceCount];
	public ulong[] Colors = new ulong[ColorCount];

	public int CurrentColor;
	public int CastlingPerms;
	public int EnPassantSquare;
	public int HalfmoveClock;

	public void Clone(Position other)
	{
		Array.Copy(other.Pieces, Pieces, PieceCount);
		Array.Copy(other.Colors, Colors, ColorCount);
		CurrentColor = other.CurrentColor;
		CastlingPerms = other.CastlingPerms;
		EnPassantSquare = other.EnPassantSquare;
		HalfmoveClock = other.HalfmoveClock;
	}

	public ulong GetHash()
	{
		return GetPiecesHash() ^ GetInfoHash();
	}

	public ulong GetPiecesHash()
	{
		ulong hash = 0;
		for (int piece = 1; piece < PieceCount; piece++)
		{
			ulong mask = Pieces[piece];
			while (mask != 0)
			{
				int square = PopSquare(ref mask);
				hash ^= PieceSquareHashes[piece][square];
			}
		}
		return hash;
	}

	public ulong GetInfoHash()
	{
		return (ulong)((long)(CurrentColor << 0 | CastlingPerms << 8 | EnPassantSquare << 16) | (long)HalfmoveClock << 32);
	}

	public ulong GetPawnMoves(int color)
	{
		return color == White ? GetWhitePawnsMoves() : GetBlackPawnsMoves();
	}

	public ulong GetPawnAttacks(int color)
	{
		return GetPawnLeftAttacks(color) | GetPawnRightAttacks(color);
	}

	public ulong GetPawnLeftAttacks(int color)
	{
		return color == White ? GetWhitePawnLeftAttacks() : GetBlackPawnLeftAttacks();
	}

	public ulong GetPawnRightAttacks(int color)
	{
		return color == White ? GetWhitePawnRightAttacks() : GetBlackPawnRightAttacks();
	}

	public ulong GetBishopMoves(int square)
	{
		return GetBishopMoveMask(square, ~Pieces[None]);
	}

	public ulong GetRookMoves(int square)
	{
		return GetRookMoveMask(square, ~Pieces[None]);
	}

	public ulong GetQueenMoves(int square)
	{
		return GetBishopMoves(square) | GetRookMoves(square);
	}

	public ulong GetKnightMoves(int square)
	{
		return KnightMoveMasks[square];
	}

	public ulong GetKingMoves(int square)
	{
		return KingMoveMasks[square];
	}

	public bool IsAttacked(int square, int color)
	{
		if ((GetPawnAttacks(color) & GetSquareMask(square)) != 0)
		{
			return true;
		}
		if ((GetKnightMoves(square) & Pieces[MakePiece(color, Knight)]) != 0)
		{
			return true;
		}
		if ((GetBishopMoves(square) & Pieces[MakePiece(color, Bishop)]) != 0)
		{
			return true;
		}
		if ((GetRookMoves(square) & Pieces[MakePiece(color, Rook)]) != 0)
		{
			return true;
		}
		if ((GetQueenMoves(square) & Pieces[MakePiece(color, Queen)]) != 0)
		{
			return true;
		}
		if ((GetKingMoves(square) & Pieces[MakePiece(color, King)]) != 0)
		{
			return true;
		}
		return false;
	}

	public bool IsInCheck()
	{
		return IsInCheck(CurrentColor);
	}

	public bool IsInCheck(int color)
	{
		return IsAttacked(GetKingPosition(color), color ^ 1);
	}

	public bool IsDraw()
	{
		return IsDrawByFiftyMoveRule() || IsDrawByInsufficientMaterial();
	}

	public bool IsDrawByFiftyMoveRule()
	{
		return HalfmoveClock >= 100;
	}

	public bool IsDrawByInsufficientMaterial()
	{
		return !HasSufficientMaterial(White) && !HasSufficientMaterial(Black);
	}

	public bool HasSufficientMaterial(int color)
	{
		return PopCount(Colors[color]) >= 3 || Pieces[MakePiece(color, Pawn)] != 0 || Pieces[MakePiece(color, Rook)] != 0 || Pieces[MakePiece(color, Queen)] != 0;
	}

	public int GetKingPosition()
	{
		return GetSquare(Pieces[MakePiece(CurrentColor, King)]);
	}

	public int GetKingPosition(int color)
	{
		return GetSquare(Pieces[MakePiece(color, King)]);
	}

	private ulong GetWhitePawnsMoves()
	{
		return Pieces[WhitePawn] >> 8;
	}

	private ulong GetBlackPawnsMoves()
	{
		return Pieces[BlackPawn] << 8;
	}

	private ulong GetWhitePawnLeftAttacks()
	{
		return Pieces[WhitePawn] >> 9 & ~GetFileMask(FileH);
	}

	private ulong GetWhitePawnRightAttacks()
	{
		return Pieces[WhitePawn] >> 7 & ~GetFileMask(FileA);
	}

	private ulong GetBlackPawnLeftAttacks()
	{
		return Pieces[BlackPawn] << 7 & ~GetFileMask(FileH);
	}

	private ulong GetBlackPawnRightAttacks()
	{
		return Pieces[BlackPawn] << 9 & ~GetFileMask(FileA);
	}

	static Position()
	{
		Random random = new Random();
		PieceSquareHashes = new ulong[PieceCount][];
		for (int piece = 0; piece < PieceCount; piece++)
		{
			PieceSquareHashes[piece] = new ulong[SquareCount];
			if (piece == None)
			{
				continue;
			}
			for (int square = 0; square < SquareCount; square++)
			{
				PieceSquareHashes[piece][square] = (ulong)random.NextInt64();
			}
		}
	}
}
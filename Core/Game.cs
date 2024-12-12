namespace Scabine.Core;

using System;
using System.Text;
using static Pieces;
using static Squares;
using static Bitboards;
using static Move;
using static Position;

public class Game
{
	public const int MaxPly = 1024;
	public const int MaxMoves = 256;
	public const int MaxDepth = 64;
	public const string StartFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

	public Game()
	{
		_board = new int[SquareCount];
		_ply = 0;
		_positionStack = new Position[MaxPly];
		_hashStack = new ulong[MaxPly];
		SetFen(StartFen);
	}

	public void Copy(Game other)
	{
		Array.Copy(other._board, _board, SquareCount);
		_ply = other._ply;
		for (int i = 0; i <= _ply; i++)
		{
			_positionStack[i] ??= new Position();
			_positionStack[i].Clone(other._positionStack[i]);
			_hashStack[i] = other._hashStack[i];
		}
	}

	public void SetFen(ReadOnlySpan<char> fen)
	{
		fen = fen.Trim();
		Array.Clear(_board);
		Position position = new Position();
		position.Pieces[None] = ulong.MaxValue;
		int fenIndex = 0;
		for (int square = 0; fen[fenIndex] != ' '; fenIndex++)
		{
			if (fen[fenIndex] == '/')
			{
				continue;
			}
			if (char.IsDigit(fen[fenIndex]))
			{
				square += fen[fenIndex] - '0';
			}
			else
			{
				if (square >= SquareCount)
				{
					throw new Exception("Too many squares in fen");
				}
				ClearSquare(ref position.Pieces[None], square);
				int piece = fen[fenIndex] switch
				{
					'P' => WhitePawn,
					'N' => WhiteKnight,
					'B' => WhiteBishop,
					'R' => WhiteRook,
					'Q' => WhiteQueen,
					'K' => WhiteKing,
					'p' => BlackPawn,
					'n' => BlackKnight,
					'b' => BlackBishop,
					'r' => BlackRook,
					'q' => BlackQueen,
					'k' => BlackKing,
					_ => throw new Exception("Invalid piece in fen")
				};
				SetSquare(ref position.Pieces[piece], square);
				_board[square] = piece;
				if (char.IsUpper(fen[fenIndex]))
				{
					SetSquare(ref position.Colors[White], square);
				}
				else
				{
					SetSquare(ref position.Colors[Black], square);
				}
				square++;
			}
		}
		fenIndex++;
		position.CurrentColor = fen[fenIndex] switch
		{
			'w' => White,
			'b' => Black,
			_ => throw new Exception("Invalid color in fen")
		};
		fenIndex++;
		fenIndex++;
		while (fen[fenIndex] != ' ')
		{
			switch (fen[fenIndex])
			{
				case '-':
					break;
				case 'K':
					position.CastlingPerms |= CastlingWK;
					break;
				case 'Q':
					position.CastlingPerms |= CastlingWQ;
					break;
				case 'k':
					position.CastlingPerms |= CastlingBK;
					break;
				case 'q':
					position.CastlingPerms |= CastlingBQ;
					break;
				default:
					throw new Exception("Invalid castling permission in fen");
			}
			fenIndex++;
		}
		fenIndex++;
		if (fen[fenIndex] != '-')
		{
			position.EnPassantSquare = ParseSquare(fen[fenIndex..]);
			fenIndex += 2;
		}
		else
		{
			position.EnPassantSquare = NoSquare;
			fenIndex += 1;
		}
		fenIndex++;
		position.HalfmoveClock = 0;
		while (char.IsDigit(fen[fenIndex]))
		{
			position.HalfmoveClock *= 10;
			position.HalfmoveClock += fen[fenIndex] - '0';
			fenIndex++;
		}
		ulong hash = position.GetHash();
		_ply = 0;
		_positionStack[0] = position;
		_hashStack[0] = hash;
	}

	public string GetFen()
	{
		StringBuilder builder = new StringBuilder();
		for (int rank = 0; rank < RankCount; rank++)
		{
			int counter = 0;
			for (int file = 0; file < FileCount; file++)
			{
				int square = MakeSquare(rank, file);
				if (IsPiece(_board[square]))
				{
					if (counter > 0)
					{
						builder.Append(counter);
						counter = 0;
					}
					builder.Append(GetPieceChar(_board[square]));
				}
				else
				{
					counter++;
				}
			}
			if (counter > 0)
			{
				builder.Append(counter);
			}
			if (rank + 1 != RankCount)
			{
				builder.Append('/');
			}
		}
		builder.Append(' ');
		Position position = GetCurrentPosition();
		builder.Append(GetColorChar(position.CurrentColor));
		builder.Append(' ');
		if (position.CastlingPerms == 0)
		{
			builder.Append('-');
		}
		else
		{
			if ((position.CastlingPerms & CastlingWK) != 0)
			{
				builder.Append('K');
			}
			if ((position.CastlingPerms & CastlingWQ) != 0)
			{
				builder.Append('Q');
			}
			if ((position.CastlingPerms & CastlingBK) != 0)
			{
				builder.Append('k');
			}
			if ((position.CastlingPerms & CastlingBQ) != 0)
			{
				builder.Append('q');
			}
		}
		builder.Append(' ');
		if (position.EnPassantSquare != NoSquare)
		{
			builder.Append(GetSquareFileChar(position.EnPassantSquare));
			builder.Append(GetSquareRankChar(position.EnPassantSquare));
		}
		else
		{
			builder.Append('-');
		}
		builder.Append(' ');
		builder.Append(position.HalfmoveClock);
		builder.Append(' ');
		builder.Append(_ply / 2 + 1);
		return builder.ToString();
	}

	public string FormatMoveToUci(Move move)
	{
		return move.ToString();
	}

	public string FormatMoveToSan(Move move)
	{
		if (move.Type == CastlingMove)
		{
			return GetSquareFile(move.TargetSquare) == FileG ? "O-O" : "O-O-O";
		}
		StringBuilder builder = new StringBuilder(8);
		int piece = GetPieceType(move.SourcePiece);
		if (piece != Pawn)
		{
			builder.Append(GetUpperTypeChar(piece));
		}
		Span<Move> moves = stackalloc Move[MaxMoves];
		int moveCount = GenerateMoves(moves);
		bool ambiguous = false;
		bool sameSourceFile = false;
		bool sameSourceRank = false;
		for (int i = 0; i < moveCount; i++)
		{
			if (moves[i].SourceSquare == move.SourceSquare)
			{
				continue;
			}
			if (moves[i].SourcePiece == move.SourcePiece && moves[i].TargetSquare == move.TargetSquare)
			{
				ambiguous = true;
				if (GetSquareFile(moves[i].SourceSquare) == GetSquareFile(move.SourceSquare))
				{
					sameSourceFile = true;
				}
				if (GetSquareRank(moves[i].SourceSquare) == GetSquareRank(move.SourceSquare))
				{
					sameSourceRank = true;
				}
			}
		}
		bool capture = move.TargetPiece != None || move.Type == EnPassantMove;
		bool needSourceFile = false;
		bool needSourceRank = false;
		if (piece == Pawn && capture)
		{
			needSourceFile = true;
		}
		else if (ambiguous)
		{
			if (sameSourceFile && sameSourceRank)
			{
				needSourceFile = true;
				needSourceRank = true;
			}
			else if (sameSourceFile)
			{
				needSourceRank = true;
			}
			else
			{
				needSourceFile = true;
			}
		}
		if (needSourceFile)
		{
			builder.Append(GetSquareFileChar(move.SourceSquare));
		}
		if (needSourceRank)
		{
			builder.Append(GetSquareRankChar(move.SourceSquare));
		}
		if (capture)
		{
			builder.Append('x');
		}
		builder.Append(GetSquareFileChar(move.TargetSquare));
		builder.Append(GetSquareRankChar(move.TargetSquare));
		if (move.IsPromotion)
		{
			builder.Append('=');
			builder.Append(GetUpperTypeChar(move.PromotionPiece));
		}
		if (PlayMove(move))
		{
			if (IsCheckmate())
			{
				builder.Append('#');
			}
			else if (IsCheck())
			{
				builder.Append('+');
			}
			UndoMove(move);
		}
		return builder.ToString();
	}

	public Move ParseMove(ReadOnlySpan<char> text)
	{
		Move move;
		move.SourceSquare = ParseSquare(text.Slice(0, 2));
		move.TargetSquare = ParseSquare(text.Slice(2, 2));
		move.SourcePiece = GetPiece(move.SourceSquare);
		move.TargetPiece = GetPiece(move.TargetSquare);
		move.Type = NormalMove;
		if (GetPieceType(move.SourcePiece) == Pawn)
		{
			if (text.Length >= 5 && char.IsLetter(text[4]))
			{
				move.Type = text[4] switch
				{
					'n' => PromotionMove + Knight,
					'b' => PromotionMove + Bishop,
					'r' => PromotionMove + Rook,
					'q' => PromotionMove + Queen,
					_ => throw new Exception("Invalid promotion piece")
				};
			}
			else if (move.TargetSquare == _positionStack[_ply].EnPassantSquare)
			{
				move.Type = EnPassantMove;
			}
			else if (move.TargetSquare == move.SourceSquare + Up * 2 || move.TargetSquare == move.SourceSquare + Down * 2)
			{
				move.Type = DoubleMove;
			}
		}
		else if (GetPieceType(move.SourcePiece) == King && Math.Abs(move.TargetSquare - move.SourceSquare) == 2)
		{
			move.Type = CastlingMove;
		}
		return move;
	}

	public int ParseSquare(ReadOnlySpan<char> text)
	{
		return MakeSquare('8' - text[1], text[0] - 'a');
	}

	public int[] GetBoard()
	{
		return _board;
	}

	public int GetPiece(int square)
	{
		return _board[square];
	}

	public int GetPly()
	{
		return _ply;
	}

	public Position GetCurrentPosition()
	{
		return _positionStack[_ply];
	}

	public Position GetStartingPosition()
	{
		return _positionStack[0];
	}

	public int GetCurrentColor()
	{
		return GetCurrentPosition().CurrentColor;
	}

	public int GetStartingColor()
	{
		return GetStartingPosition().CurrentColor;
	}

	public int GenerateMoves(Span<Move> moves, bool criticalOnly = false)
	{
		int moveCount = 0;
		ulong targetMask;
		Position position = _positionStack[_ply];
		// Pawn
		int pawnPromotionRank = GetPromotionRank(position.CurrentColor);
		int pawnMoveDirection = GetMoveDirection(position.CurrentColor);
		ulong pawnPromotionRankMask = GetRankMask(pawnPromotionRank);
		ulong pawnMoveMask = position.GetPawnMoves(position.CurrentColor) & position.Pieces[None];
		if (!criticalOnly)
		{
			// Pawn single move
			targetMask = pawnMoveMask & ~pawnPromotionRankMask;
			while (targetMask != 0)
			{
				int targetSquare = PopSquare(ref targetMask);
				int sourceSquare = targetSquare - pawnMoveDirection;
				moves[moveCount++] = new Move(sourceSquare, targetSquare, MakePiece(position.CurrentColor, Pawn), None, NormalMove);
			}
			// Pawn double move
			if (position.CurrentColor == White)
			{
				targetMask = position.Pieces[WhitePawn];
				targetMask &= GetRankMask(Rank2);
				targetMask >>= 8;
				targetMask &= position.Pieces[None];
				targetMask >>= 8;
				targetMask &= position.Pieces[None];
			}
			else
			{
				targetMask = position.Pieces[BlackPawn];
				targetMask &= GetRankMask(Rank7);
				targetMask <<= 8;
				targetMask &= position.Pieces[None];
				targetMask <<= 8;
				targetMask &= position.Pieces[None];
			}
			while (targetMask != 0)
			{
				int targetSquare = PopSquare(ref targetMask);
				int sourceSquare = targetSquare - pawnMoveDirection * 2;
				moves[moveCount++] = new Move(sourceSquare, targetSquare, MakePiece(position.CurrentColor, Pawn), None, DoubleMove);
			}
		}
		// Pawn promotion move
		targetMask = pawnMoveMask & pawnPromotionRankMask;
		while (targetMask != 0)
		{
			int targetSquare = PopSquare(ref targetMask);
			int sourceSquare = targetSquare - pawnMoveDirection;
			for (int piece = Queen; piece >= Knight; piece--)
			{
				moves[moveCount++] = new Move(sourceSquare, targetSquare, MakePiece(position.CurrentColor, Pawn), None, PromotionMove + piece);
			}
		}
		// Pawn capture
		ulong pawnLeftCaptureMask = position.GetPawnLeftAttacks(position.CurrentColor) & position.Colors[position.CurrentColor ^ 1];
		ulong pawnRightCaptureMask = position.GetPawnRightAttacks(position.CurrentColor) & position.Colors[position.CurrentColor ^ 1];
		targetMask = pawnLeftCaptureMask & ~pawnPromotionRankMask;
		while (targetMask != 0)
		{
			int targetSquare = PopSquare(ref targetMask);
			int sourceSquare = targetSquare - (pawnMoveDirection + Left);
			moves[moveCount++] = new Move(sourceSquare, targetSquare, MakePiece(position.CurrentColor, Pawn), GetPiece(targetSquare), NormalMove);
		}
		targetMask = pawnRightCaptureMask & ~pawnPromotionRankMask;
		while (targetMask != 0)
		{
			int targetSquare = PopSquare(ref targetMask);
			int sourceSquare = targetSquare - (pawnMoveDirection + Right);
			moves[moveCount++] = new Move(sourceSquare, targetSquare, MakePiece(position.CurrentColor, Pawn), GetPiece(targetSquare), NormalMove);
		}
		// Pawn promotion capture
		targetMask = pawnLeftCaptureMask & pawnPromotionRankMask;
		while (targetMask != 0)
		{
			int targetSquare = PopSquare(ref targetMask);
			int sourceSquare = targetSquare - (pawnMoveDirection + Left);
			for (int piece = Queen; piece >= Knight; piece--)
			{
				moves[moveCount++] = new Move(sourceSquare, targetSquare, MakePiece(position.CurrentColor, Pawn), GetPiece(targetSquare), PromotionMove + piece);
			}
		}
		targetMask = pawnRightCaptureMask & pawnPromotionRankMask;
		while (targetMask != 0)
		{
			int targetSquare = PopSquare(ref targetMask);
			int sourceSquare = targetSquare - (pawnMoveDirection + Right);
			for (int piece = Queen; piece >= Knight; piece--)
			{
				moves[moveCount++] = new Move(sourceSquare, targetSquare, MakePiece(position.CurrentColor, Pawn), GetPiece(targetSquare), PromotionMove + piece);
			}
		}
		// En-passant
		if (position.EnPassantSquare != NoSquare)
		{
			if (GetSquareFile(position.EnPassantSquare) != FileH)
			{
				int sourceSquare = position.EnPassantSquare - (pawnMoveDirection + Left);
				if (TestSquare(position.Pieces[MakePiece(position.CurrentColor, Pawn)], sourceSquare))
				{
					moves[moveCount++] = new Move(sourceSquare, position.EnPassantSquare, MakePiece(position.CurrentColor, Pawn), None, EnPassantMove);
				}
			}
			if (GetSquareFile(position.EnPassantSquare) != FileA)
			{
				int sourceSquare = position.EnPassantSquare - (pawnMoveDirection + Right);
				if (TestSquare(position.Pieces[MakePiece(position.CurrentColor, Pawn)], sourceSquare))
				{
					moves[moveCount++] = new Move(sourceSquare, position.EnPassantSquare, MakePiece(position.CurrentColor, Pawn), None, EnPassantMove);
				}
			}
		}
		// Non-pawn pieces
		for (int piece = MakePiece(position.CurrentColor, Knight), lastPiece = MakePiece(position.CurrentColor, King); piece <= lastPiece; piece++)
		{
			Func<int, ulong> GetMoves = GetPieceType(piece) switch
			{
				Knight => position.GetKnightMoves,
				Bishop => position.GetBishopMoves,
				Rook => position.GetRookMoves,
				Queen => position.GetQueenMoves,
				King => position.GetKingMoves,
				_ => throw new Exception("Invalid piece type")
			};
			ulong sourceMask = position.Pieces[piece];
			while (sourceMask != 0)
			{
				int sourceSquare = PopSquare(ref sourceMask);
				targetMask = GetMoves(sourceSquare) & ~position.Colors[position.CurrentColor];
				if (criticalOnly)
				{
					targetMask &= position.Colors[position.CurrentColor ^ 1];
				}
				while (targetMask != 0)
				{
					int targetSquare = PopSquare(ref targetMask);
					moves[moveCount++] = new Move(sourceSquare, targetSquare, piece, GetPiece(targetSquare), NormalMove);
				}
			}
		}
		if (!criticalOnly)
		{
			// Castling
			if (!position.IsInCheck())
			{
				if (position.CurrentColor == White)
				{
					if ((position.CastlingPerms & CastlingWK) != 0 && TestSquare(position.Pieces[None], F1) && TestSquare(position.Pieces[None], G1) && !position.IsAttacked(F1, Black))
					{
						moves[moveCount++] = new Move(E1, G1, MakePiece(position.CurrentColor, King), None, CastlingMove);
					}
					if ((position.CastlingPerms & CastlingWQ) != 0 && TestSquare(position.Pieces[None], D1) && TestSquare(position.Pieces[None], C1) && TestSquare(position.Pieces[None], B1) && !position.IsAttacked(D1, Black))
					{
						moves[moveCount++] = new Move(E1, C1, MakePiece(position.CurrentColor, King), None, CastlingMove);
					}
				}
				else
				{
					if ((position.CastlingPerms & CastlingBK) != 0 && TestSquare(position.Pieces[None], F8) && TestSquare(position.Pieces[None], G8) && !position.IsAttacked(F8, White))
					{
						moves[moveCount++] = new Move(E8, G8, MakePiece(position.CurrentColor, King), None, CastlingMove);
					}
					if ((position.CastlingPerms & CastlingBQ) != 0 && TestSquare(position.Pieces[None], D8) && TestSquare(position.Pieces[None], C8) && TestSquare(position.Pieces[None], B8) && !position.IsAttacked(D8, White))
					{
						moves[moveCount++] = new Move(E8, C8, MakePiece(position.CurrentColor, King), None, CastlingMove);
					}
				}
			}
		}
		return moveCount;
	}

	public bool PlayMove(Move move)
	{
		if (_ply + 1 == MaxPly)
		{
			return false;
		}
		Position position = _positionStack[_ply + 1] ?? new Position();
		position.Clone(_positionStack[_ply]);
		ulong hash = _hashStack[_ply];
		_ply++;
		hash ^= position.GetInfoHash();
		hash ^= PieceSquareHashes[move.SourcePiece][move.SourceSquare];
		hash ^= PieceSquareHashes[move.TargetPiece][move.TargetSquare];
		ulong moveMask = GetSquareMask(move.SourceSquare) | GetSquareMask(move.TargetSquare);
		position.Pieces[move.SourcePiece] ^= moveMask;
		position.Colors[position.CurrentColor] ^= moveMask;
		SetSquare(ref position.Pieces[None], move.SourceSquare);
		ClearSquare(ref position.Pieces[move.TargetPiece], move.TargetSquare);
		if (IsPiece(move.TargetPiece))
		{
			ClearSquare(ref position.Colors[position.CurrentColor ^ 1], move.TargetSquare);
		}
		if (IsPiece(move.TargetPiece) || GetPieceType(move.SourcePiece) == Pawn)
		{
			position.HalfmoveClock = 0;
		}
		else
		{
			position.HalfmoveClock++;
		}
		position.EnPassantSquare = NoSquare;
		int finalPiece = move.SourcePiece;
		switch (move.Type)
		{
			case NormalMove:
				break;
			case DoubleMove:
				position.EnPassantSquare = move.TargetSquare - GetMoveDirection(position.CurrentColor);
				break;
			case EnPassantMove:
				int capturedSquare = move.TargetSquare - GetMoveDirection(position.CurrentColor);
				int capturedPiece = MakePiece(position.CurrentColor ^ 1, Pawn);
				SetSquare(ref position.Pieces[None], capturedSquare);
				ClearSquare(ref position.Pieces[capturedPiece], capturedSquare);
				ClearSquare(ref position.Colors[position.CurrentColor ^ 1], capturedSquare);
				_board[capturedSquare] = None;
				hash ^= PieceSquareHashes[capturedPiece][capturedSquare];
				break;
			case CastlingMove:
				int rookSourceSquare;
				int rookTargetSquare;
				switch (move.TargetSquare)
				{
					case G1:
						rookSourceSquare = H1;
						rookTargetSquare = F1;
						break;
					case C1:
						rookSourceSquare = A1;
						rookTargetSquare = D1;
						break;
					case G8:
						rookSourceSquare = H8;
						rookTargetSquare = F8;
						break;
					case C8:
						rookSourceSquare = A8;
						rookTargetSquare = D8;
						break;
					default:
						throw new Exception("Invalid castling square");
				}
				ulong rookMoveMask = GetSquareMask(rookSourceSquare) | GetSquareMask(rookTargetSquare);
				int rookPiece = MakePiece(position.CurrentColor, Rook);
				position.Pieces[None] ^= rookMoveMask;
				position.Pieces[rookPiece] ^= rookMoveMask;
				position.Colors[position.CurrentColor] ^= rookMoveMask;
				hash ^= PieceSquareHashes[rookPiece][rookSourceSquare];
				hash ^= PieceSquareHashes[rookPiece][rookTargetSquare];
				_board[rookSourceSquare] = None;
				_board[rookTargetSquare] = rookPiece;
				break;
			default:
				ClearSquare(ref position.Pieces[finalPiece], move.TargetSquare);
				finalPiece ^= Pawn ^ (move.Type - PromotionMove);
				SetSquare(ref position.Pieces[finalPiece], move.TargetSquare);
				break;
		}
		_board[move.SourceSquare] = None;
		_board[move.TargetSquare] = finalPiece;
		if (position.IsInCheck())
		{
			UndoMove(move);
			return false;
		}
		if ((position.CastlingPerms & CastlingWK) != 0 && (moveMask & (GetSquareMask(E1) | GetSquareMask(H1))) != 0)
		{
			position.CastlingPerms &= ~CastlingWK;
		}
		if ((position.CastlingPerms & CastlingWQ) != 0 && (moveMask & (GetSquareMask(E1) | GetSquareMask(A1))) != 0)
		{
			position.CastlingPerms &= ~CastlingWQ;
		}
		if ((position.CastlingPerms & CastlingBK) != 0 && (moveMask & (GetSquareMask(E8) | GetSquareMask(H8))) != 0)
		{
			position.CastlingPerms &= ~CastlingBK;
		}
		if ((position.CastlingPerms & CastlingBQ) != 0 && (moveMask & (GetSquareMask(E8) | GetSquareMask(A8))) != 0)
		{
			position.CastlingPerms &= ~CastlingBQ;
		}
		position.CurrentColor ^= 1;
		hash ^= PieceSquareHashes[finalPiece][move.TargetSquare];
		hash ^= position.GetInfoHash();
		_positionStack[_ply] = position;
		_hashStack[_ply] = hash;
		return true;
	}

	public void UndoMove(Move move)
	{
		_ply--;
		int currentColor = _positionStack[_ply].CurrentColor;
		switch (move.Type)
		{
			case NormalMove:
				break;
			case DoubleMove:
				break;
			case EnPassantMove:
				int capturedSquare = move.TargetSquare - GetMoveDirection(currentColor);
				int capturedPiece = MakePiece(currentColor ^ 1, Pawn);
				_board[capturedSquare] = capturedPiece;
				break;
			case CastlingMove:
				int rookSourceSquare;
				int rookTargetSquare;
				switch (move.TargetSquare)
				{
					case G1:
						rookSourceSquare = H1;
						rookTargetSquare = F1;
						break;
					case C1:
						rookSourceSquare = A1;
						rookTargetSquare = D1;
						break;
					case G8:
						rookSourceSquare = H8;
						rookTargetSquare = F8;
						break;
					case C8:
						rookSourceSquare = A8;
						rookTargetSquare = D8;
						break;
					default:
						throw new Exception("Invalid castling square");
				}
				int rookPiece = MakePiece(currentColor, Rook);
				_board[rookSourceSquare] = rookPiece;
				_board[rookTargetSquare] = None;
				break;
		}
		_board[move.SourceSquare] = move.SourcePiece;
		_board[move.TargetSquare] = move.TargetPiece;
	}

	public bool IsCheck()
	{
		return GetCurrentPosition().IsInCheck();
	}

	public bool IsCheckmate()
	{
		return IsCheck() && IsFinished();
	}

	public bool IsStalemate()
	{
		return !IsCheck() && IsFinished();
	}

	public bool IsFinished()
	{
		Span<Move> moves = stackalloc Move[MaxMoves];
		int moveCount = GenerateMoves(moves);
		for (int i = 0; i < moveCount; i++)
		{
			if (!PlayMove(moves[i]))
			{
				continue;
			}
			UndoMove(moves[i]);
			return false;
		}
		return true;
	}

	public Result GetResult()
	{
		if (IsFinished())
		{
			if (IsCheck())
			{
				if (GetCurrentColor() == Black)
				{
					return Result.WhiteWon;
				}
				if (GetCurrentColor() == White)
				{
					return Result.BlackWon;
				}
			}
			else
			{
				return Result.Draw;
			}
		}
		if (GetCurrentPosition().IsDraw())
		{
			return Result.Draw;
		}
		return IsDrawByRepetition() ? Result.Draw : Result.Ongoing;
	}

	public bool IsDrawByRepetition()
	{
		int repetitionCount = 0;
		for (int i = 0; i <= _ply; i++)
		{
			if (_hashStack[i] == _hashStack[_ply])
			{
				repetitionCount++;
			}
		}
		return repetitionCount >= 3;
	}

	public long Perft(int depth)
	{
		if (depth == 0)
		{
			return 1;
		}
		long result = 0;
		Span<Move> moves = stackalloc Move[MaxMoves];
		int moveCount = GenerateMoves(moves);
		for (int i = 0; i < moveCount; i++)
		{
			if (!PlayMove(moves[i]))
			{
				continue;
			}
			result += Perft(depth - 1);
			UndoMove(moves[i]);
		}
		return result;
	}

	private int[] _board;
	private int _ply;
	private Position[] _positionStack;
	private ulong[] _hashStack;
}

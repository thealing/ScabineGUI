namespace Scabine.Core;

public static class Pieces
{
	public const int None = 0;

	public const int Pawn = 1;
	public const int Knight = 2;
	public const int Bishop = 3;
	public const int Rook = 4;
	public const int Queen = 5;
	public const int King = 6;

	public const int White = 0;
	public const int Black = 1;

	public const int TypeCount = 8;
	public const int ColorCount = 2;
	public const int PieceCount = 16;

	public static readonly int WhitePawn = MakePiece(White, Pawn);
	public static readonly int WhiteKnight = MakePiece(White, Knight);
	public static readonly int WhiteBishop = MakePiece(White, Bishop);
	public static readonly int WhiteRook = MakePiece(White, Rook);
	public static readonly int WhiteQueen = MakePiece(White, Queen);
	public static readonly int WhiteKing = MakePiece(White, King);

	public static readonly int BlackPawn = MakePiece(Black, Pawn);
	public static readonly int BlackKnight = MakePiece(Black, Knight);
	public static readonly int BlackBishop = MakePiece(Black, Bishop);
	public static readonly int BlackRook = MakePiece(Black, Rook);
	public static readonly int BlackQueen = MakePiece(Black, Queen);
	public static readonly int BlackKing = MakePiece(Black, King);

	public static int MakePiece(int color, int type)
	{
		return color * 8 + type;
	}

	public static bool IsPiece(int piece)
	{
		return piece != None;
	}

	public static int GetPieceType(int piece)
	{
		return piece % 8;
	}

	public static int GetPieceColor(int piece)
	{
		return piece / 8;
	}

	public static char GetLowerTypeChar(int type)
	{
		return _lowerTypeChars[type];
	}

	public static char GetUpperTypeChar(int type)
	{
		return _upperTypeChars[type];
	}

	public static char GetColorChar(int color)
	{
		return _colorChars[color];
	}

	public static char GetPieceChar(int piece)
	{
		return _pieceChars[piece];
	}

	static Pieces()
	{
		_lowerTypeChars = new char[TypeCount];
		_upperTypeChars = new char[TypeCount];
		_pieceChars = new char[PieceCount];
		char[] typeChars = new char[] { ' ', 'p', 'n', 'b', 'r', 'q', 'k' };
		for (int type = None; type <= King; type++)
		{
			_lowerTypeChars[type] = char.ToLower(typeChars[type]);
			_upperTypeChars[type] = char.ToUpper(typeChars[type]);
			_pieceChars[MakePiece(White, type)] = _upperTypeChars[type];
			_pieceChars[MakePiece(Black, type)] = _lowerTypeChars[type];
		}
		_colorChars = new char[] { 'w', 'b' };
	}

	private static readonly char[] _lowerTypeChars;
	private static readonly char[] _upperTypeChars;
	private static readonly char[] _colorChars;
	private static readonly char[] _pieceChars;
}
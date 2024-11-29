namespace Scabine.App.Prefs;

using Scabine.Resources;
using System;
using System.Linq;

internal static class Themes
{
	public static Tuple<string, string[]> Pieces;
	public static Tuple<string, string[]> Board;

	static Themes()
	{
		Pieces = Tuple.Create("Classic", _pieceThemes);
		Board = Tuple.Create("Brown", _boardThemes);
		SaveManager.Save += () =>
		{
			SaveManager.Sync(nameof(Pieces), ref Pieces);
			SaveManager.Sync(nameof(Board), ref Board);
			Pieces = Tuple.Create(Pieces.Item1, _pieceThemes);
			Board = Tuple.Create(Board.Item1, _boardThemes);
		};
	}

	private static readonly string[] _pieceThemes = ResourceManager.EnumerateSubfolders("Pieces").ToArray();
	private static readonly string[] _boardThemes = ResourceManager.EnumerateSubfolders("Boards").ToArray();
}

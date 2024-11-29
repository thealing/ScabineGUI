namespace Scabine.App;

using System.Drawing;
using System.Drawing.Drawing2D;
using Scabine.Scenes;
using static Scabine.Core.Pieces;
using static Scabine.App.GraphicsHelper;
using static Resources.ResourceManager;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Scabine.App.Prefs;

internal static class PieceImages
{
	public static Image? GetScaledImage(int piece)
	{
		if (_theme != Themes.Pieces.Item1)
		{
			Update();
		}
		return _scaledImages[piece];
	}

	public static void SetScaledSize(int size)
	{
		if (size != _scaledSize)
		{
			_scaledSize = size;
			Update();
		}
	}

	private static void Update()
	{
		_theme = Themes.Pieces.Item1;
		for (int piece = 0; piece < PieceCount; piece++)
		{
			Image? image = _stockImages[_theme][piece];
			if (image != null)
			{
				_scaledImages[piece] = ResizeImage(image, _scaledSize, _scaledSize);
			}
		}
	}

	static PieceImages()
	{
		_stockImages = new OrderedDictionary<string, Image?[]>();
		_scaledImages = new Image[PieceCount];
		foreach (string theme in Themes.Pieces.Item2)
		{
			_stockImages[theme] = new Image[PieceCount];
			for (int color = White; color <= Black; color++)
			{
				for (int type = Pawn; type <= King; type++)
				{
					_stockImages[theme][MakePiece(color, type)] = LoadImageResource($"Pieces.{theme}.{GetColorChar(color)}{GetLowerTypeChar(type)}.png");
				}
			}
		}
	}

	private static readonly OrderedDictionary<string, Image?[]> _stockImages;
	private static readonly Image?[] _scaledImages;
	private static int _scaledSize = 0;
	private static string _theme = "";
}
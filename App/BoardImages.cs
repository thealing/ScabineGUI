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
using System.Security.Policy;

internal static class BoardImages
{
	public static Image? GetScaledImage()
	{
		if (_theme != Themes.Board.Item1)
		{
			Update();
		}
		return _scaledImage;
	}

	public static void SetScaledSize(int size)
	{
		if (size != _scaledSize)
		{
			_scaledSize = size;
			Update();
		}
		_scaledSize = size;
	}

	private static void Update()
	{
		_theme = Themes.Board.Item1;
		Image? image = _stockImages[_theme];
		if (image != null)
		{
			_scaledImage = ResizeImage(image, _scaledSize, _scaledSize);
		}
	}

	static BoardImages()
	{
		_stockImages = new OrderedDictionary<string, Image>();
		foreach (string theme in Themes.Board.Item2)
		{
			_stockImages[theme] = LoadImageResource($"Boards.{theme}.Board.png");
		}
	}

	private static readonly OrderedDictionary<string, Image> _stockImages;
	private static Image? _scaledImage;
	private static int _scaledSize = 0;
	private static string _theme = "";
}
namespace ChessBand.Application;

using System.Drawing;
using ChessBand.Application.Settings;
using static Resources.ResourceManager;
using static ChessBand.Application.GraphicsHelper;

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
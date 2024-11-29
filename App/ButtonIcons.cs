namespace Scabine.App;

using System.Drawing;
using static Resources.ResourceManager;

internal static class ButtonIcons
{
	public static readonly Image Forward = LoadImageResource("Buttons.Forward.png");
	public static readonly Image Backward = LoadImageResource("Buttons.Backward.png");
	public static readonly Image ToStart = LoadImageResource("Buttons.ToStart.png");
	public static readonly Image ToEnd = LoadImageResource("Buttons.ToEnd.png");
	public static readonly Image Play = LoadImageResource("Buttons.Play.png");
	public static readonly Image Pause = LoadImageResource("Buttons.Pause.png");
	public static readonly Image MovePieces = LoadImageResource("Buttons.Move.png");
	public static readonly Image ErasePieces = LoadImageResource("Buttons.Erase.png");
}

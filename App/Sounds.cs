namespace Scabine.App;

using System.Media;
using static Resources.ResourceManager;

internal static class Sounds
{
	public static readonly SoundPlayer Move = LoadSoundResource("Sounds.Move.wav");
	public static readonly SoundPlayer Capture = LoadSoundResource("Sounds.Capture.wav");
	public static readonly SoundPlayer GameOver = LoadSoundResource("Sounds.GameOver.wav");
}

namespace Scabine.App;

using Scabine.App.Prefs;
using System.Collections.Concurrent;
using System.Media;
using System.Threading;
using System.Threading.Tasks;

internal static class SoundManager
{
	public static void EnqueueSound(SoundPlayer sound)
	{
		if (!General.PlaySounds)
		{
			return;
		}
		_queue.Enqueue(sound);
		_signal.Set();
	}

	public static void StopAllSounds()
	{
		_queue.Clear();
	}

	private static void PlaySounds()
	{
		while (true)
		{
			_signal.WaitOne();
			if (_queue.TryDequeue(out SoundPlayer? sound) && sound != null)
			{
				sound.PlaySync();
			}
		}
	}

	static SoundManager()
	{
		Task.Run(PlaySounds);
	}

	private static readonly ConcurrentQueue<SoundPlayer> _queue = new ConcurrentQueue<SoundPlayer>();
	private static readonly AutoResetEvent _signal = new AutoResetEvent(false);
}

namespace Scabine.Engines;

using System;

public interface IEngine : IDisposable
{
	bool IsRunning();

	string GetName();

	string GetAuthor();

	UciOption[] GetOptions();

	void SetOption(UciOption option, object value);

	void NewGame();

	void SetPosition(string? position, string[] moves);

	void StartThinking(int moveTime);

	void StartThinking(int depthLimit, int nodeLimit, int whiteTimeLeft, int blackTimeLeft, int whiteIncrement, int blackIncrement);

	void StartThinking();

	void StopThinking();

	void PauseThinking();

	void ResumeThinking();

	bool IsThinking();

	public string? GetPlayedMove();

	public int GetReachedDepth();

	public string GetBestMoves(int depth);

	public string GetBestMoves();

	public int GetBestScore(int depth);

	public int GetBestScore();
}

namespace Scabine.Engines;

public abstract class AbstractEngine : IEngine
{
	public void StartThinking()
	{
		StartThinking(0, 0, 0, 0, 0, 0);
	}

	public string GetBestMoves()
	{
		return GetBestMoves(GetReachedDepth());
	}

	public int GetBestScore()
	{
		return GetBestScore(GetReachedDepth());
	}

	public abstract void Dispose();
	
	public abstract string GetAuthor();
	
	public abstract string GetBestMoves(int depth);
	
	public abstract int GetBestScore(int depth);
	
	public abstract string GetName();
	
	public abstract UciOption[] GetOptions();
	
	public abstract string? GetPlayedMove();
	
	public abstract int GetReachedDepth();
	
	public abstract bool IsRunning();
	
	public abstract bool IsThinking();
	
	public abstract void NewGame();
	
	public abstract void PauseThinking();
	
	public abstract void ResumeThinking();
	
	public abstract void SetOption(UciOption option, object value);
	
	public abstract void SetPosition(string? position, string[] moves);
	
	public abstract void StartThinking(int moveTime);
	
	public abstract void StartThinking(int depthLimit, int nodeLimit, int whiteTimeLeft, int blackTimeLeft, int whiteIncrement, int blackIncrement);

	public abstract void StopThinking();
}

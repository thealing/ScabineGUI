namespace Scabine.Scenes;

public struct InputState
{
	public bool[] Keys;
	public bool LeftButton;
	public bool RightButton;
	public int MouseX;
	public int MouseY;
	public int MouseScroll;

	public InputState()
	{
		Keys = new bool[256];
		LeftButton = false;
		RightButton = false;
		MouseX = 0;
		MouseY = 0;
		MouseScroll = 0;
	}
}

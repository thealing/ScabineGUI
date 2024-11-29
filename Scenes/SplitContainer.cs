namespace Scabine.Scenes;

using System;
using System.Drawing;
using System.Windows.Forms;

public class SplitContainer : Container
{
	public const int SplitterWidth = 10;

	public double Split
	{
		get
		{
			return _split;
		}
		set
		{
			_split = Math.Clamp(value, 0, 1);
		}
	}

	public SceneNode FirstChild
	{
		get
		{
			return _firstChild;
		}
		set
		{
			RemoveChild(_firstChild);
			_firstChild = value;
			AddChild(_firstChild);
		}
	}

	public SceneNode SecondChild
	{
		get
		{
			return _secondChild;
		}
		set
		{
			RemoveChild(_secondChild);
			_secondChild = value;
			AddChild(_secondChild);
		}
	}

	public SplitContainer(Direction direction, double split = 0.5)
	{
		_direction = direction;
		_split = split;
		_dragging = false;
		_firstChild = new SceneNode();
		_secondChild = new SceneNode();
		AddChild(_firstChild);
		AddChild(_secondChild);
	}

	public override void Update()
	{
		bool mouseOver = IsMouseOverSplitter();
		if (mouseOver || _dragging)
		{
			CursorManager.SetFocus(this);
		}
		base.Update();
		if (!_dragging && CursorManager.GetFocus() != this)
		{
			mouseOver = false;
		}
		if (mouseOver)
		{
			if (_dragging == InputManager.IsLeftButtonDown())
			{
				switch (_direction)
				{
					case Direction.Horizontal:
						CursorManager.SetCursor(Cursors.VSplit);
						break;
					case Direction.Vertical:
						CursorManager.SetCursor(Cursors.HSplit);
						break;
				}
			}
			if (InputManager.IsLeftButtonPressed())
			{
				_dragging = true;
			}
		}
		if (InputManager.IsLeftButtonReleased())
		{
			_dragging = false;
		}
		if (_dragging)
		{
			Point mousePosition = GetMousePosition();
			switch (_direction)
			{
				case Direction.Horizontal:
					_split = (double)mousePosition.X / Size.Width;
					break;
				case Direction.Vertical:
					_split = (double)mousePosition.Y / Size.Height;
					break;
			}
		}
		Size firstSize = _firstChild.GetMinSize();
		Size secondSize = _secondChild.GetMinSize();
		switch (_direction)
		{
			case Direction.Horizontal:
				_actualSplit = Math.Min(Math.Max(_split, (double)firstSize.Width / Size.Width), 1 - (double)secondSize.Width / Size.Width);
				break;
			case Direction.Vertical:
				_actualSplit = Math.Min(Math.Max(_split, (double)firstSize.Height / Size.Height), 1 - (double)secondSize.Height / Size.Height);
				break;
		}
	}

	protected override void UpdatePosition()
	{
		switch (_direction)
		{
			case Direction.Horizontal:
				int x = (int)(Size.Width * _actualSplit);
				_firstChild.Location = new Point(0, 0);
				_firstChild.Size = new Size(x, Size.Height);
				_secondChild.Location = new Point(x, 0);
				_secondChild.Size = new Size(Size.Width - x, Size.Height);
				break;
			case Direction.Vertical:
				int y = (int)(Size.Height * _actualSplit);
				_firstChild.Location = new Point(0, 0);
				_firstChild.Size = new Size(Size.Width, y);
				_secondChild.Location = new Point(0, y);
				_secondChild.Size = new Size(Size.Width, Size.Height - y);
				break;
		}
		base.UpdatePosition();
	}

	protected override void AfterRender(Graphics g)
	{
		switch (_direction)
		{
			case Direction.Horizontal:
				int x = (int)(Size.Width * _actualSplit);
				g.DrawLine(_borderPen, new Point(x, 0), new Point(x, Size.Height));
				break;
			case Direction.Vertical:
				int y = (int)(Size.Height * _actualSplit);
				g.DrawLine(_borderPen, new Point(0, y), new Point(Size.Width, y));
				break;
		}
		base.AfterRender(g);
	}

	protected internal override Size GetMinSize()
	{
		Size minSize = base.GetMinSize();
		Size firstSize = _firstChild.GetMinSize();
		Size secondSize = _secondChild.GetMinSize();
		switch (_direction)
		{
			case Direction.Horizontal:
				minSize.Width = Math.Max(minSize.Width, firstSize.Width + secondSize.Width);
				minSize.Height = Math.Max(minSize.Height, firstSize.Height);
				minSize.Height = Math.Max(minSize.Height, secondSize.Height);
				break;
			case Direction.Vertical:
				minSize.Width = Math.Max(minSize.Width, firstSize.Width);
				minSize.Width = Math.Max(minSize.Width, secondSize.Width);
				minSize.Height = Math.Max(minSize.Height, firstSize.Height + secondSize.Height);
				break;
		}
		return minSize;
	}

	private bool IsMouseOverSplitter()
	{
		if (!ContainsMouse())
		{
			return false;
		}
		Point mousePosition = GetMousePosition();
		switch (_direction)
		{
			case Direction.Horizontal:
				int x = (int)(Size.Width * _actualSplit);
				return Math.Abs(mousePosition.X - x) <= SplitterWidth;
			case Direction.Vertical:
				int y = (int)(Size.Height * _actualSplit);
				return Math.Abs(mousePosition.Y - y) <= SplitterWidth;
			default:
				return false;
		}
	}

	private readonly Direction _direction;
	protected double _split;
	protected double _actualSplit;
	protected bool _dragging;
	protected SceneNode _firstChild;
	protected SceneNode _secondChild;
}

namespace ChessPanel.Scenes;

using System;
using System.Drawing;

public class ScrollableContainer : Container
{
	public int ScrollbarWidth = 10;
	public int VirtualHeight = 0;
	public int ScrollHeight = 0;
	public int ScrollUnit = 0;

	protected bool Dragging => _dragAnchor != null;

	public ScrollableContainer()
	{
		InvalidationManager.RegisterInvalidatingField(this, nameof(_hasScrollbar));
		InvalidationManager.RegisterInvalidatingField(this, nameof(_scrollRectangle));
		InvalidationManager.RegisterInvalidatingField(this, nameof(_scrollbarRectangle));
		InvalidationManager.RegisterInvalidatingField(this, nameof(_dragAnchor));
		InvalidationManager.RegisterInvalidatingField(this, nameof(_lastScrollHeight));
		InvalidationManager.RegisterInvalidatingField(this, nameof(ScrollbarWidth));
		InvalidationManager.RegisterInvalidatingField(this, nameof(VirtualHeight));
		InvalidationManager.RegisterInvalidatingField(this, nameof(ScrollHeight));
		InvalidationManager.RegisterInvalidatingField(this, nameof(ScrollUnit));
	}

	public override void Update()
	{
		if (ScrollHeight != _lastScrollHeight)
		{
			_lastScrollHeight = ScrollHeight;
		}
		base.Update();
	}

	public override bool ContainsMouse()
	{
		return base.ContainsMouse() && !_scrollRectangle.Contains(GetMousePosition());
	}

	protected override void UpdatePosition()
	{
		base.UpdatePosition();
		_hasScrollbar = VirtualHeight > Size.Height;
		if (_hasScrollbar)
		{
			Point mousePosition = GetMousePosition();
			if (InputManager.IsLeftButtonPressed())
			{
				if (_scrollRectangle.Contains(mousePosition))
				{
					if (_scrollbarRectangle.Contains(mousePosition))
					{
						_dragAnchor = mousePosition.Y - _scrollbarRectangle.Y;
					}
					else
					{
						ScrollHeight = mousePosition.Y * VirtualHeight / Size.Height - Size.Height / 2;
					}
				}
			}
			if (InputManager.IsLeftButtonReleased())
			{
				_dragAnchor = null;
			}
			if (_dragAnchor != null)
			{
				ScrollHeight = (mousePosition.Y - _dragAnchor.Value) * VirtualHeight / Size.Height;
			}
			if (base.ContainsMouse())
			{
				if (ScrollUnit == 0)
				{
					ScrollHeight -= InputManager.GetMouseScroll();
				}
				else
				{
					ScrollHeight -= Math.Sign(InputManager.GetMouseScroll()) * ScrollUnit;
				}
			}
			VirtualHeight = Math.Max(VirtualHeight, 1);
			ScrollHeight = Math.Max(Math.Min(ScrollHeight, VirtualHeight - Size.Height + 1), 0);
		}
		else
		{
			ScrollHeight = 0;
		}
	}

	protected override void BeforeRender(Graphics g)
	{
		base.BeforeRender(g);
		g.TranslateTransform(0, -ScrollHeight);
		_clipChanger = new ClipChanger(g, GetRenderBounds());
	}

	protected override void AfterRender(Graphics g)
	{
		if (_clipChanger != null)
		{
			_clipChanger.Dispose();
			_clipChanger = null;
		}
		g.TranslateTransform(0, ScrollHeight);
		if (_hasScrollbar && VirtualHeight > 0)
		{
			int barHeight = Math.Max(5, Size.Height * Size.Height / VirtualHeight);
			_scrollRectangle = new Rectangle(Size.Width - ScrollbarWidth, 0, ScrollbarWidth, Size.Height);
			_scrollbarRectangle = new Rectangle(Size.Width - ScrollbarWidth, Math.Clamp((int)Math.Ceiling((double)ScrollHeight * Size.Height / VirtualHeight), 0, Size.Height - barHeight + 1), ScrollbarWidth, barHeight);
			g.FillRectangle(_backgroundBrush, _scrollRectangle);
			g.FillRectangle(_scrollbarBrush, _scrollbarRectangle);
			g.DrawRectangle(_borderPen, _scrollRectangle);
		}
		base.AfterRender(g);
	}

	protected override Rectangle GetRenderBounds()
	{
		return new Rectangle(0, ScrollHeight, _hasScrollbar ? Size.Width - ScrollbarWidth : Size.Width, Size.Height);
	}

	private readonly Brush _backgroundBrush = new SolidBrush(Color.White);
	private readonly Brush _scrollbarBrush = new SolidBrush(Color.DimGray);
	private bool _hasScrollbar;
	private Rectangle _scrollRectangle;
	private Rectangle _scrollbarRectangle;
	private int? _dragAnchor;
	private ClipChanger? _clipChanger;
	private int _lastScrollHeight;
}

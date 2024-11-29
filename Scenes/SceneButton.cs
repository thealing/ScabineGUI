namespace Scabine.Scenes;

using System;
using System.Drawing;
using System.Drawing.Drawing2D;

public class SceneButton : Container
{
	public bool Disabled;
	public Font? Font;
	public string? Text;
	public Image? Image;

	public bool Clicked => _clicked && !Disabled;

	public SceneButton()
	{
		_borderPen = Pens.Black;
		_hoveredBrush = new SolidBrush(Color.FromArgb(150, Color.LightGreen));
		_pressedBrush = new SolidBrush(Color.FromArgb(150, Color.DarkGreen));
		_disabledBrush = new SolidBrush(Color.FromArgb(200, Color.White));
		_hovered = false;
		_pressed = false;
		_clicked = false;
	}

	public override void Update()
	{
		_hovered = ContainsMouse();
		if (_hovered && InputManager.IsLeftButtonPressed())
		{
			_pressed = true;
		}
		if (!_pressed && InputManager.IsLeftButtonDown())
		{
			_hovered = false;
		}
		if (_pressed && InputManager.IsLeftButtonReleased())
		{
			_clicked = true;
		}
		else
		{
			_clicked = false;
		}
		if (!_hovered || !InputManager.IsLeftButtonDown())
		{
			_pressed = false;
		}
		if (Disabled)
		{
			_pressed = false;
		}
		base.Update();
	}

	public override void Render(Graphics g)
	{
		if (!Disabled)
		{
			if (_pressed)
			{
				g.FillRectangle(_pressedBrush, SelfBounds);
			}
			else if (_hovered)
			{
				g.FillRectangle(_hoveredBrush, SelfBounds);
			}
		}
		using (new CompositingQualityChanger(g, CompositingQuality.HighQuality))
		{
			if (Image != null)
			{
				Size size = Size.Ceiling((SizeF)Image.Size * Math.Min((float)Size.Width / Image.Width, (float)Size.Height / Image.Height));
				if (_cachedImage == null || size != _cachedSize || _previousImage != Image)
				{
					_previousImage = Image;
					_cachedSize = size;
					_cachedImage = new Bitmap(size.Width, size.Height);
					using (Graphics cacheGraphics = Graphics.FromImage(_cachedImage))
					{
						cacheGraphics.DrawImage(Image, 0, 0, _cachedImage.Width, _cachedImage.Height);
					}
				}
				g.DrawImage(_cachedImage, (Size.Width - size.Width) / 2, (Size.Height - size.Height) / 2);
			}
			if (Text != null)
			{
				g.DrawString(Text, Font ?? SystemFonts.DefaultFont, Brushes.Black, SelfBounds, StringFormats.Centered);
			}
		}
		if (Disabled)
		{
			g.FillRectangle(_disabledBrush, SelfBounds);
		}
		base.Render(g);
	}

	private readonly Brush _hoveredBrush;
	private readonly Brush _pressedBrush;
	private readonly Brush _disabledBrush;
	private bool _hovered;
	private bool _pressed;
	private bool _clicked;
	private Image? _previousImage;
	private Bitmap? _cachedImage;
	private Size _cachedSize;
}

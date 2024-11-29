namespace Scabine.Scenes;

using System.Drawing.Drawing2D;
using System.Drawing;
using System;

public sealed class TransformChanger : IDisposable
{
	public static TransformChanger Translate(Graphics graphics, float x, float y)
	{
		TransformChanger changer = new TransformChanger(graphics);
		changer._graphics.TranslateTransform(x, y);
		return changer;
	}

	public static TransformChanger Scale(Graphics graphics, float x, float y)
	{
		TransformChanger changer = new TransformChanger(graphics);
		changer._graphics.ScaleTransform(x, y);
		return changer;
	}

	public static TransformChanger Rotate(Graphics graphics, float angle)
	{
		TransformChanger changer = new TransformChanger(graphics);
		changer._graphics.RotateTransform(angle);
		return changer;
	}

	public static TransformChanger Multiply(Graphics graphics, Matrix multiplier)
	{
		TransformChanger changer = new TransformChanger(graphics);
		changer._graphics.MultiplyTransform(multiplier);
		return changer;
	}

	private TransformChanger(Graphics graphics)
	{
		_graphics = graphics;
		_oldTransform = _graphics.Transform.Clone();
	}

	public void Dispose()
	{
		_graphics.Transform = _oldTransform;
	}

	private readonly Graphics _graphics;
	private readonly Matrix _oldTransform;
}
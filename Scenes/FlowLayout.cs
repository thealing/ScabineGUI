namespace Scabine.Scenes;

using System;
using System.Drawing;
using System.Linq;

internal class FlowLayout : Container
{
	public FlowLayout(Direction direction)
	{
		_direction = direction;
	}

	public override void Update()
	{
		int position = 0;
		foreach (SceneNode child in Children)
		{
			Size size = child.GetMinSize();
			switch (_direction)
			{
				case Direction.Horizontal:
					child.Location = new Point(position, 0);
					child.Size = new Size(child == Children.Last() ? Size.Width - position : size.Width, Size.Height);
					position += size.Width;
					break;
				case Direction.Vertical:
					child.Location = new Point(0, position);
					child.Size = new Size(Size.Width, child == Children.Last() ? Size.Height - position : size.Height);
					position += size.Height;
					break;
			}
		}
		base.Update();
	}

	public override void AddChild(SceneNode node)
	{
		Container child = new Container();
		child.AddChild(node);
		base.AddChild(child);
	}

	public override void RemoveChild(SceneNode node)
	{
		foreach (SceneNode child in Children)
		{
			if (child.Children.Contains(node))
			{
				base.RemoveChild(child);
			}
		}
	}

	protected internal override Size GetMinSize()
	{
		int width = 0;
		int height = 0;
		foreach (SceneNode child in Children)
		{
			Size size = child.GetMinSize();
			switch (_direction)
			{
				case Direction.Horizontal:
					width += size.Width;
					height = Math.Max(height, size.Height);
					break;
				case Direction.Vertical:
					width = Math.Max(width, size.Width);
					height += size.Height;
					break;
			}
		}
		return new Size(width, height);
	}

	private readonly Direction _direction;
}

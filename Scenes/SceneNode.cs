namespace Scabine.Scenes;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Policy;

public class SceneNode
{
	public Size MinSize = new Size(0, 0);

	public Point Location
	{ 
		get => _location; 
		set => _location = value; 
	}

	public Size Size
	{
		get => _size;
		set => _size = value;
	}

	public Rectangle Bounds
	{
		get
		{
			return new Rectangle(Location, Size);
		}
		set
		{
			Location = value.Location;
			Size = value.Size;
		}
	}

	public Rectangle SelfBounds => new Rectangle(Point.Empty, Size);

	public Point SceneLocation => Parent != null ? Location + (Size)Parent.SceneLocation : Location;

	public Rectangle SceneBounds => new Rectangle(SceneLocation, Size);

	public SceneNode? Parent => _parent;

	public Size ParentSize => Parent != null ? Parent.Size : new Size(0, 0);

	public List<SceneNode> Children => _children;

	public SceneNode()
	{
		_location = new Point(0, 0);
		_size = new Size(0, 0);
		_children = new List<SceneNode>();
		_parent = null;
	}

	public virtual void Enter()
	{
		foreach (SceneNode child in CloneChildren())
		{
			child.Enter();
		}
	}

	public virtual void Leave()
	{
		foreach (SceneNode child in CloneChildren())
		{
			child.Leave();
		}
	}

	public virtual void Update()
	{
		foreach (SceneNode child in CloneChildren())
		{
			double timeBeforeUpdate = Time.GetTime();
			SceneProfiler.UpdateDurations[child] = 0;
			child.UpdatePosition();
			child.Update();
			double timeAfterUpdate = Time.GetTime();
			double updateDuration = timeAfterUpdate - timeBeforeUpdate;
			SceneProfiler.UpdateDurations[child] += updateDuration;
			if (child.Parent != null && SceneProfiler.UpdateDurations.ContainsKey(child.Parent))
			{
				SceneProfiler.UpdateDurations[child.Parent] -= updateDuration;
			}
		}
	}

	public virtual void Render(Graphics g)
	{
		foreach (SceneNode child in CloneChildren())
		{
			if (child.SelfBounds.Width <= 0 || child.SelfBounds.Height <= 0)
			{
				continue;
			}
			double timeBeforeRender = Time.GetTime();
			SceneProfiler.RenderDurations[child] = 0;
			using (TransformChanger.Translate(g, child._location.X, child._location.Y))
			{
				child.BeforeRender(g);
				child.Render(g);
				child.AfterRender(g);
			}
			double timeAfterRender = Time.GetTime();
			double updateDuration = timeAfterRender - timeBeforeRender;
			SceneProfiler.RenderDurations[child] += updateDuration;
			if (child.Parent != null && SceneProfiler.RenderDurations.ContainsKey(child.Parent))
			{
				SceneProfiler.RenderDurations[child.Parent] -= updateDuration;
			}
		}
	}

	public virtual void AddChild(SceneNode node)
	{
		_children.Add(node);
		node._parent = this;
		node.Enter();
	}

	public virtual void RemoveChild(SceneNode node)
	{
		node.Leave();
		node._parent = null;
		_children.Remove(node);
	}

	public virtual void AddSibling(SceneNode node)
	{
		Parent?.AddChild(node);
	}

	public virtual void RemoveSibling(SceneNode node)
	{
		Parent?.RemoveChild(node);
	}

	public virtual bool ContainsMouse()
	{
		return SelfBounds.Contains(GetMousePosition());
	}

	public T? FindChildByType<T>() where T : SceneNode
	{
		if (this is T match)
		{
			return match;
		}
		T? result = null;
		foreach (SceneNode child in CloneChildren())
		{
			result = child.FindChildByType<T>();
			if (result != null)
			{
				break;
			}
		}
		return result;
	}

	protected internal virtual Size GetMinSize()
	{
		Size size = MinSize;
		foreach (SceneNode child in CloneChildren())
		{
			Size childSize = child.GetMinSize();
			size.Width = Math.Max(size.Width, childSize.Width);
			size.Height = Math.Max(size.Height, childSize.Height);
		}
		return size;
	}

	protected virtual Rectangle GetRenderBounds()
	{
		return new Rectangle(Point.Empty, Size);
	}

	protected virtual void UpdatePosition()
	{
		_size = new Size(Math.Max(_size.Width, 1), Math.Max(_size.Height, 1));
	}

	protected virtual void BeforeRender(Graphics g)
	{
	}

	protected virtual void AfterRender(Graphics g)
	{
	}

	protected virtual Point GetMousePosition()
	{
		if (_parent == null)
		{
			return InputManager.GetMousePosition();
		}
		else
		{
			return _parent.GetMousePosition() - (Size)Location;
		}
	}

	protected List<SceneNode> CloneChildren()
	{
		return new List<SceneNode>(_children);
	}

	private Point _location;
	private Size _size;
	private List<SceneNode> _children;
	private SceneNode? _parent;
}

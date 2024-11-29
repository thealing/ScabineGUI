namespace Scabine.Scenes;

using System;
using System.Drawing;
using System.Drawing.Drawing2D;

public class Scene
{
	internal SceneNode Root => _root;

	public Scene()
	{
		_root = new SceneNode();
	}

	public virtual string GetTitle()
	{
		return "Untitled Scene";
	}

	public virtual bool CanExit()
	{
		return true;
	}

	public void AddChild(SceneNode node)
	{
		_root.AddChild(node);
	}

	public void RemoveChild(SceneNode node)
	{
		_root.RemoveChild(node);
	}

	public T? FindNodeByType<T>() where T : SceneNode
	{
		return _root.FindChildByType<T>();
	}

	protected internal virtual void Enter()
	{
		_root.Enter();
	}

	protected internal virtual void Leave()
	{
		_root.Leave();
	}

	protected internal virtual void Update()
	{
		SceneProfiler.BeforeUpdate();
		_root.Update();
		SceneProfiler.AfterUpdate();
	}

	protected internal virtual void Render(Graphics g)
	{
		SceneProfiler.BeforeRender();
		_root.Render(g);
		SceneProfiler.AfterRender();
	}

	internal virtual void Resize(Rectangle rectangle)
	{
		_root.Bounds = rectangle;
		foreach (SceneNode child in _root.Children)
		{
			child.Bounds = _root.Bounds;
		}
	}

	internal Size GetMinSize()
	{
		return _root.GetMinSize();
	}

	protected SceneNode _root;
}

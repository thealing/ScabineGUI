namespace Scabine.App;

using Scabine.Engines;
using Scabine.Scenes;
using System;
using System.Drawing;
using System.Linq;

internal class EngineContainer : Container
{
	public EngineControl AddEngine(IEngine engine, string presetName)
	{
		EngineControl control = new EngineControl(engine, presetName);
		AddChild(control);
		return control;
	}

	public void RemoveEngine(EngineControl control)
	{
		RemoveChild(control);
	}

	public override void Update()
	{
		if (Children.Any())
		{
			int gap = 20;
			int controlHeight = (Size.Height - gap) / Children.Count() - gap;
			int actualHeight = gap;
			foreach (SceneNode child in Children)
			{
				int width = Math.Max(1, Size.Width - gap * 2);
				int height = Math.Max(1, controlHeight);
				child.Location = new Point(gap, actualHeight);
				child.Size = new Size(width, height);
				actualHeight += controlHeight + gap;
			}
		}
		base.Update();
	}
}

namespace Scabine.Scenes;

using System.Drawing;

public class Container : SceneNode
{
	public Container()
	{
		_borderPen = new Pen(Color.Black, 2);
	}

	protected override void AfterRender(Graphics g)
	{
		Rectangle borderRectangle = SelfBounds;
		if (Parent?.Parent == null)
		{
			borderRectangle.Inflate(-1, -1);
		}
		g.DrawRectangle(_borderPen, borderRectangle);
		base.AfterRender(g);
	}

	protected Pen _borderPen;
}

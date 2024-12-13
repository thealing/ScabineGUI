namespace Scabine.App;

using Scabine.App.Prefs;
using Scabine.Scenes;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using static Scabine.Core.Pieces;

internal class PlayerDisplay : Container
{
	public PlayerDisplay()
	{
		MinSize = new Size(200, 100);
		_borderPen = new Pen(Color.Transparent);
		_nameFont = new Font("Tahoma", 12);
		_clockFont = new Font("MS Reference Sans Serif", 20);
		_backgroundBrush = new SolidBrush(Color.Snow);
		_foregroundBrush = new SolidBrush(Color.Black);
		_activeBrush = new SolidBrush(Color.LightGreen);
		_flaggedBrush = new SolidBrush(Color.IndianRed);
	}

	public override void Render(Graphics g)
	{
		int padding = Size.Height / 20;
		Size size = TextRenderer.MeasureText("99:99.9", _clockFont);
		int width = Size.Width - padding * 2 - size.Width;
		int height = Size.Height / 2;
		int whiteHeight = Board.Flipped ? 0 : height;
		int blackHeight = Board.Flipped ? height : 0;
		g.DrawString(PgnManager.GetValue("White"), _nameFont, _foregroundBrush, new Rectangle(padding, whiteHeight, width - padding * 2, height), StringFormats.LeftClipped);
		g.DrawString(PgnManager.GetValue("Black"), _nameFont, _foregroundBrush, new Rectangle(padding, blackHeight, width - padding * 2, height), StringFormats.LeftClipped);
		Rectangle whiteRectangle = new Rectangle(width, whiteHeight, Size.Width - width, height);
		Rectangle blackRectangle = new Rectangle(width, blackHeight, Size.Width - width, height);
		whiteRectangle.Inflate(-padding, -padding);
		blackRectangle.Inflate(-padding, -padding);
		g.FillRectangle(_backgroundBrush, whiteRectangle);
		g.FillRectangle(_backgroundBrush, blackRectangle);
		int whiteTime = 0;
		int blackTime = 0;
		if (MatchManager.IsPlaying())
		{
			whiteTime = MatchManager.GetWhiteClock();
			blackTime = MatchManager.GetBlackClock();
			TreeNode lastNode = GameManager.GetGame().GetLastNode();
			int turn = lastNode.Color ^ 1;
			if (whiteTime < 0)
			{
				turn = -1;
				g.FillRectangle(_flaggedBrush, whiteRectangle);
			}
			if (blackTime < 0)
			{
				turn = -1;
				g.FillRectangle(_flaggedBrush, blackRectangle);
			}
			if (!MatchManager.IsPaused() && !MatchManager.IsFinished())
			{
				if (turn == White)
				{
					g.FillRectangle(_activeBrush, whiteRectangle);
				}
				if (turn == Black)
				{
					g.FillRectangle(_activeBrush, blackRectangle);
				}
			}
		}
		else
		{
			TreeNode? node = GameManager.GetGame().GetCurrentNode();
			while (node != null && (!node.IsMainLine || node.Time == null))
			{
				node = node.Parent;
			}
			node ??= GameManager.GetGame().GetRootNode().Children.FirstOrDefault();
			int previousTime = node?.Parent?.Time ?? 0;
			int nextTime = node?.Children.FirstOrDefault()?.Time ?? 0;
			if (node != null)
			{
				int currentTime = node.Time ?? 0;
				int opponentTime = node.Rank < 1 ? nextTime : previousTime;
				if (node.Color == White)
				{
					whiteTime = currentTime;
					blackTime = opponentTime;
				}
				if (node.Color == Black)
				{
					blackTime = currentTime;
					whiteTime = opponentTime;
				}
			}
		}
		g.DrawRectangle(Pens.Black, whiteRectangle);
		g.DrawRectangle(Pens.Black, blackRectangle);
		whiteTime = Math.Max(whiteTime, 0);
		blackTime = Math.Max(blackTime, 0);
		int whiteSeconds = whiteTime / 1000;
		int blackSeconds = blackTime / 1000;
		int whiteTenths = whiteTime / 100 % 10;
		int blackTenths = blackTime / 100 % 10;
		string whiteText = $"{whiteSeconds / 60,2}:{whiteSeconds % 60:D2}.{whiteTenths}";
		string blackText = $"{blackSeconds / 60,2}:{blackSeconds % 60:D2}.{blackTenths}";
		whiteRectangle.Inflate(-padding, -padding);
		blackRectangle.Inflate(-padding, -padding);
		g.DrawString(whiteText, _clockFont, _foregroundBrush, whiteRectangle, StringFormats.Centered);
		g.DrawString(blackText, _clockFont, _foregroundBrush, blackRectangle, StringFormats.Centered);
		base.Render(g);
	}

	protected override void UpdatePosition()
	{
		base.UpdatePosition();
		int gap = 10;
		int width = Math.Max(1, ParentSize.Width - gap * 2);
		int height = Math.Max(1, ParentSize.Height - gap * 2);
		Location = new Point((ParentSize.Width - width) / 2, (ParentSize.Height - height) / 2);
		Size = new Size(width, height);
		MinSize = new Size(_clockFont.Height * 8, _clockFont.Height * 6 / 2 + gap * 2);
		if (!PgnManager.HasValue("White") && !PgnManager.HasValue("Black"))
		{
			Size = Size.Empty;
			MinSize = Size.Empty;
		}
	}

	private readonly Font _nameFont;
	private readonly Font _clockFont;
	private readonly Brush _backgroundBrush;
	private readonly Brush _foregroundBrush;
	private readonly Brush _activeBrush;
	private readonly Brush _flaggedBrush;
}

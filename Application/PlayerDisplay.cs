namespace ChessBand.Application;

using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ChessBand.Application.Settings;
using ChessBand.Scenes;
using static ChessBand.Core.Pieces;

internal class PlayerDisplay : Container
{
	public PlayerDisplay()
	{
		_borderPen = new Pen(Color.Transparent);
		_nameFont = new Font("Tahoma", 12);
		_clockFont = new Font("MS Reference Sans Serif", 20);
		_backgroundBrush = new SolidBrush(Color.FromArgb(250, 250, 250));
		_activeBrush = new SolidBrush(Color.LightGreen);
		_flaggedBrush = new SolidBrush(Color.IndianRed);
		_foregroundColor = Color.Black;
		_fontHeight = _clockFont.Height;
		_clockWidth = TextRenderer.MeasureText("99:99.9", _clockFont).Width;
		InvalidationManager.RegisterInvalidatingField(this, nameof(_whiteSeconds));
		InvalidationManager.RegisterInvalidatingField(this, nameof(_whiteTenths));
		InvalidationManager.RegisterInvalidatingField(this, nameof(_blackSeconds));
		InvalidationManager.RegisterInvalidatingField(this, nameof(_blackTenths));
	}

	public override void Render(Graphics g)
	{
		int padding = Size.Height / 20;
		int width = Size.Width - padding * 2 - _clockWidth;
		int height = Size.Height / 2;
		int whiteHeight = Board.Flipped ? 0 : height;
		int blackHeight = Board.Flipped ? height : 0;
		GraphicsHelper.DrawText(g, PgnManager.GetValue("White"), _nameFont, new Rectangle(padding, whiteHeight, width - padding * 2, height), _foregroundColor, TextFormats.LeftClipped);
		GraphicsHelper.DrawText(g, PgnManager.GetValue("Black"), _nameFont, new Rectangle(padding, blackHeight, width - padding * 2, height), _foregroundColor, TextFormats.LeftClipped);
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
				SceneManager.ScheduleUpdate();
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
		_whiteSeconds = whiteTime / 1000;
		_blackSeconds = blackTime / 1000;
		_whiteTenths = whiteTime / 100 % 10;
		_blackTenths = blackTime / 100 % 10;
		string whiteText = $"{_whiteSeconds / 60,2}:{_whiteSeconds % 60:D2}.{_whiteTenths}";
		string blackText = $"{_blackSeconds / 60,2}:{_blackSeconds % 60:D2}.{_blackTenths}";
		whiteRectangle.Inflate(-padding, -padding);
		blackRectangle.Inflate(-padding, -padding);
		GraphicsHelper.DrawText(g, whiteText, _clockFont, whiteRectangle, _foregroundColor, TextFormats.Centered);
		GraphicsHelper.DrawText(g, blackText, _clockFont, blackRectangle, _foregroundColor, TextFormats.Centered);
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
		MinSize = new Size(_fontHeight * 8, _fontHeight * 6 / 2 + 20);
		if (!PgnManager.HasValue("White") && !PgnManager.HasValue("Black"))
		{
			Size = Size.Empty;
			MinSize = Size.Empty;
		}
	}

	private readonly Font _nameFont;
	private readonly Font _clockFont;
	private readonly Brush _backgroundBrush;
	private readonly Brush _activeBrush;
	private readonly Brush _flaggedBrush;
	private readonly Color _foregroundColor;
	private readonly int _fontHeight;
	private readonly int _clockWidth;
	private int _whiteSeconds;
	private int _blackSeconds;
	private int _whiteTenths;
	private int _blackTenths;
}

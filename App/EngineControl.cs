namespace Scabine.App;

using Scabine.Core;
using Scabine.Engines;
using Scabine.App.Prefs;
using Scabine.Scenes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using static Scabine.Core.Pieces;
using static Scabine.Core.Game;

internal class EngineControl : Container
{
	public static readonly string[] Columns = { "Depth", "Score" };

	public EngineControl(IEngine engine, string presetName)
	{
		_borderPen = new Pen(Color.Black);
		_moveList = new MoveList(this);
		_font = new Font("Segoe UI", 10);
		_backgroundBrush = new SolidBrush(Color.Snow);
		_foregroundBrush = new SolidBrush(Color.Black);
		_hoveredBrush = new SolidBrush(Color.LightGray);
		_columnPositions = new int[Columns.Length];
		_columnWidths = new int[Columns.Length];
		_columnValues = new string[MaxDepth + 1][];
		_engine = engine;
		_presetName = presetName;
		_game = new Game();
		_depthMoves = new Dictionary<int, IEnumerable<string>>();
		_depthUciMoves = new Dictionary<int, IEnumerable<string>>();
		_columnsDirty = true;
		_moveNumber = -1;
	}

	public void SetEngine(IEngine engine)
	{
		_engine = engine;
		_moveNumber = -1;
	}

	public void Remove()
	{
		if (Parent is EngineContainer engineContainer)
		{
			engineContainer.RemoveEngine(this);
		}
	}

	public override void Enter()
	{
		base.Enter();
	}

	public override void Leave()
	{
		base.Leave();
		_engine.StopThinking();
		_engine.Dispose();
	}

	public override void Update()
	{
		UpdateEngine();
		UpdateColumns();
		UpdateMouse();
		base.Update();
	}

	public override void Render(Graphics g)
	{
		RenderBackground(g);
		RenderData(g);
		base.Render(g);
	}

	protected override void UpdatePosition()
	{
		_rowHeight = _font.Height * 5 / 4;
		_padding = _rowHeight / 4;
		Size = new Size(Size.Width, Size.Height / _rowHeight * _rowHeight);
		base.UpdatePosition();
	}

	private void SwapWithSibling(int delta)
	{
		if (Parent != null)
		{
			int index = Parent.Children.IndexOf(this);
			int siblingIndex = index + delta;
			if (siblingIndex >= 0 && siblingIndex < Parent.Children.Count)
			{
				SceneNode sibling = Parent.Children[siblingIndex];
				Parent.Children[siblingIndex] = this;
				Parent.Children[index] = sibling;
			}
		}
	}

	private void UpdateEngine()
	{
		Game game = GameManager.GetGame();
		int moveNumber = game.GetStartingColor() + game.GetPly();
		if (_moveNumber != moveNumber && (IsAnalyzing() || MatchManager.IsEngineThinking(_engine)))
		{
			_moveNumber = moveNumber;
			_analysisTime = Time.GetTime();
			_game.SetFen(game.GetFen());
			_depthMoves.Clear();
			_depthUciMoves.Clear();
			Array.Fill(_columnValues, null);
			_moveList.ScrollHeight = 0;
			StartThinking();
		}
		if (IsAnalyzing())
		{
			if (Time.GetTime() > _analysisTime + Engines.MaxAnalysisTime / 1000.0)
			{
				_engine.StopThinking();
			}
			if (SceneManager.IsBackground())
			{
				_engine.PauseThinking();
			}
			else
			{
				_engine.ResumeThinking();
			}
		}
		int reachedDepth = _engine.GetReachedDepth();
		for (int depth = reachedDepth; depth >= 1; depth--)
		{
			if (_columnValues[depth] == null)
			{
				_columnsDirty = true;
				_columnValues[depth] = new string[Columns.Length];
			}
			for (int column = 0; column < Columns.Length; column++)
			{
				string value = GetColumnValue(column, depth);
				if (value != _columnValues[depth][column])
				{
					_columnsDirty = true;
					_depthMoves.Remove(depth);
				}
				_columnValues[depth][column] = value;
			}
			if (!_depthMoves.ContainsKey(depth))
			{
				_depthMoves[depth] = Array.Empty<string>();
			}
			string moveList = _engine.GetBestMoves(depth);
			string[] uciMoves = moveList.Split(' ');
			if (!_depthUciMoves.ContainsKey(depth) || !uciMoves.SequenceEqual(_depthUciMoves[depth]))
			{
				_depthUciMoves[depth] = uciMoves;
				IEnumerable<string> sanMoves = ConvertMovesToSan(uciMoves);
				if (sanMoves.Count() == uciMoves.Length)
				{
					_depthMoves[depth] = sanMoves;
					_columnsDirty = true;
				}
			}
		}
	}

	private IEnumerable<string> ConvertMovesToSan(IEnumerable<string> uciMoves)
	{
		Move[] moves = new Move[MaxMoves];
		List<string> sanMoves = new List<string>();
		Game game = new Game();
		game.SetFen(_game.GetFen());
		foreach (string uciMove in uciMoves)
		{
			if (uciMove.Length < 4)
			{
				break;
			}
			Move move = game.ParseMove(uciMove);
			int moveCount = game.GenerateMoves(moves);
			if (!moves.Take(moveCount).Contains(move))
			{
				break;
			}
			sanMoves.Add(game.FormatMoveToSan(move));
			if (!game.PlayMove(move))
			{
				sanMoves.RemoveAt(sanMoves.Count - 1);
				break;
			}
		}
		return sanMoves;
	}

	private void UpdateColumns()
	{
		if (!_columnsDirty)
		{
			return;
		}
		_columnsDirty = false;
		_columnsTotalWidth = 0;
		for (int column = 0; column < Columns.Length; column++)
		{
			_columnPositions[column] = _columnsTotalWidth;
			_columnWidths[column] = TextRenderer.MeasureText(Columns[column], _font).Width;
			int reachedDepth = _engine.GetReachedDepth();
			_nonEmptyRowCount = reachedDepth;
			for (int depth = reachedDepth, row = 0; depth >= 1; depth--, row++)
			{
				if (!_depthMoves.ContainsKey(depth) || !_depthMoves[depth].Any())
				{
					_nonEmptyRowCount--;
					continue;
				}
				_columnWidths[column] = Math.Max(_columnWidths[column], TextRenderer.MeasureText(GetColumnValue(column, depth), _font).Width);
			}
			_columnWidths[column] += _padding * 2;
			_columnsTotalWidth += _columnWidths[column];
		}
		int minPvWidth = TextRenderer.MeasureText("Best line", _font).Width + _padding * 2;
		MinSize = new Size(_columnsTotalWidth + minPvWidth + 100, 160);
	}

	private void UpdateMouse()
	{
		if (InputManager.IsLeftButtonPressed())
		{
			int depth = _engine.GetReachedDepth();
			if (_hoveredRow >= 0 && _hoveredRow < depth && IsAnalyzing())
			{
				string moves = _engine.GetBestMoves(depth - _hoveredRow);
				if (moves.Length >= 4)
				{
					GameManager.TryPlayMove(moves);
				}
			}
		}
		if (InputManager.IsRightButtonPressed() && ContainsMouse())
		{
			ContextMenuStrip menu = MenuCreator.CreateContextMenu();
			MenuCreator.AddMenuLabel(menu, _engine.GetName());
			MenuCreator.AddMenuSeparator(menu);
			MenuCreator.AddMenuItem(menu, "Move up", MenuIcons.Up, () => SwapWithSibling(-1));
			MenuCreator.AddMenuItem(menu, "Move down", MenuIcons.Down, () => SwapWithSibling(1));
			if (!MatchManager.IsEnginePlaying(_engine))
			{
				MenuCreator.AddMenuItem(menu, "Reload", MenuIcons.Reload, () => EngineManager.ReloadEngine(_engine, _presetName));
				MenuCreator.AddMenuItem(menu, "Close", MenuIcons.Close, () => EngineManager.StopEngine(_engine));
			}
			menu.Show(Cursor.Position);
		}
	}

	private void RenderBackground(Graphics g)
	{
		g.FillRectangle(_backgroundBrush, GetRenderBounds());
	}

	private void RenderData(Graphics g)
	{
		using (new ClipChanger(g, SelfBounds))
		{
			int engineNameWidth = Size.Width * 2 / 3;
			Rectangle engineNameRectangle = new Rectangle(0, 0, engineNameWidth, _rowHeight);
			g.DrawRectangle(_borderPen, engineNameRectangle);
			g.DrawString(_engine.GetName(), _font, _foregroundBrush, engineNameRectangle, StringFormats.Centered);
			Rectangle presetNameRectangle = new Rectangle(engineNameWidth, 0, Size.Width - engineNameWidth - _rowHeight, _rowHeight);
			g.DrawRectangle(_borderPen, presetNameRectangle);
			Rectangle indicatorRectangle = new Rectangle(Size.Width - _rowHeight, 0, _rowHeight, _rowHeight);
			g.DrawRectangle(_borderPen, indicatorRectangle);
			g.DrawString(_presetName, _font, _foregroundBrush, presetNameRectangle, StringFormats.Centered);
			if (MatchManager.IsEnginePlaying(_engine))
			{
				using (new SmoothingModeChanger(g, SmoothingMode.HighQuality))
				{
					indicatorRectangle.Inflate(-_rowHeight / 4, -_rowHeight / 4);
					if (MatchManager.IsEnginePlayingItself(_engine))
					{
						g.FillPie(Brushes.White, indicatorRectangle, 90, 180);
						g.FillPie(Brushes.Black, indicatorRectangle, 270, 180);
					}
					else
					{
						Brush[] colorBrushes = new Brush[] { Brushes.White, Brushes.Black };
						g.FillEllipse(colorBrushes[MatchManager.GetEngineSide(_engine)], indicatorRectangle);
					}
					g.DrawEllipse(Pens.Black, indicatorRectangle);
				}
			}
			else
			{
				g.DrawString("A", _font, _foregroundBrush, indicatorRectangle, StringFormats.Centered);
			}
			for (int column = 0; column < Columns.Length; column++)
			{
				Rectangle subHeaderRectangle = new Rectangle(_columnPositions[column], _rowHeight, _columnWidths[column], _rowHeight);
				g.DrawRectangle(_borderPen, subHeaderRectangle);
				g.DrawString(Columns[column], _font, _foregroundBrush, subHeaderRectangle, StringFormats.Centered);
			}
			Rectangle pvRectangle = new Rectangle(_columnsTotalWidth, _rowHeight, Size.Width - _columnsTotalWidth, _rowHeight);
			g.DrawRectangle(_borderPen, pvRectangle);
			g.DrawString("Best line", _font, _foregroundBrush, pvRectangle, StringFormats.Centered);
		}
	}

	private void UpdateMoveList()
	{
		_moveList.Location = new Point(0, _rowHeight * 2);
		_moveList.Size = Size - (Size)_moveList.Location;
		_moveList.VirtualHeight = _nonEmptyRowCount * _rowHeight;
		_moveList.ScrollHeight = (_moveList.ScrollHeight + _rowHeight / 2) / _rowHeight * _rowHeight;
		_moveList.ScrollUnit = Size.Height / 2;
	}

	private void RenderMoveList(Graphics g)
	{
		Rectangle bounds = _moveList.GetBounds();
		_reachedDepth = _engine.GetReachedDepth();
		for (int depth = _reachedDepth, row = 0; depth >= 1; depth--, row++)
		{
			if (!_depthMoves.ContainsKey(depth) || !_depthMoves[depth].Any())
			{
				row--;
				continue;
			}
			int height = _rowHeight * row;
			if (row == _hoveredRow && IsAnalyzing())
			{
				Rectangle rowRectangle = new Rectangle(0, height, bounds.Width, _rowHeight);
				g.FillRectangle(_hoveredBrush, rowRectangle);
			}
			for (int column = 0; column < Columns.Length; column++)
			{
				Rectangle columnRectangle = new Rectangle(_columnPositions[column], height, _columnWidths[column], _rowHeight);
				g.DrawRectangle(_borderPen, columnRectangle);
				string value = GetColumnValue(column, depth);
				columnRectangle.Width -= _padding;
				g.DrawString(value, _font, _foregroundBrush, columnRectangle, StringFormats.RightAligned);
			}
			Rectangle pvColumnRectangle = new Rectangle(_columnsTotalWidth, height, bounds.Width - _columnsTotalWidth, _rowHeight);
			g.DrawRectangle(_borderPen, pvColumnRectangle);
			StringBuilder pv = new StringBuilder();
			int moveNumber = _moveNumber;
			if (moveNumber % 2 == 1)
			{
				pv.Append(moveNumber / 2 + 1);
				pv.Append("...");
			}
			foreach (string move in _depthMoves[depth])
			{
				if (moveNumber % 2 == 0)
				{
					pv.Append(moveNumber / 2 + 1);
					pv.Append('.');
				}
				pv.Append(move);
				pv.Append(' ');
				moveNumber++;
			}
			pvColumnRectangle.X += _padding;
			pvColumnRectangle.Width -= _padding;
			g.DrawString(pv.ToString(), _font, _foregroundBrush, pvColumnRectangle, StringFormats.LeftClipped);
		}
	}

	private string GetColumnValue(int column, int depth)
	{
		return Columns[column] switch
		{
			"Depth" => depth.ToString(),
			"Score" => GetScoreString(_engine.GetBestScore(depth)),
			_ => throw new Exception("Invalid column index")
		};
	}

	private string GetScoreString(int score)
	{
		return Scores.IsMateScore(score) ? $"Mate in {Scores.MateScore * Math.Sign(score) - score}" : $"{score / 100.0:0.00}";
	}

	private void StartThinking()
	{
		if (IsAnalyzing())
		{
			_engine.SetPosition(GameManager.GetGame().GetUciPosition(), GameManager.GetGame().GetUciMoves());
			_engine.StartThinking();
		}
	}

	private bool IsAnalyzing()
	{
		return !GameManager.GetGame().IsFinished() && !MatchManager.IsEnginePlaying(_engine);
	}

	private class MoveList : ScrollableContainer
	{
		public MoveList(EngineControl parent)
		{
			_borderPen = Pens.Black;
			_parent = parent;
			_parent.AddChild(this);
		}

		public Rectangle GetBounds()
		{
			return GetRenderBounds();
		}

		public override void Update()
		{
			_parent._hoveredRow = ContainsMouse() ? (GetMousePosition().Y + GetBounds().Top) / _parent._rowHeight : -3;
			_parent.UpdateMoveList();
			base.Update();
		}

		public override void Render(Graphics g)
		{
			_parent.RenderMoveList(g);
			base.Render(g);
		}

		private readonly EngineControl _parent;
	}

	private readonly MoveList _moveList;
	private readonly Font _font;
	private readonly Brush _backgroundBrush;
	private readonly Brush _foregroundBrush;
	private readonly Brush _hoveredBrush;
	private readonly int[] _columnPositions;
	private readonly int[] _columnWidths;
	private readonly string[][] _columnValues;
	private readonly string _presetName;
	private readonly Game _game;
	private readonly Dictionary<int, IEnumerable<string>> _depthMoves;
	private readonly Dictionary<int, IEnumerable<string>> _depthUciMoves;
	private IEngine _engine;
	private bool _columnsDirty;
	private int _reachedDepth;
	private int _nonEmptyRowCount;
	private int _rowHeight;
	private int _padding;
	private int _columnsTotalWidth;
	private int _hoveredRow;
	private int _moveNumber;
	private double _analysisTime;
}

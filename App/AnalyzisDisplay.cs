﻿namespace Scabine.App;

using System.Drawing;
using System;
using Scabine.Scenes;
using System.Collections.Generic;
using System.Linq;
using Scabine.Core;
using System.Drawing.Drawing2D;
using System.Xml.Linq;
using static Scabine.Core.Pieces;
using static Scabine.App.MoveClassifications;

internal class AnalyzisDisplay : Container
{
	public AnalyzisDisplay()
	{
		_borderPen = Pens.Black;
		_font = new Font("Segoe UI", 10);
		_curentPen = Pens.SkyBlue;
		_hoverPen = Pens.Turquoise;
		_whiteBrush = new SolidBrush(Color.FromArgb(230, 230, 230));
		_blackBrush = new SolidBrush(Color.FromArgb(20, 20, 20));
		_nodes = new List<TreeNode>();
		_evals = new List<int>();
		_classCounts = new int[ColorCount, MoveClassCount];
		_lineHeight = _font.Height * 6 / 5;
	}

	protected override void UpdatePosition()
	{
		base.UpdatePosition();
		_gap = 20;
		int width = Math.Max(1, ParentSize.Width - _gap * 2);
		int height = 120;
		Location = new Point((ParentSize.Width - width) / 2, _gap);
		Size = new Size(width, height);
		MinSize = new Size(200, height + _gap * 3 + _lineHeight * 10);
		if (!PgnManager.HasValue("WhiteAccuracy") && !PgnManager.HasValue("BlackAccuracy") || _classCounts.Cast<int>().All(count => count == 0))
		{
			Size = Size.Empty;
			MinSize = Size.Empty;
		}
	}

	public override void Update()
	{
		int[] oldEvals = _evals.ToArray();
		_nodes.Clear();
		_evals.Clear();
		Array.Clear(_classCounts, 0, _classCounts.Length);
		TreeGame game = GameManager.GetGame();
		TreeNode lastNode = game.GetLastNode();
		for (TreeNode? node = game.GetRootNode(); node != null; node = node.Children.FirstOrDefault())
		{
			if (node.Eval != null)
			{
				_nodes.Add(node);
				_evals.Add(node.Eval.Value);
			}
			else if (node == lastNode)
			{
				switch (game.GetResult())
				{
					case Result.WhiteWon:
						_nodes.Add(node);
						_evals.Add(Scores.MateScore);
						break;
					case Result.BlackWon:
						_nodes.Add(node);
						_evals.Add(-Scores.MateScore);
						break;
					case Result.Draw:
						_nodes.Add(node);
						_evals.Add(Scores.DrawScore);
						break;
				}
			}
			if (node.Class != null)
			{
				_classCounts[node.Color, node.Class.Value]++;
			}
		}
		if (ContainsMouse())
		{
			_hoveredIndex = Math.Clamp((int)Math.Round((double)(_nodes.Count - 1) * GetMousePosition().X / Size.Width), 0, _nodes.Count - 1);
			if (InputManager.IsLeftButtonPressed())
			{
				game.SetCurrentNode(_nodes[_hoveredIndex]);
			}
		}
		else
		{
			_hoveredIndex = -1;
		}
		if (Size != _graph?.Size || !_evals.SequenceEqual(oldEvals))
		{
			_graph = null;
		}
	}

	public override void Render(Graphics g)
	{
		if (_nodes.Count >= 2)
		{
			int[] x = new int[_nodes.Count];
			int[] y = new int[_nodes.Count];
			for (int i = 0; i < _nodes.Count; i++)
			{
				x[i] = Size.Width * i / (_nodes.Count - 1);
				y[i] = (int)(Size.Height * (1 - Scores.ToWinProbability(_evals[i])));
			}
			if (_graph == null)
			{
				_graph = new Bitmap(Size.Width, Size.Height);
				using (Graphics graphics = Graphics.FromImage(_graph))
				using (new SmoothingModeChanger(graphics, SmoothingMode.AntiAlias))
				{
					for (int i = 0; i < _nodes.Count - 1; i++)
					{
						graphics.FillPolygon(_whiteBrush, new Point[] { new Point(x[i], Size.Height), new Point(x[i + 1] + 1, Size.Height), new Point(x[i + 1] + 1, y[i + 1]), new Point(x[i], y[i]) });
						graphics.FillPolygon(_blackBrush, new Point[] { new Point(x[i], 0), new Point(x[i + 1] + 1, 0), new Point(x[i + 1] + 1, y[i + 1]), new Point(x[i], y[i]) });
					}
				}
			}
			g.DrawImage(_graph, 0, 0);
			if (_hoveredIndex >= 0 && _hoveredIndex < _nodes.Count)
			{
				g.DrawLine(_hoverPen, new Point(x[_hoveredIndex], 0), new Point(x[_hoveredIndex], Size.Height));
			}
			for (int i = 0; i < _nodes.Count; i++)
			{
				if (_nodes[i] == GameManager.GetGame().GetCurrentNode())
				{
					g.DrawLine(_curentPen, new Point(x[i], 0), new Point(x[i], Size.Height));
				}
			}
		}
		else
		{
			g.FillRectangle(Brushes.Snow, SelfBounds);
		}
		int height = Size.Height + 20;
		void RenderLine(string name, object value1, object value2)
		{
			g.DrawString(name, _font, Brushes.Black, new Rectangle(0, height, Size.Width * 2 / 6, _lineHeight), StringFormats.LeftClipped);
			g.DrawString(value1.ToString(), _font, Brushes.Black, new Rectangle(Size.Width * 2 / 6, height, Size.Width * 2 / 6, _lineHeight), StringFormats.CenteredClipped);
			g.DrawString(value2.ToString(), _font, Brushes.Black, new Rectangle(Size.Width * 4 / 6, height, Size.Width * 2 / 6, _lineHeight), StringFormats.CenteredClipped);
			height += _lineHeight;
		}
		void RenderSeparator()
		{
			g.DrawLine(Pens.Gray, new Point(5, height + _lineHeight / 4), new Point(Size.Width - 5, height + _lineHeight / 4));
			height += _lineHeight / 2;
		}
		RenderLine("", "White", "Black");
		RenderSeparator();
		RenderLine("Name", PgnManager.GetValue("White"), PgnManager.GetValue("Black"));
		RenderLine("Accuracy", PgnManager.GetValue("WhiteAccuracy"), PgnManager.GetValue("BlackAccuracy"));
		RenderSeparator();
		RenderLine("Best", _classCounts[White, Best], _classCounts[Black, Best]);
		RenderLine("Great", _classCounts[White, Great], _classCounts[Black, Great]);
		RenderLine("Good", _classCounts[White, Good], _classCounts[Black, Good]);
		RenderLine("Inaccuracy", _classCounts[White, Inaccuracy], _classCounts[Black, Inaccuracy]);
		RenderLine("Mistake", _classCounts[White, Mistake], _classCounts[Black, Mistake]);
		RenderLine("Blunder", _classCounts[White, Blunder], _classCounts[Black, Blunder]);
	}

	private readonly Font _font;
	private readonly Pen _curentPen;
	private readonly Pen _hoverPen;
	private readonly Brush _whiteBrush;
	private readonly Brush _blackBrush;
	private readonly List<TreeNode> _nodes;
	private readonly List<int> _evals;
	private readonly int[,] _classCounts;
	private Bitmap? _graph;
	private int _gap;
	private int _lineHeight;
	private int _hoveredIndex;
}

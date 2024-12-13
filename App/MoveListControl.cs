namespace Scabine.App;

using Scabine.Scenes;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using static Scabine.Core.Pieces;
using static Scabine.Core.Game;
using static Scabine.App.GraphicsHelper;
using Scabine.App.Prefs;
using System.CodeDom;

internal class MoveListControl : ScrollableContainer
{
	public MoveListControl()
	{
		MinSize = new Size(200, 200);
		_borderPen = new Pen(Color.Black);
		_font = new Font("Verdana", 16);
		_branchFont = new Font("Gadugi", 12);
		_gridPen = new Pen(Color.DarkGray);
		_linePen = new Pen(Color.DimGray);
		_backBrush = new SolidBrush(Color.Snow);
		_backgroundBrush = new SolidBrush(Color.LightGray);
		_foregroundBrush = new SolidBrush(Color.Black);
		_currentMoveBrush = new SolidBrush(Color.LightCyan);
		_hoveredMoveBrush = new SolidBrush(Color.LightBlue);
		_buttons = new SceneButton[5];
	}

	public override void Enter()
	{
		for (int i = 0; i < _buttons.Length; i++)
		{
			_buttons[i] = new SceneButton();
			AddSibling(_buttons[i]);
		}
		base.Enter();
	}

	public override void Update()
	{
		UpdateButtons();
		UpdateAutoPlay();
		UpdateMenu();
		UpdateMouse();
		UpdateKeyboard();
		UpdateTooltip();
		base.Update();
	}

	public override void Render(Graphics g)
	{
		RenderBackground(g);
		RenderTree(g);
		base.Render(g);
	}

	protected override void UpdatePosition()
	{
		base.UpdatePosition();
		int gap = 20;
		int width = Math.Max(1, Math.Min(ParentSize.Width, 500) - gap * 2);
		int height = Math.Max(1, ParentSize.Height - gap * 2);
		int buttonWidth = width / _buttons.Length;
		int buttonHeight = Math.Min(width / _buttons.Length, 50);
		Location = new Point((ParentSize.Width - width) / 2, (ParentSize.Height - height) / 2);
		Size = new Size(width, height - buttonHeight - gap);
		for (int i = 0; i < _buttons.Length; i++)
		{
			_buttons[i].Location = new Point((ParentSize.Width - buttonWidth * _buttons.Length) / 2 + i * buttonWidth, Location.Y + height - buttonHeight);
			_buttons[i].Size = new Size(buttonWidth, buttonHeight);
		}
		Rectangle bounds = GetRenderBounds();
		_numberWidth = Math.Min(60, Size.Width / 6);
		_moveWidth = (bounds.Width - _numberWidth) / 2;
		_numberWidth = bounds.Width - _moveWidth * 2;
		_rowHeight = _font.Height * 3 / 2;
		_padding = _rowHeight / 4;
		if (_currentRectangle != _previousRectangle && GameManager.GetGame().GetCurrentNode() != _hoveredNode)
		{
			ScrollHeight = Math.Max(Math.Min(_currentRectangle.Top - Size.Height / 2, VirtualHeight - Size.Height), 0);
		}
		_previousRectangle = _currentRectangle;
	}

	private void PopupMenu(TreeNode node)
	{
		_menuNode = node;
		ContextMenuStrip menu = MenuCreator.CreateContextMenu();
		MenuCreator.AddMenuLabel(menu, 1 + node.Rank / 2 + (node.Color == White ? "." : "...") + node.San);
		MenuCreator.AddMenuSeparator(menu);
		void AddOption(string text, Image image, Action action)
		{
			MenuCreator.AddMenuItem(menu, text, image, () => _menuAction = action);
		}
		void Promote()
		{
			GameManager.GetGame().PromoteNode(_menuNode);
		}
		void MoveUp()
		{
			GameManager.GetGame().SwapWithPreviousNode(_menuNode);
		}
		void MoveDown()
		{
			GameManager.GetGame().SwapWithNextNode(_menuNode);
		}
		void Expand()
		{
			_menuNode.IsCollapsed = false;
		}
		void Collapse()
		{
			_menuNode.IsCollapsed = true;
		}
		void Delete()
		{
			GameManager.GetGame().DeleteNode(_menuNode);
		}
		if (!_menuNode.IsMainLine)
		{
			AddOption("Make main line", MenuIcons.Promote, Promote);
			AddOption("Move up", MenuIcons.Up, MoveUp);
			AddOption("Move down", MenuIcons.Down, MoveDown);
			AddOption("Expand variations", MenuIcons.Expand, Expand);
			AddOption("Collapse variations", MenuIcons.Collapse, Collapse);
		}
		AddOption("Delete branch", MenuIcons.Delete, Delete);
		menu.Show(Cursor.Position);
	}

	private void UpdateButtons()
	{
		_buttons[0].Image = ButtonIcons.ToStart;
		_buttons[1].Image = ButtonIcons.Backward;
		_buttons[2].Image = _autoPlay ? ButtonIcons.Pause : ButtonIcons.Play;
		_buttons[3].Image = ButtonIcons.Forward;
		_buttons[4].Image = ButtonIcons.ToEnd;
		_buttons[0].Disabled = GameManager.IsAtStart();
		_buttons[1].Disabled = GameManager.IsAtStart();
		_buttons[2].Disabled = GameManager.IsAtEnd();
		_buttons[3].Disabled = GameManager.IsAtEnd();
		_buttons[4].Disabled = GameManager.IsAtEnd();
		if (_buttons[0].Clicked)
		{
			GameManager.StepBackward(MaxPly);
		}
		if (_buttons[1].Clicked)
		{
			GameManager.StepBackward(1);
		}
		if (_buttons[2].Clicked)
		{
			_autoPlay ^= true;
		}
		if (_buttons[3].Clicked)
		{
			GameManager.StepForward(1);
		}
		if (_buttons[4].Clicked)
		{
			GameManager.StepForward(MaxPly);
		}
	}

	private void UpdateAutoPlay()
	{
		double time = Time.GetTime();
		if (_autoPlay)
		{
			if (_autoPlayTime + Play.AutoPlayInterval / 1000.0 < time)
			{
				_autoPlayTime = time;
				GameManager.StepForward(1);
			}
		}
		if (GameManager.IsAtEnd())
		{
			_autoPlay = false;
		}
	}

	private void UpdateMouse()
	{
		if (_hoveredNode != null)
		{
			if (InputManager.IsLeftButtonReleased())
			{
				GameManager.GetGame().SetCurrentNode(_hoveredNode);
				_autoPlay = false;
			}
			if (InputManager.IsRightButtonReleased())
			{
				PopupMenu(_hoveredNode);
			}
		}
	}

	private void UpdateKeyboard()
	{
		if (InputManager.IsKeyRepeated(Keys.Left))
		{
			GameManager.StepBackward(1);
		}
		if (InputManager.IsKeyRepeated(Keys.Up))
		{
			GameManager.StepBackward(5);
		}
		if (InputManager.IsKeyRepeated(Keys.Right))
		{
			GameManager.StepForward(1);
		}
		if (InputManager.IsKeyRepeated(Keys.Down))
		{
			GameManager.StepForward(5);
		}
	}

	private void UpdateMenu()
	{
		if (_menuAction != null)
		{
			_menuAction();
			_menuAction = null;
			_menuNode = null;
		}
	}

	private void UpdateTooltip()
	{
		if (_hoveredNode?.Comment is string comment)
		{
			ToolTipManager.SetToolTip(comment);
		}
		else
		{
		}
	}

	private void RenderNode(Graphics g, TreeNode node, Rectangle rectangle)
	{
		if (node == GameManager.GetGame().GetCurrentNode())
		{
			_currentRectangle = rectangle;
		}
		if (!g.ClipBounds.IntersectsWith(rectangle))
		{
			return;
		}
		Point mousePosition = GetMousePosition() + new Size(0, ScrollHeight);
		if (ContainsMouse() && rectangle.Contains(mousePosition))
		{
			_hoveredNode = node;
			FillRectangle(g, _hoveredMoveBrush, rectangle);
		}
		if (node == GameManager.GetGame().GetCurrentNode())
		{
			FillRectangle(g, _currentMoveBrush, rectangle);
		}
	}

	private void RenderBackground(Graphics g)
	{
		FillRectangle(g, _backBrush, GetRenderBounds());
	}

	private void RenderTree(Graphics g)
	{
		_hoveredNode = null;
		int height = _branchFont.Height;
		int actualHeight = 0;
		int[] depthHeights = new int[MaxPly];
		void RenderSideLine(TreeNode node, int depth)
		{
			int padding = height / 4;
			int branchStart = height * depth - height / 2;
			int branchEnd = height * depth;
			int branchLength = node == node.Parent?.Children.Last() ? height / 2 : height;
			Point branchTop = new Point(branchStart, depthHeights[depth]);
			Point branchBottom = new Point(branchStart, actualHeight + branchLength + padding);
			Point branchLeft = new Point(branchStart, actualHeight + height / 2 + padding);
			Point branchRight = new Point(branchEnd - padding, actualHeight + height / 2 + padding);
			DrawLine(g, _linePen, branchTop, branchBottom);
			DrawLine(g, _linePen, branchLeft, branchRight);
			depthHeights[depth] = actualHeight;
			Rectangle rectangle = new Rectangle(branchEnd, actualHeight + padding, GetRenderBounds().Width - branchEnd, int.MaxValue);
			int x = 0, y = 0;
			Rectangle GetNextTextRect(string text)
			{
				Size size = Size.Round(g.MeasureString(text, _branchFont));
				if (!string.IsNullOrWhiteSpace(text) && x != 0 && x + size.Width > rectangle.Width)
				{
					x = 0;
					y += size.Height;
				}
				Point point = new Point(rectangle.X + x, rectangle.Y + y);
				x += size.Width;
				return new Rectangle(point, size);
			}
			void DrawMove(TreeNode node, string text)
			{
				Rectangle rectangle = GetNextTextRect(text);
				RenderNode(g, node, rectangle);
				DrawString(g, text, _branchFont, _foregroundBrush, rectangle, StringFormats.LeftAligned);
			}
			DrawMove(node, $"{node.Rank / 2 + 1}" + (node.Color == Black ? "..." : ".") + node.San + " ");
			while (!node.IsCollapsed && node.Children.Count == 1)
			{
				node = node.Children[0];
				DrawString(g, " ", _branchFont, _foregroundBrush, GetNextTextRect(" "), StringFormats.LeftAligned);
				DrawMove(node, node.Color == White ? $"{node.Rank / 2 + 1}." + node.San : node.San);
			}
			if (node.IsCollapsed && node.Children.Count != 0)
			{
				string ellipsisText = "[...]";
				DrawString(g, ellipsisText, _branchFont, _foregroundBrush, GetNextTextRect(ellipsisText), StringFormats.LeftAligned);
			}
			actualHeight += y + height + padding * 2;
			depthHeights[depth + 1] = actualHeight;
			if (!node.IsCollapsed)
			{
				for (int i = 0; i < node.Children.Count; i++)
				{
					RenderSideLine(node.Children[i], depth + 1);
				}
			}
		}
		void RenderMainLine(TreeNode node)
		{
			while (true)
			{
				if (node.Children.Count == 0)
				{
					return;
				}
				TreeNode next = node.Children[0];
				Rectangle rowRectangle = new Rectangle(0, actualHeight, Size.Width, _rowHeight);
				Rectangle numberRectangle = new Rectangle(0, actualHeight, _numberWidth, _rowHeight);
				Rectangle whiteRectangle = new Rectangle(_numberWidth, actualHeight, _moveWidth, _rowHeight);
				Rectangle blackRectangle = new Rectangle(_numberWidth + _moveWidth, actualHeight, _moveWidth, _rowHeight);
				actualHeight += _rowHeight;
				FillRectangle(g, _backgroundBrush, rowRectangle);
				DrawString(g, $"{next.Rank / 2 + 1}", _font, _foregroundBrush, numberRectangle, StringFormats.Centered);
				void DrawMove(TreeNode? node, string move, Rectangle rectangle)
				{
					if (node != null)
					{
						node.IsCollapsed = false;
						RenderNode(g, node, rectangle);
					}
					rectangle.Inflate(-_padding, 0);
					rectangle.Inflate(-_padding, 0);
					Size moveSize = Size.Round(g.MeasureString(move, _font));
					DrawString(g, move, _font, _foregroundBrush, rectangle, moveSize.Width >= rectangle.Width ? StringFormats.Centered : StringFormats.LeftAligned);
				}
				void DrawGrid()
				{
					DrawRectangle(g, _gridPen, numberRectangle);
					DrawRectangle(g, _gridPen, whiteRectangle);
					DrawRectangle(g, _gridPen, blackRectangle);
				}
				if (next.Color == White)
				{
					DrawMove(next, next.San, whiteRectangle);
				}
				if (next.Color == Black)
				{
					DrawMove(null, "...", whiteRectangle);
					DrawMove(next, next.San, blackRectangle);
				}
				DrawGrid();
				if (node.Children.Count > 1)
				{
					if (next.Color == White && next.Children.Count > 0)
					{
						DrawMove(null, "...", blackRectangle);
					}
					break;
				}
				node = next;
				if (node.Children.Count == 0)
				{
					return;
				}
				if (node.Color == Black)
				{
					continue;
				}
				next = node.Children[0];
				DrawMove(next, next.San, blackRectangle);
				DrawGrid();
				if (node.Children.Count > 1)
				{
					break;
				}
				node = next;
			}
			depthHeights[1] = actualHeight;
			for (int i = 1; i < node.Children.Count; i++)
			{
				RenderSideLine(node.Children[i], 1);
			}
			RenderMainLine(node.Children[0]);
		}
		TreeNode root = GameManager.GetGame().GetRootNode();
		RenderMainLine(root);
		VirtualHeight = actualHeight;
	}

	private readonly Font _font;
	private readonly Font _branchFont;
	private readonly Pen _gridPen;
	private readonly Pen _linePen;
	private readonly Brush _backBrush;
	private readonly Brush _backgroundBrush;
	private readonly Brush _foregroundBrush;
	private readonly Brush _currentMoveBrush;
	private readonly Brush _hoveredMoveBrush;
	private int _moveWidth;
	private int _numberWidth;
	private int _rowHeight;
	private int _padding;
	private TreeNode? _hoveredNode;
	private TreeNode? _menuNode;
	private Action? _menuAction;
	private Rectangle _currentRectangle;
	private Rectangle _previousRectangle;
	private SceneButton[] _buttons;
	private bool _autoPlay;
	private double _autoPlayTime;
}

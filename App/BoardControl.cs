namespace Scabine.App;

using Scabine.App.Dialogs;
using Scabine.App.Prefs;
using Scabine.Core;
using Scabine.Scenes;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using static Scabine.Core.Pieces;
using static Scabine.Core.Squares;
using static Scabine.Core.Move;

internal class BoardControl : SceneNode
{
	public BoardControl()
	{
		MinSize = new Size(160, 160);
		_borderPen = new Pen(Color.Black, 1);
		_selectedBrush = new SolidBrush(Colors.SelectedSquare);
		_highlightedBrush = new SolidBrush(Colors.HighlightedSquare);
		_legalBrush = new SolidBrush(Colors.LegalSquare);
		_checkBrush = new SolidBrush(Colors.AttackedSquare);
		_renderLastMove = true;
		_gapSize = 0;
		_squareSize = 0;
		_boardSize = 0;
		_selectedSquare = NoSquare;
		_hoveredSquare = NoSquare;
	}

	public override void Update()
	{
		bool canClick = Play.CanMoveByClicking();
		bool canDrag = Play.CanMoveByDragging();
		int square = GetSquareUnderMouse();
		if (square != NoSquare && InputManager.IsLeftButtonPressed())
		{
			_hoveredSquare = NoSquare;
			int piece = GameManager.GetGame().GetPiece(square);
			if (IsPiece(piece) && GetPieceColor(piece) == GameManager.GetGame().GetCurrentColor())
			{
				if (_selectedSquare == square)
				{
					_hoveredSquare = _selectedSquare;
				}
				_selectedSquare = _selectedSquare == square && !canDrag ? NoSquare : square;
				_dragging = canDrag;
				_draggedLocation = InputManager.GetMousePosition();
			}
			else if (_selectedSquare != NoSquare)
			{
				Move? move = PlayMove(_selectedSquare, square);
				AnimationManager.AnimateMove(move, AnimationDirection.Forward);
				_selectedSquare = NoSquare;
			}
		}
		if (InputManager.IsLeftButtonReleased() && canDrag)
		{
			if (square == _hoveredSquare && square != _selectedSquare)
			{
				PlayMove(_selectedSquare, square);
			}
			if (square == NoSquare || _hoveredSquare != NoSquare)
			{
				_selectedSquare = NoSquare;
			}
			_dragging = false;
		}
		if (InputManager.IsLeftButtonReleased() && !canClick)
		{
			_selectedSquare = NoSquare;
		}
		if (ContainsMouse())
		{
			int scroll = InputManager.GetMouseScroll();
			if (scroll < 0)
			{
				GameManager.StepForward(1);
			}
			if (scroll > 0)
			{
				GameManager.StepBackward(1);
			}
		}
		if (square != _selectedSquare)
		{
			_hoveredSquare = square;
		}
		_draggedLocation = InputManager.GetMousePosition();
		if (MatchManager.IsBoardDisabled() || GameManager.IsDirty())
		{
			_selectedSquare = NoSquare;
			_hoveredSquare = NoSquare;
			_dragging = false;
		}
		base.Update();
	}

	public override void Render(Graphics g)
	{
		RenderSquares(g);
		RenderPieces(g);
		RenderLegalSquares(g);
		RenderCheck(g);
		RenderBorder(g);
		RenderCoordinates(g);
		base.Render(g);
	}

	public void RenderBoardOnly(Graphics g)
	{
		bool renderLastMove = _renderLastMove;
		int selectedSquare = _selectedSquare;
		int hoveredSquare = _hoveredSquare;
		bool dragging = _dragging;
		_renderLastMove = false;
		_selectedSquare = NoSquare;
		_hoveredSquare = NoSquare;
		_dragging = false;
		RenderSquares(g);
		RenderPieces(g);
		RenderBorder(g);
		RenderCoordinates(g);
		base.Render(g);
		_renderLastMove = renderLastMove;
		_selectedSquare = selectedSquare;
		_hoveredSquare = hoveredSquare;
		_dragging = dragging;
	}

	public void RenderGrabbedPiece(Graphics g)
	{
		if (_dragging)
		{
			PieceImages.SetScaledSize(_squareSize);
			Image? pieceImage = PieceImages.GetScaledImage(GameManager.GetGame().GetPiece(_selectedSquare));
			if (pieceImage != null)
			{
				g.DrawImage(pieceImage, Point.Round(_draggedLocation) - new Size(_squareSize / 2, _squareSize / 2));
			}
		}
	}

	protected override void UpdatePosition()
	{
		Size parentSize = Parent != null ? Parent.Size : new Size(0, 0);
		int sideLength = Math.Min(parentSize.Width, parentSize.Height);
		_gapSize = sideLength / 15;
		if (!Board.ShowCoordinates)
		{
			_gapSize /= 2;
		}
		sideLength -= _gapSize * 2;
		_squareSize = Math.Max(sideLength / 8, 1);
		_boardSize = _squareSize * 8;
		Location = new Point((parentSize.Width - _boardSize) / 2, (parentSize.Height - _boardSize) / 2);
		Size = new Size(_boardSize, _boardSize);
	}

	private void RenderSquares(Graphics g)
	{
		BoardImages.SetScaledSize(_boardSize);
		Image? boardImage = BoardImages.GetScaledImage();
		if (boardImage != null)
		{
			g.DrawImage(boardImage, 0, 0);
		}
		Move? lastMove = _renderLastMove ? GameManager.GetGame().GetLastMove() : null;
		for (int rank = 0; rank < RankCount; rank++)
		{
			for (int file = 0; file < FileCount; file++)
			{
				int square = MakeSquare(rank, file);
				Rectangle rectangle = GetSquareRectangle(square);
				if (square == lastMove?.SourceSquare || square == lastMove?.TargetSquare)
				{
					if (Board.HighlightMoves)
					{
						g.FillRectangle(_highlightedBrush, rectangle);
					}
				}
				if (square == _selectedSquare)
				{
					if (Board.HighlightSelection)
					{
						g.FillRectangle(_selectedBrush, rectangle);
					}
				}
			}
		}
	}

	private void RenderPieces(Graphics g)
	{
		using (new CompositingQualityChanger(g, CompositingQuality.HighQuality))
		{
			for (int rank = 0; rank < RankCount; rank++)
			{
				for (int file = 0; file < FileCount; file++)
				{
					int square = MakeSquare(rank, file);
					if (_dragging && square == _selectedSquare)
					{
						continue;
					}
					if (AnimationManager.IsAnimating() && AnimationManager.AnimatedMove is Move move)
					{
						if (square == move.SourceSquare || square == move.TargetSquare)
						{
							continue;
						}
						if (move.Type == CastlingMove)
						{
							bool skipSquare = false;
							switch (move.TargetSquare)
							{
								case G1:
									skipSquare = square == H1 || square == F1;
									break;
								case C1:
									skipSquare = square == A1 || square == D1;
									break;
								case G8:
									skipSquare = square == H8 || square == F8;
									break;
								case C8:
									skipSquare = square == A8 || square == D8;
									break;
							}
							if (skipSquare)
							{
								continue;
							}
						}
					}
					int piece = GameManager.GetGame().GetPiece(square);
					if (IsPiece(piece))
					{
						PieceImages.SetScaledSize(_squareSize);
						Image? pieceImage = PieceImages.GetScaledImage(piece);
						if (pieceImage != null)
						{
							Rectangle rectangle = GetSquareRectangle(square);
							g.DrawImage(pieceImage, rectangle);
						}
					}
				}
			}
			if (AnimationManager.IsAnimating() && AnimationManager.AnimatedMove != null)
			{
				Move move = AnimationManager.AnimatedMove.Value;
				double progress = AnimationManager.GetAnimationProgress();
				void DrawPiece(int piece, PointF point)
				{
					PieceImages.SetScaledSize(_squareSize);
					Image? pieceImage = PieceImages.GetScaledImage(piece);
					if (pieceImage != null)
					{
						g.DrawImage(pieceImage, point);
					}
				}
				void DrawAnimatedPiece(int piece, int sourceSquare, int targetSquare)
				{
					Rectangle sourceRectangle = GetSquareRectangle(sourceSquare);
					Rectangle targetRectangle = GetSquareRectangle(targetSquare);
					double pieceX = 0;
					double pieceY = 0;
					switch (AnimationManager.AnimationDirection)
					{
						case AnimationDirection.Forward:
							pieceX = targetRectangle.X * progress + sourceRectangle.X * (1 - progress);
							pieceY = targetRectangle.Y * progress + sourceRectangle.Y * (1 - progress);
							break;
						case AnimationDirection.Backward:
							pieceX = sourceRectangle.X * progress + targetRectangle.X * (1 - progress);
							pieceY = sourceRectangle.Y * progress + targetRectangle.Y * (1 - progress);
							break;
					}
					DrawPiece(piece, new PointF((float)pieceX, (float)pieceY));
				}
				switch (move.Type)
				{
					case EnPassantMove:
						int capturedColor = GetPieceColor(move.SourcePiece) ^ 1;
						int capturedPawn = MakePiece(capturedColor, Pawn);
						int capturedSquare = move.TargetSquare + GetMoveDirection(capturedColor);
						DrawPiece(capturedPawn, GetSquareRectangle(capturedSquare).Location);
						break;
					case CastlingMove:
						int moveColor = GetPieceColor(move.SourcePiece);
						int castlingRook = MakePiece(moveColor, Rook);
						switch (move.TargetSquare)
						{
							case G1:
								DrawAnimatedPiece(castlingRook, H1, F1);
								break;
							case C1:
								DrawAnimatedPiece(castlingRook, A1, D1);
								break;
							case G8:
								DrawAnimatedPiece(castlingRook, H8, F8);
								break;
							case C8:
								DrawAnimatedPiece(castlingRook, A8, D8);
								break;
						}
						break;
					default:
						DrawPiece(move.TargetPiece, GetSquareRectangle(move.TargetSquare).Location);
						break;
				}
				DrawAnimatedPiece(move.SourcePiece, move.SourceSquare, move.TargetSquare);
			}
		}
	}

	private void RenderLegalSquares(Graphics g)
	{
		if (!Board.ShowLegalMoves)
		{
			return;
		}
		if (_selectedSquare == NoSquare)
		{
			return;
		}
		int squareUnderMouse = GetSquareUnderMouse();
		ReadOnlySpan<Move> legalMoves = GameManager.GetLegalMoves();
		bool[] visited = new bool[SquareCount];
		foreach (Move move in legalMoves)
		{
			if (move.SourceSquare != _selectedSquare)
			{
				continue;
			}
			int square = move.TargetSquare;
			if (visited[square])
			{
				continue;
			}
			visited[square] = true;
			Rectangle rectangle = GetSquareRectangle(square);
			if (square == squareUnderMouse)
			{
				g.FillRectangle(_legalBrush, rectangle);
			}
			else
			{
				rectangle.Inflate(-_squareSize / 3, -_squareSize / 3);
				using (new SmoothingModeChanger(g, SmoothingMode.HighQuality))
				{
					g.FillEllipse(_legalBrush, rectangle);
				}
			}
		}
	}

	private void RenderCheck(Graphics g)
	{
		if (!Board.HighlightCheck)
		{
			return;
		}
		if (!GameManager.GetGame().IsCheck())
		{
			return;
		}
		int kingSquare = GameManager.GetGame().GetCurrentPosition().GetKingPosition();
		if (_selectedSquare == kingSquare)
		{
			return;
		}
		Rectangle rectangle = GetSquareRectangle(kingSquare);
		g.FillRectangle(_checkBrush, rectangle);
	}

	private void RenderBorder(Graphics g)
	{
		g.DrawRectangle(_borderPen, SelfBounds);
	}

	private void RenderCoordinates(Graphics g)
	{
		if (!Board.ShowCoordinates)
		{
			return;
		}
		Font font = new Font("Arial", Math.Max(1, _gapSize / 3));
		for (int rank = 0; rank < RankCount; rank++)
		{
			int x = -_gapSize / 2;
			int y = _squareSize / 2 + _squareSize * (Board.Flipped ? 7 - rank : rank);
			for (int i = 0; i < 2; i++)
			{
				g.DrawString(GetRankChar(rank).ToString(), font, Brushes.Black, new Point(x, y), StringFormats.Centered);
				x = _boardSize - x;
			}
		}
		for (int file = 0; file < FileCount; file++)
		{
			int x = _squareSize / 2 + _squareSize * (Board.Flipped ? 7 - file : file);
			int y = _boardSize + _gapSize / 2;
			for (int i = 0; i < 2; i++)
			{
				g.DrawString(GetFileChar(file).ToString(), font, Brushes.Black, new Point(x, y), StringFormats.Centered);
				y = _boardSize - y;
			}
		}
	}

	private Rectangle GetSquareRectangle(int square)
	{
		if (Board.Flipped)
		{
			square = MirrorSquare(square);
		}
		int rank = GetSquareRank(square);
		int file = GetSquareFile(square);
		return new Rectangle(file * _squareSize, rank * _squareSize, _squareSize, _squareSize);
	}

	private int GetSquareUnderMouse()
	{
		if (!ContainsMouse())
		{
			return NoSquare;
		}
		Point mousePosition = GetMousePosition();
		int rank = mousePosition.Y / _squareSize;
		int file = mousePosition.X / _squareSize;
		if (rank >= 0 && rank < RankCount && file >= 0 && file < FileCount)
		{
			int square = MakeSquare(rank, file);
			return Board.Flipped ? MirrorSquare(square) : square;
		}
		else
		{
			return NoSquare;
		}
	}

	private Move? PlayMove(int sourceSquare, int targetSquare)
	{
		if (sourceSquare < 0 || sourceSquare >= SquareCount || targetSquare < 0 || targetSquare >= SquareCount)
		{
			return null;
		}
		int movingPiece = GameManager.GetGame().GetPiece(sourceSquare);
		for (int color = 0; color < ColorCount; color++)
		{
			if (movingPiece == MakePiece(color, Pawn) && GetSquareRank(targetSquare) == GetPromotionRank(color))
			{
				if (Play.AutoQueen)
				{
					return GameManager.TryPlayMove(sourceSquare, targetSquare, Queen);
				}
				else
				{
					PromotionDialog dialog = new PromotionDialog(_squareSize * 5 / 6, color, sourceSquare, targetSquare);
					DialogHelper.ShowCancellableDialog(dialog);
					return dialog.PlayedMove;
				}
			}
		}
		return GameManager.TryPlayMove(sourceSquare, targetSquare, None);
	}

	private readonly Pen _borderPen;
	private readonly Brush _selectedBrush;
	private readonly Brush _highlightedBrush;
	private readonly Brush _legalBrush;
	private readonly Brush _checkBrush;
	private bool _renderLastMove;
	private int _gapSize;
	private int _squareSize;
	private int _boardSize;
	private int _selectedSquare;
	private int _hoveredSquare;
	private bool _dragging;
	private Point _draggedLocation;
}

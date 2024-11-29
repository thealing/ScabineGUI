namespace Scabine.App.Dialogs;

using Scabine.App.Prefs;
using Scabine.Core;
using Scabine.Scenes;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using static Scabine.App.Dialogs.DialogCreator;
using static Scabine.Core.Pieces;
using static Scabine.Core.Squares;
using static Scabine.Core.Position;
using System.Xml.Linq;
using System.Text;

internal class PositionSetupDialog : BaseDialog
{
	public bool Success => _success;

	public PositionSetupDialog()
	{
		ClientSize = new Size(900, 510);
		Text = "Position Setup";
		Font = new Font("Segoe UI", 10);
		_boardPicture = AddPictureBox(Controls, 20, 20, 400, 400, null, PaintBoard, null);
		_boardPicture.MouseMove += OnMouseMove;
		_boardPicture.MouseDown += OnMouseDown;
		_boardPicture.MouseUp += OnMouseUp;
		_squareSize = Math.Min(_boardPicture.Width, _boardPicture.Height) / 8;
		GroupBox colorGroup = AddGroupBox(Controls, "Color To Move", 450, 20, 120, 100);
		_whiteToMove = AddRadioButton(colorGroup.Controls, "White", 20, 30, 60, 20, Update);
		_blackToMove = AddRadioButton(colorGroup.Controls, "Black", 20, 60, 60, 20, Update);
		GroupBox castlingGroup = AddGroupBox(Controls, "Castling Permissions", 590, 20, 290, 100);
		_castleWK = AddCheckBox(castlingGroup.Controls, "White Kingside", 20, 30, 120, 20, Update);
		_castleWQ = AddCheckBox(castlingGroup.Controls, "White Queenside", 150, 30, 130, 20, Update);
		_castleBK = AddCheckBox(castlingGroup.Controls, "Black Kingside", 20, 60, 120, 20, Update);
		_castleBQ = AddCheckBox(castlingGroup.Controls, "Black Queenside", 150, 60, 130, 20, Update);
		GroupBox piecesGroup = AddGroupBox(Controls, "Edit Pieces", 450, 140, 410, 170);
		_moveButton = AddPictureBox(piecesGroup.Controls, 20, 30, 50, 50, ButtonIcons.MovePieces, PaintButton, ClickButton);
		_eraseButton = AddPictureBox(piecesGroup.Controls, 20, 100, 50, 50, ButtonIcons.ErasePieces, PaintButton, ClickButton);
		_pieceButtons = new PictureBox[PieceCount];
		for (int color = White; color <= Black; color++)
		{
			for (int type = Pawn; type <= King; type++)
			{
				int piece = MakePiece(color, type);
				PieceImages.SetScaledSize(160);
				_pieceButtons[piece] = AddPictureBox(piecesGroup.Controls, 40 + type * 50, 30 + color * 70, 51, 50, PieceImages.GetScaledImage(piece), PaintButton, ClickButton);
			}
		}
		_selection = _moveButton;
		_selection.BackColor = Color.LightGreen;
		_flipped = Board.Flipped;
		CheckBox flipCheckBox = AddCheckButton(Controls, "Flip Board", 460, 340, 110, 30, FlipBoard);
		flipCheckBox.Checked = _flipped;
		AddButton(Controls, "Clear Board", 600, 340, 110, 30, ClearBoard);
		AddButton(Controls, "Reset Board", 740, 340, 110, 30, ResetBoard);
		AddButton(Controls, "Cancel", 20, 440, 120, 50, Cancel);
		_doneButton = AddButton(Controls, "Done", 760, 440, 120, 50, Done);
		_statusLabel = AddLabel(Controls, "Status", 460, 390, 400, 30);
		_pieces = new int[SquareCount];
		SetGame(GameManager.GetGame());
		_game = new Game();
		Update(null, new EventArgs());
	}

	public string GetFen()
	{
		StringBuilder builder = new StringBuilder();
		for (int rank = 0; rank < RankCount; rank++)
		{
			int counter = 0;
			for (int file = 0; file < FileCount; file++)
			{
				int square = MakeSquare(rank, file);
				if (IsPiece(_pieces[square]))
				{
					if (counter > 0)
					{
						builder.Append(counter);
						counter = 0;
					}
					builder.Append(GetPieceChar(_pieces[square]));
				}
				else
				{
					counter++;
				}
			}
			if (counter > 0)
			{
				builder.Append(counter);
			}
			if (rank + 1 != RankCount)
			{
				builder.Append('/');
			}
		}
		builder.Append(' ');
		builder.Append(GetColorChar(_colorToMove));
		builder.Append(' ');
		if (_castlingPerms == 0)
		{
			builder.Append('-');
		}
		else
		{
			if ((_castlingPerms & CastlingWK) != 0 && _pieces[E1] == WhiteKing && _pieces[H1] == WhiteRook)
			{
				builder.Append('K');
			}
			if ((_castlingPerms & CastlingWQ) != 0 && _pieces[E1] == WhiteKing && _pieces[A1] == WhiteRook)
			{
				builder.Append('Q');
			}
			if ((_castlingPerms & CastlingBK) != 0 && _pieces[E8] == BlackKing && _pieces[H8] == BlackRook)
			{
				builder.Append('k');
			}
			if ((_castlingPerms & CastlingBQ) != 0 && _pieces[E8] == BlackKing && _pieces[A8] == BlackRook)
			{
				builder.Append('q');
			}
		}
		builder.Append(' ');
		builder.Append('-');
		builder.Append(' ');
		builder.Append(0);
		builder.Append(' ');
		builder.Append(1);
		return builder.ToString();
	}

	private void SetGame(Game game)
	{
		Array.Copy(game.GetBoard(), _pieces, SquareCount);
		_colorToMove = game.GetCurrentColor();
		switch (_colorToMove)
		{
			case White:
				_whiteToMove.Checked = true;
				break;
			case Black:
				_blackToMove.Checked = true;
				break;
		}
		_castlingPerms = game.GetCurrentPosition().CastlingPerms;
		_castleWK.Checked = (_castlingPerms & CastlingWK) != 0;
		_castleWQ.Checked = (_castlingPerms & CastlingWQ) != 0;
		_castleBK.Checked = (_castlingPerms & CastlingBK) != 0;
		_castleBQ.Checked = (_castlingPerms & CastlingBQ) != 0;
	}

	private void Cancel(object? sender, EventArgs e)
	{
		Close();
	}

	private void Done(object? sender, EventArgs e)
	{
		if (_valid)
		{
			_success = true;
			Board.Flipped = _flipped;
			Close();
		}
	}

	private void FlipBoard(object? sender, EventArgs e)
	{
		_flipped ^= true;
		_boardPicture.Refresh();
	}

	private void ClearBoard(object? sender, EventArgs e)
	{
		Array.Fill(_pieces, None);
		_boardPicture.Refresh();
	}

	private void ResetBoard(object? sender, EventArgs e)
	{
		SetGame(new Game());
		_boardPicture.Refresh();
	}

	private void ClickButton(object? sender, EventArgs e)
	{
		if (sender is PictureBox current)
		{
			PictureBox previous = _selection;
			_selection.BackColor = SystemColors.Control;
			_selection = current;
			_selection.BackColor = Color.LightGreen;
			current.Refresh();
			previous.Refresh();
		}
	}

	private void OnMouseMove(object? sender, MouseEventArgs e)
	{
		_dragLocation = e.Location;
		_hoveredSquare = NoSquare;
		if (sender is PictureBox pictureBox)
		{
			if (pictureBox.ClientRectangle.Contains(_dragLocation))
			{
				_hoveredSquare = MakeSquare(e.Y / _squareSize, e.X / _squareSize);
				if (_flipped)
				{
					_hoveredSquare = MirrorSquare(_hoveredSquare);
				}
			}
		}
		if (_mouseDown)
		{
			UpdateMouse();
		}
		
	}

	private void OnMouseDown(object? sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Right)
		{
			if (_hoveredSquare != NoSquare)
			{
				_pieces[_hoveredSquare] = None;
				_boardPicture.Refresh();
			}
			return;
		}
		if (e.Button != MouseButtons.Left)
		{
			return;
		}
		_mouseDown = true;
		if (_selection == _moveButton)
		{
			int square = MakeSquare(e.Y / _squareSize, e.X / _squareSize);
			if (_flipped)
			{
				square = MirrorSquare(square);
			}
			_grappedPiece = _pieces[square];
			_pieces[square] = None;
			_boardPicture.Refresh();
		}
		else
		{
			_grappedPiece = None;
		}
		UpdateMouse();
	}

	private void OnMouseUp(object? sender, MouseEventArgs e)
	{
		_mouseDown = false;
		if (IsPiece(_grappedPiece) && _hoveredSquare != NoSquare)
		{
			_pieces[_hoveredSquare] = _grappedPiece;
			_grappedPiece = None;
		}
		if (sender is PictureBox pictureBox)
		{
			if (!pictureBox.ClientRectangle.Contains(_dragLocation))
			{
				_grappedPiece = None;
				_hoveredSquare = NoSquare;
			}
		}
		_boardPicture.Refresh();
	}

	private void PaintBoard(object? sender, PaintEventArgs e)
	{
		Graphics g = e.Graphics;
		if (sender is PictureBox pictureBox)
		{
			BoardImages.SetScaledSize(_squareSize * 8);
			Image? boardImage = BoardImages.GetScaledImage();
			if (boardImage != null)
			{
				g.DrawImage(boardImage, 0, 0);
			}
			for (int rank = 0; rank < RankCount; rank++)
			{
				for (int file = 0; file < FileCount; file++)
				{
					int square = MakeSquare(rank, file);
					int piece = _pieces[_flipped ? MirrorSquare(square) : square];
					if (IsPiece(piece))
					{
						PieceImages.SetScaledSize(_squareSize);
						Image? pieceImage = PieceImages.GetScaledImage(piece);
						if (pieceImage != null)
						{
							g.DrawImage(pieceImage, file * _squareSize, rank * _squareSize);
						}
					}
				}
			}
			if (IsPiece(_grappedPiece))
			{
				PieceImages.SetScaledSize(_squareSize);
				Image? pieceImage = PieceImages.GetScaledImage(_grappedPiece);
				if (pieceImage != null)
				{
					g.DrawImage(pieceImage, _dragLocation - new Size(_squareSize / 2, _squareSize / 2));
				}
			}
			g.DrawRectangle(Pens.Black, 0, 0, pictureBox.Width - 1, pictureBox.Height - 1);
		}
		Update(sender, e);
	}

	private void PaintButton(object? sender, PaintEventArgs e)
	{
		Graphics g = e.Graphics;
		if (sender is PictureBox pictureBox)
		{
			g.DrawRectangle(Pens.DarkGray, 0, 0, pictureBox.Width - 1, pictureBox.Height - 1);
		}
	}

	private void UpdateMouse()
	{
		if (_selection == _moveButton)
		{
			if (IsPiece(_grappedPiece))
			{
				_boardPicture.Refresh();
			}
		}
		if (_selection == _eraseButton)
		{
			if (_hoveredSquare != NoSquare && IsPiece(_pieces[_hoveredSquare]))
			{
				_pieces[_hoveredSquare] = None;
				_boardPicture.Refresh();
			}
		}
		int selectedPiece = Array.IndexOf(_pieceButtons, _selection);
		if (selectedPiece != -1 && _hoveredSquare != NoSquare)
		{
			_pieces[_hoveredSquare] = selectedPiece;
			_boardPicture.Refresh();
		}
	}

	private void Update(object? sender, EventArgs e)
	{
		if (sender == _whiteToMove)
		{
			_colorToMove = White;
		}
		if (sender == _blackToMove)
		{
			_colorToMove = Black;
		}
		if (sender == _castleWK)
		{
			_castlingPerms ^= CastlingWK;
		}
		if (sender == _castleWQ)
		{
			_castlingPerms ^= CastlingWQ;
		}
		if (sender == _castleBK)
		{
			_castlingPerms ^= CastlingBK;
		}
		if (sender == _castleBQ)
		{
			_castlingPerms ^= CastlingBQ;
		}
		if (!IsPiece(_grappedPiece))
		{
			_game.SetFen(GetFen());
			_valid = ValidateStatus();
			_doneButton.Enabled = _valid;
		}
	}

	private bool ValidateStatus()
	{
		_statusLabel.ForeColor = Color.Red;
		int whiteKingCount = _pieces.Count(piece => piece == WhiteKing);
		int blackKingCount = _pieces.Count(piece => piece == BlackKing);
		if (whiteKingCount == 0)
		{
			_statusLabel.Text = "Status: No white king";
			return false;
		}
		if (whiteKingCount > 1)
		{
			_statusLabel.Text = "Status: Multiple white kings";
			return false;
		}
		if (blackKingCount == 0)
		{
			_statusLabel.Text = "Status: No black king";
			return false;
		}
		if (blackKingCount > 1)
		{
			_statusLabel.Text = "Status: Multiple black kings";
			return false;
		}
		int whiteKingSquare = Array.IndexOf(_pieces, WhiteKing);
		int blackKingSquare = Array.IndexOf(_pieces, BlackKing);
		if (Math.Abs(GetSquareRank(whiteKingSquare) - GetSquareRank(blackKingSquare)) <= 1 && Math.Abs(GetSquareFile(whiteKingSquare) - GetSquareFile(blackKingSquare)) <= 1)
		{
			_statusLabel.Text = "Status: Kings are too close";
			return false;
		}
		for (int file = 0; file < FileCount; file++)
		{
			if (_pieces[MakeSquare(Rank1, file)] == WhitePawn)
			{
				_statusLabel.Text = "Status: White pawn on the first rank";
				return false;
			}
			if (_pieces[MakeSquare(Rank8, file)] == WhitePawn)
			{
				_statusLabel.Text = "Status: White pawn on the last rank";
				return false;
			}
			if (_pieces[MakeSquare(Rank8, file)] == BlackPawn)
			{
				_statusLabel.Text = "Status: Black pawn on the first rank";
				return false;
			}
			if (_pieces[MakeSquare(Rank1, file)] == BlackPawn)
			{
				_statusLabel.Text = "Status: Black pawn on the last rank";
				return false;
			}
		}
		if (_game.GetCurrentPosition().IsInCheck(_colorToMove ^ 1))
		{
			_statusLabel.Text = $"Status: The {(_colorToMove == White ? "black" : "white")} king can be captured";
			return false;
		}
		_statusLabel.ForeColor = Color.Green;
		_statusLabel.Text = "Status: OK";
		return true;
	}

	private readonly PictureBox _boardPicture;
	private readonly Label _statusLabel;
	private readonly int[] _pieces;
	private readonly int _squareSize;
	private readonly PictureBox _moveButton;
	private readonly PictureBox _eraseButton;
	private readonly PictureBox[] _pieceButtons;
	private readonly RadioButton _whiteToMove;
	private readonly RadioButton _blackToMove;
	private readonly CheckBox _castleWK;
	private readonly CheckBox _castleWQ;
	private readonly CheckBox _castleBK;
	private readonly CheckBox _castleBQ;
	private readonly Button _doneButton;
	private readonly Game _game;
	private PictureBox _selection;
	private bool _mouseDown;
	private int _grappedPiece;
	private int _hoveredSquare;
	private Point _dragLocation;
	private bool _flipped;
	private int _colorToMove;
	private int _castlingPerms;
	private bool _valid;
	private bool _success;
}

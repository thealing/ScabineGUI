namespace Scabine.App.Dialogs;

using Scabine.App.Prefs;
using Scabine.Core;
using Scabine.Scenes;
using System;
using System.Drawing;
using System.Security.Policy;
using System.Windows.Forms;
using static Scabine.Core.Pieces;

internal class PromotionDialog : Form
{
	public PromotionDialog(int size, int color, int sourceSquare, int targetSquare)
	{
		FormBorderStyle = FormBorderStyle.None;
		StartPosition = FormStartPosition.Manual;
		MinimumSize = new Size(1, 1);
		Size = new Size(size, size * 4);
		BackColor = Color.White;
		bool upsideDown = color == Black ^ Board.Flipped;
		for (int i = 0; i < 4; i++)
		{
			int promotionType = Queen - i;
			PieceImages.SetScaledSize(size - 1);
			Image? pieceImage = PieceImages.GetScaledImage(MakePiece(color, promotionType));
			if (pieceImage != null)
			{
				Button pieceButton = new Button
				{
					Size = new Size(size, size),
					Location = new Point(0, upsideDown ? size * (3 - i) : size * i),
					BackgroundImage = pieceImage,
					FlatStyle = FlatStyle.Flat,
					BackColor = Color.White
				};
				pieceButton.FlatAppearance.BorderSize = 0;
				pieceButton.MouseEnter += (s, e) =>
				{
					pieceButton.BackColor = Color.LightGray;
				};
				pieceButton.MouseLeave += (s, e) =>
				{
					pieceButton.BackColor = Color.White;
				};
				pieceButton.Click += (s, e) =>
				{
					GameManager.TryPlayMove(sourceSquare, targetSquare, promotionType);
					Close();
				};
				Controls.Add(pieceButton);
			}
		}
		HandleCreated += (s, e) => Location = Cursor.Position - new Size(size / 2, size / 2 + (upsideDown ? size * 3 : 0));
	}
}

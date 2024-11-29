namespace Scabine.App.Menus;

using Scabine.App;
using Scabine.App.Dialogs;
using Scabine.App.Prefs;
using Scabine.Scenes;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Windows.Forms;

internal static class BoardMenu
{
	public static void Init(ToolStripMenuItem menu)
	{
		MenuCreator.AddSubMenuItem(menu, "Flip board", null, FlipBoard, Keys.Control | Keys.F);
		MenuCreator.AddSubMenuSeparator(menu);
		MenuCreator.AddSubMenuItem(menu, "Set start position", null, SetStartPosition);
		MenuCreator.AddSubMenuItem(menu, "Set current position", null, SetCurrentPosition);
		MenuCreator.AddSubMenuItem(menu, "Set custom position", null, SetCustomPosition);
		MenuCreator.AddSubMenuItem(menu, "Set position FEN", null, SetCustomFen);
		MenuCreator.AddSubMenuSeparator(menu);
		MenuCreator.AddSubMenuItem(menu, "Copy FEN", null, CopyFen);
		MenuCreator.AddSubMenuItem(menu, "Save FEN", null, SaveFen);
		MenuCreator.AddSubMenuItem(menu, "Load FEN", null, LoadFen);
		MenuCreator.AddSubMenuSeparator(menu);
		MenuCreator.AddSubMenuItem(menu, "Export image", null, ExportImage);
		MenuCreator.AddSubMenuItem(menu, "Import image", null, ImportImage);
	}

	private static void FlipBoard()
	{
		Board.Flipped ^= true;
	}

	private static void SetStartPosition()
	{
		if (!PgnManager.SaveBackup())
		{
			return;
		}
		PgnManager.NewGame();
	}

	private static void SetCurrentPosition()
	{
		if (!PgnManager.SaveBackup())
		{
			return;
		}
		string fen = GameManager.GetGame().GetFen();
		PgnManager.NewGame(fen);
	}

	private static void SetCustomPosition()
	{
		if (!PgnManager.SaveBackup())
		{
			return;
		}
		PositionSetupDialog dialog = new PositionSetupDialog();
		DialogHelper.ShowDialog(dialog);
		if (dialog.Success)
		{
			PgnManager.NewGame(dialog.GetFen());
		}
	}

	private static void SetCustomFen()
	{
		if (!PgnManager.SaveBackup())
		{
			return;
		}
		FenDialog dialog = new FenDialog();
		DialogHelper.ShowDialog(dialog);
	}

	private static void CopyFen()
	{
		string fen = GameManager.GetGame().GetFen();
		Clipboard.SetText(fen);
	}

	private static void SaveFen()
	{
		string? path = FileChooser.ChooseSaveFile("Save Position", "Fen", "fen", "Position");
		if (path != null)
		{
			string content = GameManager.GetGame().GetFen() + "\n";
			File.WriteAllText(path, content);
		}
	}

	private static void LoadFen()
	{
		if (!PgnManager.SaveBackup())
		{
			return;
		}
		string? path = FileChooser.ChooseOpenFile("Open Position", "Fen", "fen");
		if (path != null)
		{
			string content = File.ReadAllText(path);
			PgnManager.NewGame(content);
		}
	}

	private static void ExportImage()
	{
		Scene? scene = SceneManager.GetScene();
		if (scene == null)
		{
			return;
		}
		BoardControl? board = scene.FindNodeByType<BoardControl>();
		if (board?.Parent == null)
		{
			return;
		}
		SceneNode boardContainer = board.Parent;
		Size boardSize = boardContainer.Size;
		int imageSize = Math.Min(boardSize.Width, boardSize.Height);
		using (Bitmap bitmap = new Bitmap(imageSize, imageSize))
		{
			using (Graphics g = Graphics.FromImage(bitmap))
			{
				g.Clear(Color.White);
				g.TranslateTransform((imageSize - boardSize.Width) / 2 + board.Location.X, (imageSize - boardSize.Height) / 2 + board.Location.Y);
				board.RenderBoardOnly(g);
			}
			string? path = FileChooser.ChooseSaveFile("Export Board", "Image", "jpg", "Board");
			if (path != null)
			{
				QualityDialog dialog = new QualityDialog();
				DialogHelper.ShowDialog(dialog);
				if (dialog.Success)
				{
					long quality = dialog.Quality;
					string fen = GameManager.GetGame().GetFen();
					byte[] fenBytes = Encoding.UTF8.GetBytes(fen);
					PropertyItem propertyItem = (PropertyItem)FormatterServices.GetUninitializedObject(typeof(PropertyItem));
					propertyItem.Id = 0x010E;
					propertyItem.Type = 1;
					propertyItem.Len = fenBytes.Length;
					propertyItem.Value = fenBytes;
					bitmap.SetPropertyItem(propertyItem);
					EncoderParameters encoderParameters = new EncoderParameters(1);
					encoderParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);
					ImageCodecInfo jpegEncoder = ImageCodecInfo.GetImageEncoders().First(codec => codec.FormatID == ImageFormat.Jpeg.Guid);
					bitmap.Save(path, jpegEncoder, encoderParameters);
				}
			}
		}
	}

	private static void ImportImage()
	{
		if (!PgnManager.SaveBackup())
		{
			return;
		}
		string? path = FileChooser.ChooseOpenFile("Import Board", "Image", "jpg");
		if (path != null)
		{
			using (Bitmap bitmap = new Bitmap(path))
			{
				PropertyItem? propertyItem = bitmap.GetPropertyItem(0x010E);
				if (propertyItem?.Value != null)
				{
					string fen = Encoding.UTF8.GetString(propertyItem.Value);
					PgnManager.NewGame(fen);
				}
			}
		}
	}
}

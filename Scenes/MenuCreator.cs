namespace Scabine.Scenes;

using Scabine.App.Menus;
using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;

internal static class MenuCreator
{
	private static readonly Font MenuFont = new Font("Segoe UI", 12);

	public static ContextMenuStrip CreateContextMenu()
	{
		ContextMenuStrip menu = new ContextMenuStrip
		{
			Padding = Padding.Empty,
			Margin = Padding.Empty,
			Renderer = new SceneToolStripRenderer(),
			Font = MenuFont
		};
		return menu;
	}

	public static MenuStrip CreateMainMenu()
	{
		MenuStrip menu = new MenuStrip
		{
			Padding = Padding.Empty,
			Margin = Padding.Empty,
			Renderer = new SceneToolStripRenderer(),
			Font = MenuFont
		};
		return menu;
	}

	public static ToolStripMenuItem AddMenuItem(ToolStrip menu, string text, Image? image, Action? action)
	{
		ToolStripMenuItem item = new ToolStripMenuItem(text)
		{
			TextAlign = ContentAlignment.MiddleLeft,
			Padding = Padding.Empty,
			Margin = Padding.Empty
		};
		if (image != null)
		{
			item.Image = image;
		}
		if (action != null)
		{
			item.Click += (sender, e) => action();
		}
		menu.Items.Add(item);
		return item;
	}

	public static ToolStripLabel AddMenuLabel(ToolStrip menu, string text)
	{
		ToolStripLabel item = new ToolStripLabel(text)
		{
			Padding = Padding.Empty,
			TextAlign = ContentAlignment.MiddleCenter
		};
		menu.Items.Add(item);
		return item;
	}

	public static void AddMenuSeparator(ToolStrip menu)
	{
		ToolStripSeparator separator = new ToolStripSeparator();
		menu.Items.Add(separator);
	}

	public static ToolStripMenuItem AddSubMenuItem(ToolStripMenuItem menu, string text, Image? image, Action? action)
	{
		return AddSubMenuItem(menu, text, image, action, Keys.None);
	}

	public static ToolStripMenuItem AddSubMenuItem(ToolStripMenuItem menu, string text, Image? image, Action? action, Keys shortCut)
	{
		ToolStripMenuItem item = new ToolStripMenuItem(text)
		{
			TextAlign = ContentAlignment.MiddleLeft,
			ShortcutKeys = shortCut
		};
		if (image != null)
		{
			item.Image = image;
		}
		if (action != null)
		{
			item.Click += (sender, e) => action();
		}
		menu.DropDownItems.Add(item);
		return item;
	}

	public static ToolStripLabel AddSubMenuLabel(ToolStripMenuItem menu, string text)
	{
		ToolStripLabel item = new ToolStripLabel(text)
		{
			Margin = Padding.Empty,
			Padding = Padding.Empty,
			AutoSize = false,
			Size = TextRenderer.MeasureText(text, menu.Font)
		};
		menu.DropDownItems.Add(item);
		return item;
	}

	public static void AddSubMenuSeparator(ToolStripMenuItem menu)
	{
		ToolStripSeparator separator = new ToolStripSeparator();
		menu.DropDownItems.Add(separator);
	}
}

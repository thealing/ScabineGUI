namespace Scabine.App.Dialogs;

using System;
using System.Drawing;
using System.Windows.Forms;

internal static partial class DialogCreator
{
	internal static GroupBox AddGroupBox(Control.ControlCollection collection, string text, int x, int y, int width, int height)
	{
		GroupBox groupBox = new GroupBox()
		{
			Text = text,
			Bounds = new Rectangle(x, y, width, height)
		};
		collection.Add(groupBox);
		return groupBox;
	}

	internal static Label AddLabel(Control.ControlCollection collection, string text, int x, int y, int width, int height)
	{
		return AddLabel(collection, text, x, y, width, height, ContentAlignment.MiddleLeft);
	}

	internal static Label AddLabel(Control.ControlCollection collection, string text, int x, int y, int width, int height, ContentAlignment alignment)
	{
		Label label = new Label()
		{
			Text = text,
			Bounds = new Rectangle(x, y, width, height),
			TextAlign = alignment
		};
		collection.Add(label);
		return label;
	}

	internal static TextBox AddTextBox(Control.ControlCollection collection, string text, int x, int y, int width, int height, EventHandler? change)
	{
		TextBox textBox = new TextBox()
		{
			Text = text,
			Bounds = new Rectangle(x, y, width, height)
		};
		if (change != null)
		{
			textBox.TextChanged += change;
		}
		collection.Add(textBox);
		return textBox;
	}

	internal static Button AddButton(Control.ControlCollection collection, string text, int x, int y, int width, int height, EventHandler? click)
	{
		Button button = new NonSelectableButton()
		{
			Text = text,
			Bounds = new Rectangle(x, y, width, height)
		};
		if (click != null)
		{
			button.Click += click;
		}
		collection.Add(button);
		return button;
	}

	internal static CheckBox AddCheckBox(Control.ControlCollection collection, string text, int x, int y, int width, int height, EventHandler? click)
	{
		CheckBox button = new NonSelectableCheckBox()
		{
			Text = text,
			Bounds = new Rectangle(x, y, width, height)
		};
		if (click != null)
		{
			button.Click += click;
		}
		collection.Add(button);
		return button;
	}

	internal static BigCheckBox AddBigCheckBox(Control.ControlCollection collection, string text, int x, int y, int width, int height, EventHandler? click)
	{
		BigCheckBox button = new NonSelectableBigCheckBox()
		{
			Text = text,
			Bounds = new Rectangle(x, y, width, height)
		};
		if (click != null)
		{
			button.Click += click;
		}
		collection.Add(button);
		return button;
	}

	internal static CheckBox AddCheckButton(Control.ControlCollection collection, string text, int x, int y, int width, int height, EventHandler? click)
	{
		CheckBox button = new NonSelectableCheckBox()
		{
			Text = text,
			Bounds = new Rectangle(x, y, width, height),
			Appearance = Appearance.Button,
			TextAlign = ContentAlignment.MiddleCenter
		};
		if (click != null)
		{
			button.Click += click;
		}
		collection.Add(button);
		return button;
	}

	internal static ComboBox AddDropDownList(Control.ControlCollection collection, int x, int y, int width, int height)
	{
		ComboBox comboBox = new ComboBox
		{
			Bounds = new Rectangle(x, y, width, height),
			DropDownStyle = ComboBoxStyle.DropDownList
		};
		collection.Add(comboBox);
		return comboBox;
	}

	internal static PictureBox AddPictureBox(Control.ControlCollection collection, int x, int y, int width, int height, Image? image, PaintEventHandler? paint, EventHandler? click)
	{
		PictureBox pictureBox = new PictureBox
		{
			Bounds = new Rectangle(x, y, width, height)
		};
		if (image != null)
		{
			pictureBox.Image = image;
			pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
		}
		if (paint != null)
		{
			pictureBox.Paint += paint;
		}
		if (click != null)
		{
			pictureBox.Click += click;
		}
		collection.Add(pictureBox);
		return pictureBox;
	}

	internal static RadioButton AddRadioButton(Control.ControlCollection collection, string text, int x, int y, int width, int height, EventHandler? click)
	{
		return AddRadioButton(collection, text, x, y, width, height, click, Appearance.Normal);
	}

	internal static RadioButton AddRadioButton(Control.ControlCollection collection, string text, int x, int y, int width, int height, EventHandler? click, Appearance appearance)
	{
		RadioButton button = new RadioButton()
		{
			Text = text,
			Bounds = new Rectangle(x, y, width, height),
			CheckAlign = ContentAlignment.MiddleLeft,
			TextAlign = appearance == Appearance.Button ? ContentAlignment.MiddleCenter : ContentAlignment.MiddleLeft, 
			Appearance = appearance
		};
		if (click != null)
		{
			button.Click += click;
		}
		collection.Add(button);
		return button;
	}

	internal static TrackBar AddTrackBar(Control.ControlCollection collection, int x, int y, int width, int height, EventHandler? change)
	{
		TrackBar trackBar = new TrackBar()
		{
			AutoSize = false,
			Bounds = new Rectangle(x, y, width, height)
		};
		collection.Add(trackBar);
		if (change != null)
		{
			trackBar.ValueChanged += change;
		}
		return trackBar;
	}

	internal static NumericUpDown AddNumber(Control.ControlCollection collection, bool arrows, int x, int y, int width, int height, decimal value, decimal min, decimal max, EventHandler? change)
	{
		NumericUpDown numericUpDown = arrows ? new NumericUpDown() : new ButtonlessNumericUpDown();
		numericUpDown.AutoSize = false;
		numericUpDown.Bounds = new Rectangle(x, y, width, height);
		numericUpDown.TextAlign = arrows ? HorizontalAlignment.Center : HorizontalAlignment.Right;
		numericUpDown.Minimum = min;
		numericUpDown.Maximum = max;
		numericUpDown.Value = Math.Clamp(value, min, max);
		if (change != null)
		{
			numericUpDown.ValueChanged += change;
		}
		numericUpDown.Controls[1].TextChanged += (sender, e) =>
		{
			if (decimal.TryParse(numericUpDown.Controls[1].Text, out decimal result) && result >= numericUpDown.Minimum && result <= numericUpDown.Maximum)
			{
				numericUpDown.Value = result;
			}
		};
		collection.Add(numericUpDown);
		return numericUpDown;
	}

	internal static ListBox AddListBox(Control.ControlCollection collection, int x, int y, int width, int height, EventHandler? change)
	{
		ListBox listBox = new ListBox()
		{
			Bounds = new Rectangle(x, y, width, height)
		};
		collection.Add(listBox);
		if (change != null)
		{
			listBox.Click += change;
		}
		return listBox;
	}

	internal static TabControl AddTabControl(Control.ControlCollection collection, int x, int y, int width, int height)
	{
		TabControl tabControl = new TabControl()
		{
			Bounds = new Rectangle(x, y, width, height),
			SizeMode = TabSizeMode.Fixed,
			ItemSize = new Size(90, 30),
		};
		collection.Add(tabControl);
		return tabControl;
	}

	internal static TabPage AddTabPage(Control.ControlCollection collection, string text, int x, int y, int width, int height)
	{
		TabPage tabPage = new TabPage()
		{
			Text = text,
			Bounds = new Rectangle(x, y, width, height),
			UseVisualStyleBackColor = true
		};
		collection.Add(tabPage);
		return tabPage;
	}

	internal static Panel AddPanel(Control.ControlCollection collection, int x, int y, int width, int height)
	{
		Panel panel = new Panel()
		{
			Bounds = new Rectangle(x, y, width, height),
			BorderStyle = BorderStyle.FixedSingle
		};
		collection.Add(panel);
		return panel;
	}

	private class NonSelectableButton : Button
	{
		public NonSelectableButton()
		{
			SetStyle(ControlStyles.Selectable, false);
		}
	}

	private class NonSelectableCheckBox : CheckBox
	{
		public NonSelectableCheckBox()
		{
			SetStyle(ControlStyles.Selectable, false);
		}
	}

	private class NonSelectableBigCheckBox : BigCheckBox
	{
		public NonSelectableBigCheckBox()
		{
			SetStyle(ControlStyles.Selectable, false);
		}
	}

	private class ButtonlessNumericUpDown : NumericUpDown
	{
		public ButtonlessNumericUpDown()
		{
			Controls[0].Hide();
		}

		protected override void OnTextBoxResize(object source, EventArgs e)
		{
			Controls[1].Width = Width - 10;
		}
	}
}

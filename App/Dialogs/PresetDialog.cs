namespace Scabine.App.Dialogs;

using Scabine.App;
using Scabine.Engines;
using Scabine.Scenes;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

internal class PresetDialog : BaseDialog
{
	public PresetDialog(EngineInfo engine, string presetName)
	{
		EngineOptions preset = engine.Presets[presetName];
		EngineOptions editedPreset = new EngineOptions(preset);
		FormBorderStyle = FormBorderStyle.Sizable;
		MaximizeBox = true;
		ClientSize = new Size(600, 600);
		MinimumSize = new Size(560, 200) + Size - ClientSize;
		Text = "Edit Preset: " + presetName;
		Font = new Font("Verdana", 15);
		Panel panel = new Panel
		{
			AutoScroll = true,
			BorderStyle = BorderStyle.FixedSingle
		};
		Button setDefaultsButton = new Button()
		{
			Text = "Set Defaults"
		};
		Button cancelButton = new Button()
		{
			Text = "Cancel"
		};
		Button doneButton = new Button()
		{
			Text = "Done"
		};
		void UpdatePanel()
		{
			panel.Location = new Point(20, 20);
			panel.ClientSize = new Size(ClientSize.Width - 40, ClientSize.Height - 100);
			setDefaultsButton.Location = new Point(20, ClientSize.Height - 60);
			setDefaultsButton.ClientSize = new Size(160, 40);
			cancelButton.Location = new Point(ClientSize.Width - 360, ClientSize.Height - 60);
			cancelButton.ClientSize = new Size(160, 40);
			doneButton.Location = new Point(ClientSize.Width - 180, ClientSize.Height - 60);
			doneButton.ClientSize = new Size(160, 40);
		}
		UpdatePanel();
		ResizeEnd += (sender, e) => UpdatePanel();
		Controls.Add(setDefaultsButton);
		Controls.Add(cancelButton);
		Controls.Add(doneButton);
		int rowHeight = Font.Height * 3 / 2;
		int height = 0;
		foreach (var option in preset.Options.Keys)
		{
			int currentHeight = height;
			Label label = new Label()
			{
				Text = option.Name,
				TextAlign = ContentAlignment.MiddleLeft,
				AutoEllipsis = true
			};
			void UpdateLabel()
			{
				label.Location = new Point(10, currentHeight);
				label.ClientSize = new Size(panel.ClientSize.Width / 2, rowHeight);
			}
			UpdateLabel();
			panel.Resize += (sender, e) => UpdateLabel();
			panel.Controls.Add(label);
			Control? control = null;
			if (option is CheckOption checkOption)
			{
				CheckBox checkBox = new BigCheckBox
				{
					Checked = (bool)preset.Options[option],
					CheckAlign = ContentAlignment.MiddleCenter
				};
				setDefaultsButton.Click += (sender, e) => checkBox.Checked = (bool)option.GetValue();
				checkBox.CheckedChanged += (sender, e) => editedPreset.Options[option] = checkBox.Checked;
				control = checkBox;
			}
			if (option is ComboOption comboOption)
			{
				ComboBox comboBox = new ComboBox()
				{
					DropDownStyle = ComboBoxStyle.DropDownList
				};
				comboBox.Items.AddRange(comboOption.Options);
				comboBox.SelectedItem = (string)preset.Options[option];
				setDefaultsButton.Click += (sender, e) => comboBox.SelectedItem = (string)option.GetValue();
				comboBox.SelectedIndexChanged += (sender, e) => editedPreset.Options[option] = (string)comboBox.SelectedItem;
				control = comboBox;
			}
			if (option is SpinOption spinOption)
			{
				NumericUpDown numericUpDown = new NumericUpDown
				{
					Minimum = spinOption.Min,
					Maximum = spinOption.Max,
					Value = (int)preset.Options[option],
					Increment = (spinOption.Max - spinOption.Min) / 50
				};
				setDefaultsButton.Click += (sender, e) => numericUpDown.Value = (int)option.GetValue();
				numericUpDown.ValueChanged += (sender, e) => editedPreset.Options[option] = (int)numericUpDown.Value;
				control = numericUpDown;
			}
			if (option is StringOption stringOption)
			{
				TextBox textBox = new TextBox
				{
					Text = (string)preset.Options[option]
				};
				setDefaultsButton.Click += (sender, e) => textBox.Text = (string)option.GetValue();
				textBox.TextChanged += (sender, e) => editedPreset.Options[option] = textBox.Text;
				control = textBox;
			}
			if (control != null)
			{
				panel.Controls.Add(control);
				void UpdateControl()
				{
					control.Location = new Point(panel.ClientSize.Width * 7 / 12, currentHeight);
					control.ClientSize = new Size(panel.ClientSize.Width / 3, rowHeight);
				}
				UpdateControl();
				panel.Resize += (sender, e) => UpdateControl();
			}
			height += rowHeight;
		}
		Controls.Add(panel);
		cancelButton.Click += (sender, e) =>
		{
			Close();
		};
		doneButton.Click += (sender, e) =>
		{
			engine.Presets[presetName] = editedPreset;
			Close();
		};
	}
}

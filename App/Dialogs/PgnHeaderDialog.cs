namespace Scabine.App.Dialogs;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Security.Policy;
using System.Threading.Channels;
using System.Windows.Forms;
using static Scabine.App.Dialogs.DialogCreator;

internal class PgnHeaderDialog : BaseDialog
{
	public PgnHeaderDialog()
	{
		// Form settings
		ClientSize = new Size(600, 500);
		Text = "Edit Game Data";
		Font = new Font("Segoe UI", 13);
		AddButton(Controls, "Cancel", 20, ClientSize.Height - 60, 100, 40, Cancel);
		AddButton(Controls, "Done", ClientSize.Width - 120, ClientSize.Height - 60, 100, 40, Done);
		dataGridView = new DataGridView
		{
			Dock = DockStyle.Top,
			Height = 420,
			AllowUserToAddRows = true,
			AllowUserToDeleteRows = true,
			AutoGenerateColumns = false,
			BackgroundColor = SystemColors.Control,
			BorderStyle = BorderStyle.Fixed3D,
			ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
			AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCellsExceptHeaders
		};
		dataGridView.Columns.Add(new DataGridViewTextBoxColumn
		{
			Name = "Key",
			HeaderText = "Key",
			AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
		});
		dataGridView.Columns.Add(new DataGridViewTextBoxColumn
		{
			Name = "Value",
			HeaderText = "Value",
			AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
		});
		IDictionary<string, string> pgn = PgnManager.GetHeader();
		foreach (KeyValuePair<string, string> tag in pgn)
		{
			dataGridView.Rows.Add(tag.Key, tag.Value);
		}
		Controls.Add(dataGridView);
	}

	private void Cancel(object? sender, EventArgs e)
	{
		Close();
	}

	private void Done(object? sender, EventArgs e)
	{
		dataGridView.EndEdit();
		IDictionary<string, string> pgn = new Dictionary<string, string>();
		foreach (DataGridViewRow row in dataGridView.Rows)
		{
			if (row.IsNewRow)
			{
				continue;
			}
			string? key = row.Cells[0].Value?.ToString();
			string? value = row.Cells[1].Value?.ToString();
			if (!string.IsNullOrEmpty(key) && value != null)
			{
				pgn[key] = value;
			}
		}
		PgnManager.SetHeader(pgn);
		Close();
	}

	private readonly DataGridView dataGridView;
}

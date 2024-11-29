namespace Scabine.Scenes;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

internal static class SceneProfiler
{
	public static readonly Dictionary<SceneNode, double> UpdateDurations;
	public static readonly Dictionary<SceneNode, double> RenderDurations;

	static SceneProfiler()
	{
		UpdateDurations = new Dictionary<SceneNode, double>();
		RenderDurations = new Dictionary<SceneNode, double>();
		_resultsWindow = new Form()
		{
			Text = "Scene Profiler",
			ClientSize = new Size(800, 600),
			StartPosition = FormStartPosition.CenterScreen,
			TopMost = true
		};
		_resultsWindow.FormClosing += (sender, e) =>
		{
			e.Cancel = true;
			_resultsWindow.Hide();
			SceneManager.FocusWindow();
		};
		System.Windows.Forms.SplitContainer splitContainer = new System.Windows.Forms.SplitContainer
		{
			Dock = DockStyle.Fill,
			Orientation = Orientation.Vertical,
			BorderStyle = BorderStyle.Fixed3D
		};
		_updatePanel = new DoubleBufferedPanel() { Dock = DockStyle.Fill };
		_renderPanel = new DoubleBufferedPanel() { Dock = DockStyle.Fill };
		_updatePanel.Paint += RenderPanel;
		_renderPanel.Paint += RenderPanel;
		splitContainer.Panel1.Controls.Add(_updatePanel);
		splitContainer.Panel2.Controls.Add(_renderPanel);
		splitContainer.SplitterDistance = splitContainer.Width / 2;
		_resultsWindow.Controls.Add(splitContainer);
		_updateCounters = new Dictionary<SceneNode, (int, double)>();
		_renderCounters = new Dictionary<SceneNode, (int, double)>();
	}

	public static void BeforeUpdate()
	{
		UpdateDurations.Clear();
	}

	public static void AfterUpdate()
	{
		AddResults(UpdateDurations, _updateCounters);
		if (InputManager.IsKeyPressed(Keys.N))
		{
			_resultsWindow.Show();
		}
	}

	public static void BeforeRender()
	{
		RenderDurations.Clear();
	}

	public static void AfterRender()
	{
		AddResults(RenderDurations, _renderCounters);
		double time = Time.GetTime();
		if (time > _renderTime + 0.1)
		{
			_renderTime = time;
			ShowResults(_updateCounters, _updatePanel);
			ShowResults(_renderCounters, _renderPanel);
		}
		if (time > _clearTime + 1.0)
		{
			_clearTime = time;
			_updateCounters.Clear();
			_renderCounters.Clear();
		}
	}

	private static void AddResults(Dictionary<SceneNode, double> values, Dictionary<SceneNode, (int, double)> counters)
	{
		foreach (var (key, value) in values)
		{
			var (count, sum) = counters.GetValueOrDefault(key);
			counters[key] = (count + 1, sum + value);
		}
	}

	private static void ShowResults(Dictionary<SceneNode, (int, double)> counters, Panel panel)
	{
		if (_resultsWindow.Visible)
		{
			_sortedDurations = counters.ToDictionary(kvp => kvp.Key, pair => pair.Value.Item2 / pair.Value.Item1).OrderByDescending(pair => pair.Value).ToList();
			panel.Refresh();
		}
	}

	private static void RenderPanel(object? sender, PaintEventArgs e)
	{
		if (_sortedDurations == null)
		{
			return;
		}
		e.Graphics.Clear(SystemColors.Control);
		Font font = new Font("Segoe UI", 9);
		int padding = 10;
		int lineHeight = font.Height + padding;
		int y = 0;
		foreach (var (node, duration) in _sortedDurations)
		{
			string text = $"{node.GetType().Name}: {duration * 1000:00.00}ms";
			Rectangle rectangle = new Rectangle(0, y, e.ClipRectangle.Width, lineHeight);
			if (node.ContainsMouse())
			{
				e.Graphics.FillRectangle(Brushes.Red, rectangle);
			}
			e.Graphics.DrawString(text, font, Brushes.Black, rectangle, StringFormats.LeftClipped);
			y += lineHeight;
			if (y > e.ClipRectangle.Height)
			{
				break;
			}
		}
	}

	private static readonly Form _resultsWindow;
	private static readonly Panel _updatePanel;
	private static readonly Panel _renderPanel;
	private static readonly Dictionary<SceneNode, (int, double)> _updateCounters;
	private static readonly Dictionary<SceneNode, (int, double)> _renderCounters;
	private static double _renderTime;
	private static double _clearTime;
	private static List<KeyValuePair<SceneNode, double>>? _sortedDurations;

	private class DoubleBufferedPanel : Panel
	{
		public DoubleBufferedPanel()
		{
			DoubleBuffered = true;
			SetStyle(ControlStyles.UserPaint, true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, true);
			SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
		}
	}
}

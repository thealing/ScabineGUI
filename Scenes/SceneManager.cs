namespace Scabine.Scenes;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

public static class SceneManager
{
	private static readonly Size WindowSize = new Size(1280, 720);
	private static readonly double UpdateDelta = 1.0 / 500.0;
	private static readonly double RenderDelta = 1.0 / 60.0;
	private static readonly double MeasureDelta = 1.0 / 2.0;

	public static void Run(Scene? scene)
	{
		Application.EnableVisualStyles();
		_window.KeyDown += (sender, e) => InputManager.OnKeyDown(e);
		_window.KeyUp += (sender, e) => InputManager.OnKeyUp(e);
		_window.MouseDown += (sender, e) => InputManager.OnMouseDown(e);
		_window.MouseUp += (sender, e) => InputManager.OnMouseUp(e);
		_window.MouseWheel += (sender, e) => InputManager.OnMouseWheel(e);
		_window.MouseMove += (sender, e) => InputManager.OnMouseMove(e);
		_window.MouseLeave += (sender, e) => InputManager.OnMouseLeave(e);
		_window.Deactivate += (sender, e) => InputManager.OnDeactivate(e);
		_window.Paint += (sender, e) => OnPaint(e);
		_window.Resize += (sender, e) => OnResize();
		_window.Disposed += (sender, e) => OnClose();
		_window.ClientSize = WindowSize;
		_window.Show();
		ChangeScene(scene);
		Loop();
		ChangeScene(null);
	}

	public static void Exit()
	{
		if (_scene != null && !_scene.CanExit())
		{
			return;
		}
		_scene?.Leave();
		_window.Dispose();
	}

	public static Scene? GetScene()
	{
		return _scene;
	}

	public static void ChangeScene(Scene? scene)
	{
		_scene?.Leave();
		_scene = scene;
		_scene?.Enter();
	}

	public static void SetMenu(MenuStrip? menu)
	{
		if (_menu != null)
		{
			_window.Controls.Remove(_menu);
		}
		_menu = menu;
		if (_menu != null)
		{
			_menu.Cursor = Cursors.Default;
			_window.Controls.Add(_menu);
		}
	}

	public static bool IsBackground()
	{
		return _window != Form.ActiveForm;
	}

	public static DialogResult ShowMessageBox(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
	{
		return CenteredMessageBox.Show(_window, text, caption, buttons, icon);
	}

	internal static void FocusWindow()
	{
		_window.Focus();
	}

	private static void Loop()
	{
		uint timerResolution = 1;
		TimeBeginPeriod(timerResolution);
		double updateTime = Time.GetTime();
		double renderTime = Time.GetTime();
		while (!_disposed)
		{
			double time = Time.GetTime();
			if (time > updateTime + UpdateDelta)
			{
				BeforeUpdate();
				UpdateScene();
				AfterUpdate();
				updateTime = time;
			}
			if (time > renderTime + RenderDelta)
			{
				if (_window.WindowState != FormWindowState.Minimized)
				{
					_window.Text = _scene?.GetTitle();
					_window.Refresh();
				}
				renderTime = time;
			}
			if (time > _measureTime + MeasureDelta)
			{
				MeasureFps();
			}
			double sleepDuration = Math.Min(updateTime + UpdateDelta, renderTime + RenderDelta) - Time.GetTime() - timerResolution / 1000.0;
			Time.Sleep(sleepDuration);
		}
		TimeEndPeriod(timerResolution);
	}

	private static void UpdateScene()
	{
		Point position = new Point(0, _menu == null ? 0 : _menu.Bottom);
		_scene?.Resize(new Rectangle(position, _window.ClientSize - (Size)position));
		_scene?.Update();
		_updateCount++;
	}

	private static void BeforeUpdate()
	{
		if (_window.WindowState != FormWindowState.Minimized && _scene != null)
		{
			_window.MinimumSize = _scene.GetMinSize() + _window.Size - _window.ClientSize;
			_window.Text = _scene.GetTitle();
		}
		Application.DoEvents();
		CursorManager.Commit();
		_showFps ^= InputManager.IsKeyPressed(Keys.B);
	}

	private static void AfterUpdate()
	{
		InputManager.Commit();
	}

	private static void RenderScene(Graphics g)
	{
		_scene?.Render(g);
		_renderCount++;
	}

	private static void BeforeRender(Graphics g)
	{
		g.Clear(Color.White);
	}

	private static void AfterRender(Graphics g)
	{
		if (_showFps)
		{
			Font font = new Font("Segoe UI", 15, FontStyle.Bold);
			Brush brush = new SolidBrush(Color.FromArgb(170, Color.Green));
			g.DrawString(GetUpdateFps().ToString(), font, brush, new Point(50, _window.ClientSize.Height - 20), StringFormats.RightAligned);
			g.DrawString(GetRenderFps().ToString(), font, brush, new Point(100, _window.ClientSize.Height - 20), StringFormats.RightAligned);
		}
	}

	private static void MeasureFps()
	{
		double elapsed = Time.GetTime() - _measureTime;
		_updateFps = (int)Math.Round(_updateCount / elapsed);
		_renderFps = (int)Math.Round(_renderCount / elapsed);
		_updateCount = 0;
		_renderCount = 0;
		_measureTime += elapsed;
	}

	private static int GetUpdateFps()
	{
		return _updateFps;
	}

	private static int GetRenderFps()
	{
		return _renderFps;
	}

	private static void OnPaint(PaintEventArgs e)
	{
		Graphics g = e.Graphics;
		BeforeRender(g);
		RenderScene(g);
		AfterRender(g);
	}

	private static void OnResize()
	{
		_scene?.Resize(new Rectangle(Point.Empty, _window.ClientSize));
	}

	private static void OnClose()
	{
		_disposed = true;
	}

	static SceneManager()
	{
		_window = new SceneWindow();
		_disposed = false;
		_scene = null;
		_updateCount = 0;
		_renderCount = 0;
		_measureTime = 0;
		_updateFps = 0;
		_renderFps = 0;
		_showFps = false;
	}

	private static SceneWindow _window;
	private static MenuStrip? _menu;
	private static bool _disposed;
	private static Scene? _scene;
	private static int _updateCount;
	private static int _renderCount;
	private static double _measureTime;
	private static int _updateFps;
	private static int _renderFps;
	private static bool _showFps;

	[DllImport("winmm.dll", EntryPoint = "timeBeginPeriod")]
	private static extern uint TimeBeginPeriod(uint period);

	[DllImport("winmm.dll", EntryPoint = "timeEndPeriod")]
	private static extern uint TimeEndPeriod(uint period);
}

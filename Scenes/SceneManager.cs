namespace ChessPanel.Scenes;

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using ChessPanel.Application.Settings;

public static class SceneManager
{
	public static Form Window => _window;

	private static readonly double UpdateDelta = 1.0 / 150.0;
	private static readonly double RenderDelta = 1.0 / 60.0;
	private static readonly double MeasureDelta = 1.0 / 2.0;

	public static void Run(Scene? scene)
	{
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
		ChangeScene(scene);
		_window.Show();
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
		ScheduleUpdate();
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

	public static void ScheduleUpdate()
	{
		_doUpdate = true;
		_doRender = true;
	}

	internal static void FocusWindow()
	{
		_window.Focus();
	}

	private static bool IsMinimized()
	{
		return _window.WindowState == FormWindowState.Minimized;
	}

	private static void Loop()
	{
		uint timerResolution = 1;
		TimeBeginPeriod(timerResolution);
		double updateTime = Time.GetTime();
		double renderTime = Time.GetTime();
		while (!_disposed)
		{
			if (!_doRender && InvalidationManager.IsInvalidated())
			{
				ScheduleUpdate();
			}
			double time = Time.GetTime();
			if (time > updateTime + UpdateDelta)
			{
				Application.DoEvents();
				if (_doUpdate || !General.EnableIdleMode)
				{
					_doUpdate = false;
					BeforeUpdate();
					UpdateScene();
					AfterUpdate();
				}
				UpdateFpsDisplay();
				InputManager.Commit();
				updateTime = time;
			}
			if (_doRender && time > renderTime + RenderDelta)
			{
				_doRender = false;
				if (!_window.IsDisposed && !IsMinimized())
				{
					_window.Refresh();
				}
				renderTime = time;
			}
			if (time > _measureTime + MeasureDelta)
			{
				MeasureFps();
			}
			double sleepDuration = updateTime + UpdateDelta;
			if (_doRender)
			{
				sleepDuration = Math.Min(sleepDuration, renderTime + RenderDelta);
			}
			sleepDuration -=  Time.GetTime() + timerResolution / 1000.0;
			Time.Sleep(sleepDuration);
		}
		TimeEndPeriod(timerResolution);
	}

	private static void UpdateScene()
	{
		if (!IsMinimized())
		{
			UpdateSize();
		}
		_scene?.Update();
		_updateCount++;
	}

	private static void BeforeUpdate()
	{
		if (!IsMinimized() && _scene != null)
		{
			_window.MinimumSize = _scene.GetMinSize() + _window.Size - _window.ClientSize;
			_window.Text = _scene.GetTitle();
		}
		CursorManager.Commit();
		ToolTipManager.Commit();
	}

	private static void AfterUpdate()
	{
		UpdateToolTip();
	}

	private static void UpdateToolTip()
	{
		if (_window.IsDisposed)
		{
			return;
		}
		string? toolTipText = ToolTipManager.GetToolTipText();
		if (toolTipText == null)
		{
			_toolTip.Hide(_window);
		}
		else if (ToolTipManager.IsDirty() || !InputManager.GetMouseDelta().IsEmpty)
		{
			_toolTip.Show(toolTipText, _window, InputManager.GetMousePosition() + new Size(0, 50));
		}
	}

	private static void UpdateFpsDisplay()
	{
		if (InputManager.IsKeyPressed(Keys.B))
		{
			_showFps ^= true;
			InvalidationManager.ForceInvalidate();
		}
	}

	private static void RenderScene(Graphics g)
	{
		_scene?.Render(g);
	}

	private static void BeforeRender(Graphics g)
	{
		g.Clear(Color.White);
	}

	private static void AfterRender(Graphics g)
	{
		RenderFpsDisplay(g);
	}

	private static void RenderFpsDisplay(Graphics g)
	{
		if (_showFps)
		{
			Font font = new Font("Segoe UI", 15, FontStyle.Bold);
			Brush brush = new SolidBrush(Color.FromArgb(170, Color.Green));
			StringFormat format = new StringFormat()
			{
				Alignment = StringAlignment.Far,
				LineAlignment = StringAlignment.Center,
				Trimming = StringTrimming.None,
				FormatFlags = StringFormatFlags.NoWrap | StringFormatFlags.NoClip
			};
			g.DrawString(GetUpdateFps().ToString(), font, brush, new Point(50, _window.ClientSize.Height - 20), format);
			g.DrawString(GetRenderFps().ToString(), font, brush, new Point(100, _window.ClientSize.Height - 20), format);
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
		if (_showFps)
		{
			InvalidationManager.ForceInvalidate();
		}
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
		Graphics g = _graphics.Graphics;
		InvalidationManager.Update();
		if (InvalidationManager.IsInvalidated())
		{
			g.ResetClip();
		}
		else
		{
			g.SetClip(Rectangle.Empty);
		}
		BeforeRender(g);
		RenderScene(g);
		AfterRender(g);
		if (InvalidationManager.IsInvalidated())
		{
			_graphics.Render();
			_renderCount++;
		}
	}

	private static void OnResize()
	{
		InvalidationManager.ForceInvalidate();
		_context = BufferedGraphicsManager.Current;
		_graphics?.Dispose();
		_graphics = _context.Allocate(_window.CreateGraphics(), _window.ClientRectangle);
		UpdateSize();
	}

	private static void OnClose()
	{
		_disposed = true;
	}

	private static void UpdateSize()
	{
		Point position = new Point(0, _menu == null ? 0 : _menu.Bottom);
		_scene?.Resize(new Rectangle(position, _window.ClientSize - (Size)position));
	}

	static SceneManager()
	{
		_window = new SceneWindow();
		_context = BufferedGraphicsManager.Current;
		_graphics = _context.Allocate(_window.CreateGraphics(), _window.ClientRectangle);
		_toolTip = new ToolTip();
		_disposed = false;
		_scene = null;
		_updateCount = 0;
		_renderCount = 0;
		_measureTime = 0;
		_updateFps = 0;
		_renderFps = 0;
		_showFps = false;
		InvalidationManager.RegisterInvalidatingStaticField(typeof(SceneManager), nameof(_scene));
		InvalidationManager.RegisterInvalidatingStaticField(typeof(SceneManager), nameof(_showFps));
	}

	private static SceneWindow _window;
	private static BufferedGraphicsContext _context;
	private static BufferedGraphics _graphics;
	private static ToolTip _toolTip;
	private static MenuStrip? _menu;
	private static bool _disposed;
	private static Scene? _scene;
	private static int _updateCount;
	private static int _renderCount;
	private static double _measureTime;
	private static int _updateFps;
	private static int _renderFps;
	private static bool _showFps;
	private static bool _doUpdate;
	private static bool _doRender;

	[DllImport("winmm.dll", EntryPoint = "timeBeginPeriod")]
	private static extern uint TimeBeginPeriod(uint period);

	[DllImport("winmm.dll", EntryPoint = "timeEndPeriod")]
	private static extern uint TimeEndPeriod(uint period);
}

namespace Scabine.App;

using Scabine.App.Dialogs;
using Scabine.App.Menus;
using Scabine.App.Prefs;
using Scabine.Core;
using Scabine.Engines;
using Scabine.Scenes;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;

internal class MainScene : Scene
{
	public MainScene()
	{
		_boardContainer = new PersistentSplitContainer("board", Direction.Horizontal);
		_rightContainer = new PersistentSplitContainer("right", Direction.Horizontal);
		_boardControl = new BoardControl();
		_moveListControl = new MoveListControl();
		_engineContainer = new EngineContainer();
		_playerDisplay = new PlayerDisplay();
		_analyzisDisplay = new AnalyzisDisplay();
		CreateMenu();
	}

	public override string GetTitle()
	{
		return PgnManager.GetTitle();
	}

	public override bool CanExit()
	{
		if (General.ConfirmExit)
		{
			return DialogHelper.ShowMessageBox("Are you sure?", "Exit", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes;
		}
		return true;
	}

	protected internal override void Enter()
	{
		AddChild(_boardContainer);
		_rightContainer.FirstChild = new FlowLayout(Direction.Vertical);
		_rightContainer.SecondChild = new FlowLayout(Direction.Vertical);
		_boardContainer.SecondChild = _rightContainer;
		_rightContainer.FirstChild.AddChild(_playerDisplay);
		_rightContainer.FirstChild.AddChild(_moveListControl);
		_rightContainer.SecondChild.AddChild(_analyzisDisplay);
		_rightContainer.SecondChild.AddChild(_engineContainer);
		_boardContainer.FirstChild = new SceneNode();
		_boardContainer.FirstChild.AddChild(_boardControl);
		base.Enter();
	}

	protected internal override void Leave()
	{
		PgnManager.SaveBackup();
		SaveManager.ForceUpdate();
		base.Leave();
	}

	protected internal override void Update()
	{
		GameManager.Update();
		PgnManager.Update();
		MatchManager.Update();
		SaveManager.Update();
		GameMenu.Update();
		base.Update();
	}

	protected internal override void Render(Graphics g)
	{
		base.Render(g);
		_boardControl.RenderGrabbedPiece(g);
	}

	private static void CreateMenu()
	{
		MenuStrip menu = MenuCreator.CreateMainMenu();
		ToolStripMenuItem fileMenu = MenuCreator.AddMenuItem(menu, " File ", null, null);
		FileMenu.Init(fileMenu);
		ToolStripMenuItem gameMenu = MenuCreator.AddMenuItem(menu, " Game ", null, null);
		GameMenu.Init(gameMenu);
		ToolStripMenuItem boardMenu = MenuCreator.AddMenuItem(menu, " Board ", null, null);
		BoardMenu.Init(boardMenu);
		ToolStripMenuItem engines = MenuCreator.AddMenuItem(menu, " Engines ", null, null);
		EnginesMenu.Init(engines);
		SceneManager.SetMenu(menu);
	}

	private readonly PersistentSplitContainer _boardContainer;
	private readonly PersistentSplitContainer _rightContainer;
	private readonly BoardControl _boardControl;
	private readonly MoveListControl _moveListControl;
	private readonly EngineContainer _engineContainer;
	private readonly PlayerDisplay _playerDisplay;
	private readonly AnalyzisDisplay _analyzisDisplay;
}

namespace Scabine.App;

using Scabine.App.Dialogs;
using Scabine.App.Prefs;
using Scabine.Engines;
using Scabine.Scenes;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

internal static class EngineManager
{
	public static IEnumerable<EngineInfo> GetInstalledEngines()
	{
		return _installedEngines;
	}

	public static bool InstallEngine(string path)
	{
		if (!File.Exists(path))
		{
			DialogHelper.ShowMessageBox("Invalid engine path!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			return false;
		}
		string hash = GetFileHash(path);
		if (_installedEngines.Any(engine => engine.Hash == hash))
		{
			DialogHelper.ShowMessageBox("Engine already installed!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			return false;
		}
		using IEngine engine = new ExternalEngine(new EngineParams(path, "", ""));
		if (!engine.IsRunning())
		{
			DialogHelper.ShowMessageBox("Failed to install engine!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			return false;
		}
		string name = engine.GetName();
		string author = engine.GetAuthor();
		EngineOptions options = new EngineOptions(engine.GetOptions());
		EngineInfo engineInfo = new EngineInfo(path, hash, name, author);
		engineInfo.Presets["Default"] = options;
		_installedEngines.Add(engineInfo);
		return true;
	}

	public static void UninstallEngine(EngineInfo engineInfo)
	{
		_installedEngines.Remove(engineInfo);
		List<EngineConfig> removedConfigs = new List<EngineConfig>();
		foreach (var (key, value) in _runningEngines)
		{
			if (key.Info == engineInfo)
			{
				value.Control.Remove();
				removedConfigs.Add(key);
			}
		}
		foreach (EngineConfig config in removedConfigs)
		{
			_runningEngines.Remove(config);
		}
	}

	public static IEngine? GetOrStartEngine(EngineInfo engineInfo, string presetName)
	{
		EngineOptions preset = engineInfo.Presets[presetName];
		EngineConfig config = new EngineConfig() { Info = engineInfo, Options = preset };
		do
		{
			if (_runningEngines.TryGetValue(config, out EngineInstance instance))
			{
				return instance.Engine;
			}
		}
		while (StartEngine(engineInfo, presetName));
		return null;
	}

	public static bool StartEngine(EngineInfo engineInfo, string presetName, out IEngine? engine)
	{
		engine = null;
		if (!engineInfo.Presets.ContainsKey(presetName))
		{
			DialogHelper.ShowMessageBox("Invalid engine preset!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			return false;
		}
		EngineOptions preset = engineInfo.Presets[presetName];
		EngineConfig config = new EngineConfig() { Info = engineInfo, Options = preset };
		if (_runningEngines.ContainsKey(config))
		{
			DialogHelper.ShowMessageBox("Engine is already running!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			return false;
		}
		if (!File.Exists(engineInfo.Path))
		{
			DialogHelper.ShowMessageBox("Engine not found!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			return false;
		}
		string hash = GetFileHash(engineInfo.Path);
		if (hash != engineInfo.Hash)
		{
			DialogResult result = DialogHelper.ShowMessageBox("Engine has been modified! Continue?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
			if (result == DialogResult.Yes)
			{
				engineInfo.Hash = hash;
			}
			else
			{
				return false;
			}
		}
		engine = new ExternalEngine(engineInfo);
		if (!engine.IsRunning())
		{
			DialogHelper.ShowMessageBox("Failed to start engine!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			return false;
		}
		foreach (var (option, value) in preset.Options)
		{
			engine.SetOption(option, value);
		}
		return true;
	}

	public static bool StartEngine(EngineInfo engineInfo, string presetName)
	{
		if (!StartEngine(engineInfo, presetName, out IEngine? engine) || engine == null)
		{
			return false;
		}
		EngineOptions preset = engineInfo.Presets[presetName];
		EngineConfig config = new EngineConfig() { Info = engineInfo, Options = preset };
		EngineContainer? engineContainer = GetEngineContainer();
		if (engineContainer == null)
		{
			return false;
		}
		_runningEngines[config] = new EngineInstance()
		{ 
			Engine = engine, 
			Control = engineContainer.AddEngine(engine, presetName) 
		};
		return true;
	}

	public static void ReloadEngine(IEngine engine, string presetName)
	{
		foreach (KeyValuePair<EngineConfig, EngineInstance> pair in _runningEngines)
		{
			if (pair.Value.Engine == engine)
			{
				_runningEngines.Remove(pair.Key);
				if (!StartEngine(pair.Key.Info, presetName, out IEngine? newEngine) || newEngine == null)
				{
					_runningEngines.Add(pair.Key, pair.Value);
					break;
				}
				engine.Dispose();
				pair.Value.Control.SetEngine(newEngine);
				_runningEngines[pair.Key] = new EngineInstance()
				{
					Engine = newEngine,
					Control = pair.Value.Control
				};
				break;
			}
		}
	}

	public static void StopEngine(IEngine engine)
	{
		foreach (KeyValuePair< EngineConfig, EngineInstance> pair in _runningEngines)
		{
			if (pair.Value.Engine == engine)
			{
				pair.Value.Control.Remove();
				_runningEngines.Remove(pair.Key);
				break;
			}
		}
	}

	public static void StopAllEngines()
	{
		IEnumerable<EngineConfig> keys = _runningEngines.Keys;
		foreach (EngineConfig key in keys)
		{
			EngineInstance instance = _runningEngines[key];
			if (!MatchManager.IsEnginePlaying(instance.Engine))
			{
				instance.Control.Remove();
				_runningEngines.Remove(key);
			}
		}
	}

	private static EngineContainer? GetEngineContainer()
	{
		return SceneManager.GetScene()?.FindNodeByType<EngineContainer>();
	}

	private static string GetFileHash(string path)
	{
		using SHA256 sha256 = SHA256.Create();
		using Stream stream = File.OpenRead(path);
		byte[] hashBytes = sha256.ComputeHash(stream);
		StringBuilder hashStringBuilder = new StringBuilder();
		foreach (byte hashByte in hashBytes)
		{
			hashStringBuilder.Append(hashByte.ToString("X2"));
		}
		return hashStringBuilder.ToString();
	}

	static EngineManager()
	{
		_installedEngines = new List<EngineInfo>();
		_runningEngines = new Dictionary<EngineConfig, EngineInstance>();
		SaveManager.Save += () => SaveManager.Sync("ie", ref _installedEngines);
	}

	private static List<EngineInfo> _installedEngines;
	private static readonly Dictionary<EngineConfig, EngineInstance> _runningEngines;

	private struct EngineConfig
	{
		public EngineInfo Info;
		public EngineOptions Options;
	}

	private struct EngineInstance
	{
		public IEngine Engine;
		public EngineControl Control;
	}
}

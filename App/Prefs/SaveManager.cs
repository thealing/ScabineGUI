namespace Scabine.App.Prefs;

using Scabine.App.Dialogs;
using Scabine.Engines;
using Scabine.Scenes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

internal static class SaveManager
{
	public static JsonSerializerOptions Options => _options;

	public static event SaveHandler? Save
	{
		add
		{
			if (_save != null && _save.GetInvocationList().Contains(value))
			{
				return;
			}
			_save += value;
			ForceUpdate();
		}
		remove
		{
			_save -= value;
		}
	}

	public delegate void SaveHandler();

	private static event SaveHandler? _save;

	public static string GetObjectTag(object obj)
	{
		return obj.GetType().Name + "::";
	}

	public static bool Sync<T>(object obj, string tag, T value) where T : notnull
	{
		return Sync(GetObjectTag(obj) + tag, value);
	}

	public static bool Sync<T>(object obj, string tag, ref T value) where T : notnull
	{
		return Sync(GetObjectTag(obj) + tag, ref value);
	}

	public static T? GetValue<T>(object obj, string tag)
	{
		return GetValue<T>(GetObjectTag(obj) + tag);
	}

	public static object? GetValue(object obj, string tag)
	{
		return GetValue(GetObjectTag(obj) + tag);
	}

	public static bool Sync<T>(string tag, T value) where T : notnull
	{
		T temp = value;
		return Sync(tag, ref temp);
	}

	public static bool Sync<T>(string tag, ref T value) where T : notnull
	{
		if (_changed.ContainsKey(tag))
		{
			value = (T)_changed[tag];
			_changed.Remove(tag);
		}
		bool result = !_loaded.Contains(tag);
		if (result && _elements.ContainsKey(tag))
		{
			_loaded.Add(tag);
			value = JsonSerializer.Deserialize<T>(_elements[tag], _options) ?? value;
		}
		if (value != null)
		{
			_values[tag] = value;
			JsonElement element = JsonSerializer.SerializeToElement(value, _options);
			if (!_elements.ContainsKey(tag) || _elements[tag].ToString() != element.ToString())
			{
				_loaded.Add(tag);
				_elements[tag] = element;
				_dirty = true;
			}
		}
		else
		{
			_values.Remove(tag);
			_elements.Remove(tag);
		}
		return result;
	}

	public static T? GetValue<T>(string tag)
	{
		if (!_values.ContainsKey(tag) && _elements.ContainsKey(tag))
		{
			try
			{
				T? value = JsonSerializer.Deserialize<T>(_elements[tag], _options);
				if (value != null)
				{
					_values[tag] = value;
				}
			}
			catch
			{
			}
		}
		object? obj = _values.GetValueOrDefault(tag);
		return obj == null ? default : (T?)obj;
	}

	public static object? GetValue(string tag)
	{
		return _values.GetValueOrDefault(tag);
	}

	public static void SetValue<T>(string tag, T value) where T : notnull
	{
		_values[tag] = value;
		_changed[tag] = value;
	}

	public static void ForceUpdate()
	{
		Update(true);
	}

	public static void Update(bool force = false)
	{
		if (!force && Time.GetTime() < _lastSaveTime + General.AutoSaveInterval / 1000.0)
		{
			return;
		}
		_save?.Invoke();
		if (_dirty)
		{
			try
			{
				string data = JsonSerializer.Serialize(_elements, _elements.GetType(), _options);
				File.WriteAllText(_savePath, data);
				_dirty = false;
			}
			catch
			{
			}
			_lastSaveTime = Time.GetTime();
		}
	}

	static SaveManager()
	{
		_savePath = Process.GetCurrentProcess().ProcessName + ".json";
		_options = new JsonSerializerOptions()
		{
			MaxDepth = int.MaxValue,
			IncludeFields = true
		};
		_options.Converters.Add(new TreeNode.Converter());
		_options.Converters.Add(new TreeGame.Converter());
		_options.Converters.Add(new UciOption.Converter());
		_options.Converters.Add(new EngineOptions.Converter());
		_values = new Dictionary<string, object>();
		_changed = new Dictionary<string, object>();
		_elements = new Dictionary<string, JsonElement>();
		_loaded = new HashSet<string>();
		_dirty = false;
		try
		{
			string data = File.ReadAllText(_savePath);
			_elements = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(data, _options) ?? _elements;
		}
		catch
		{
		}
	}

	private static readonly string _savePath;
	private static readonly JsonSerializerOptions _options;
	private static readonly Dictionary<string, object> _values;
	private static readonly Dictionary<string, object> _changed;
	private static readonly Dictionary<string, JsonElement> _elements;
	private static readonly HashSet<string> _loaded;
	private static bool _dirty;
	private static double _lastSaveTime;
}

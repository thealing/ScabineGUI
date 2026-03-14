namespace ChessBand.Scenes;

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

public static class InvalidationManager
{
	public static bool IsInvalidated()
	{
		return _entriesChanged || _invalidated;
	}

	public static void ForceInvalidate()
	{
		_entriesChanged = true;
	}

	public static void Update()
	{
		_invalidated = _entriesChanged;
		_entriesChanged = false;
		_entries.RemoveAll(entry => !entry.IsAlive());
		foreach (Entry entry in _entries)
		{
			if (entry.HasValueChanged())
			{
				_invalidated = true;
			}
		}
	}

	public static void RegisterInvalidatingField(object target, string fieldName)
	{
		Type type = target.GetType();
		FieldInfo? info = GetFieldInHierarchy(type, fieldName, BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		if (info == null)
		{
			throw new ArgumentException($"Field {fieldName} not found.");
		}
		_entries.Add(new Entry(target, info.GetValue));
		_entriesChanged = true;
	}

	public static void RegisterInvalidatingProperty(object target, string propertyName)
	{
		Type type = target.GetType();
		PropertyInfo? info = GetPropertyInHierarchy(type, propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		if (info == null)
		{
			throw new ArgumentException($"Property {propertyName} not found.");
		}
		_entries.Add(new Entry(target, info.GetValue));
		_entriesChanged = true;
	}

	public static void RegisterInvalidatingStaticField(Type type, string fieldName)
	{
		FieldInfo? info = GetFieldInHierarchy(type, fieldName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		if (info == null)
		{
			throw new ArgumentException($"Static field {fieldName} not found.");
		}
		_entries.Add(new Entry(null, info.GetValue));
		_entriesChanged = true;
	}

	public static void RegisterInvalidatingStaticProperty(Type type, string propertyName)
	{
		PropertyInfo? info = GetPropertyInHierarchy(type, propertyName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		if (info == null)
		{
			throw new ArgumentException($"Static property {propertyName} not found.");
		}
		_entries.Add(new Entry(null, info.GetValue));
		_entriesChanged = true;
	}

	private static FieldInfo? GetFieldInHierarchy(Type? type, string fieldName, BindingFlags flags)
	{
		while (type != null)
		{
			FieldInfo? info = type.GetField(fieldName, flags);
			if (info != null)
			{
				return info;
			}
			type = type?.BaseType;
		}
		return null;
	}

	private static PropertyInfo? GetPropertyInHierarchy(Type? type, string propertyName, BindingFlags flags)
	{
		while (type != null)
		{
			PropertyInfo? info = type.GetProperty(propertyName, flags);
			if (info != null)
			{
				return info;
			}
			type = type.BaseType;
		}
		return null;
	}

	static InvalidationManager()
	{
		_entries = new List<Entry>();
		_entriesChanged = true;
		_invalidated = true;
	}

	private static readonly List<Entry> _entries;
	private static bool _entriesChanged;
	private static bool _invalidated;

	private class Entry
	{
		public Entry(object? target, Func<object?, object?> getValue)
		{
			if (target != null)
			{
				_targetReference = new WeakReference(target);
			}
			_getValue = getValue;
			_lastValue = getValue(target);
		}

		public bool IsAlive()
		{
			return _targetReference?.IsAlive ?? true;
		}

		public bool HasValueChanged()
		{
			object? target = _targetReference?.Target;
			if (target != null)
			{
				object? value = _getValue(target);
				if (!Equals(value, _lastValue))
				{
					_lastValue = value;
					return true;
				}
			}
			return false;
		}

		private readonly WeakReference? _targetReference;
		private readonly Func<object?, object?> _getValue;
		private object? _lastValue;
	}

	public static void WriteDebugMessage(string message)
	{
		AllocConsole();
		Console.WriteLine(Time.GetTime() + ": " + message);
	}

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern bool AllocConsole();
}

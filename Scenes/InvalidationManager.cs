namespace ChessPanel.Scenes;

using System;
using System.Collections.Generic;
using System.Reflection;

public static class InvalidationManager
{
	public static bool IsInvalidated(object obj)
	{
		return _staticInvalidated || _changedObjects.Contains(obj);
	}

	public static bool IsInvalidated()
	{
		return _entriesChanged || _invalidated;
	}

	public static void ForceRender()
	{
		_entriesChanged = true;
	}

	public static void ForceInvalidate()
	{
		_entriesChanged = true;
		_staticChanged = true;
	}

	public static void ForceInvalidate(object obj)
	{
		_entriesChanged = true;
		_invalidatedObjects.Add(obj);
	}

	public static void Update()
	{
		_invalidated = _entriesChanged;
		_entriesChanged = false;
		_staticInvalidated = _staticChanged;
		_staticChanged = false;
		_entries.RemoveAll(entry => !entry.IsAlive());
		_changedObjects.Clear();
		foreach (Entry entry in _entries)
		{
			if (entry.HasValueChanged())
			{
				object? target = entry.GetObject();
				if (target != null)
				{
					_changedObjects.Add(target);
				}
				else
				{
					_staticChanged = true;
				}
				_invalidated = true;
			}
		}
		_changedObjects.UnionWith(_invalidatedObjects);
		_invalidatedObjects.Clear();
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
		_changedObjects = new HashSet<object>();
		_invalidatedObjects = new HashSet<object>();
		_entries = new List<Entry>();
		_entriesChanged = true;
		_invalidated = true;
	}

	private static readonly HashSet<object> _changedObjects;
	private static readonly HashSet<object> _invalidatedObjects;
	private static readonly List<Entry> _entries;
	private static bool _entriesChanged;
	private static bool _invalidated;
	private static bool _staticChanged;
	private static bool _staticInvalidated;

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

		public object? GetObject()
		{
			return _targetReference?.Target;
		}

		public bool IsAlive()
		{
			return _targetReference?.IsAlive ?? true;
		}

		public bool HasValueChanged()
		{
			if (_targetReference != null && _targetReference.Target == null)
			{
				return false;
			}
			object? target = _targetReference?.Target;
			object? value = _getValue(target);
			if (!Equals(value, _lastValue))
			{
				_lastValue = value;
				return true;
			}
			return false;
		}

		private readonly WeakReference? _targetReference;
		private readonly Func<object?, object?> _getValue;
		private object? _lastValue;
	}
}

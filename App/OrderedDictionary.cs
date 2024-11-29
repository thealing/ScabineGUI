namespace Scabine.App;

using System.Collections.Generic;

internal class OrderedDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IDictionary<TKey, TValue> where TKey : notnull
{
	private readonly List<TKey> _keys = new();

	public new TValue this[TKey key]
	{
		get => base[key];
		set
		{
			if (!base.ContainsKey(key))
			{
				_keys.Add(key);
			}
			base[key] = value;
		}
	}

	public new ICollection<TKey> Keys => _keys.AsReadOnly();

	public new ICollection<TValue> Values => _keys.ConvertAll(key => base[key]).AsReadOnly();

	public new void Add(TKey key, TValue value)
	{
		base.Add(key, value);
		_keys.Add(key);
	}

	public void Add(KeyValuePair<TKey, TValue> item)
	{
		Add(item.Key, item.Value);
	}

	public new bool Remove(TKey key)
	{
		if (!base.Remove(key))
		{
			return false;
		}
		_keys.Remove(key);
		return true;
	}

	public new void Clear()
	{
		base.Clear();
		_keys.Clear();
	}

	public new IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
	{
		foreach (var key in _keys)
		{
			yield return new KeyValuePair<TKey, TValue>(key, base[key]);
		}
	}
}


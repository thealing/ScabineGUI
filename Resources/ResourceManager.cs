namespace Scabine.Resources;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Reflection;

public static class ResourceManager
{
	public static Image LoadImageResource(string name)
	{
		Stream? stream = LoadResource(name);
		if (stream == null)
		{
			throw CreateInvalidResourceNameException(name);
		}
		return Image.FromStream(stream);
	}

	public static SoundPlayer LoadSoundResource(string name)
	{
		Stream? stream = LoadResource(name);
		if (stream == null)
		{
			throw CreateInvalidResourceNameException(name);
		}
		return new SoundPlayer(stream);
	}

	public static Stream? LoadResource(string name)
	{
		return _assembly.GetManifestResourceStream($"{typeof(ResourceManager).Namespace}.{name}");
	}

	public static IEnumerable<string> EnumerateSubfolders(string name)
	{
		string baseNamespace = $"{typeof(ResourceManager).Namespace}.{name}";
		return _assembly.GetManifestResourceNames().Where(s => s.StartsWith(baseNamespace)).Select(s => s.Substring(baseNamespace.Length + 1).Split('.')[0]).Distinct();
	}

	private static Exception CreateInvalidResourceNameException(string name)
	{
		return new ArgumentException("Invalid resource name: " + name);
	}

	static ResourceManager()
	{
		_assembly = Assembly.GetExecutingAssembly();
	}

	private readonly static Assembly _assembly;
}

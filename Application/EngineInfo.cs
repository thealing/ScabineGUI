namespace ChessBand.Application;

using System;
using System.Collections.Generic;
using ChessBand.Engines;

internal class EngineInfo : EngineParams
{
	public string Hash;
	public IDictionary<string, EngineOptions> Presets;

	public EngineInfo(string path, string hash, string name, string author)
		: base(path, name, author)
	{
		Hash = hash;
		Arguments = "";
		Commands = Array.Empty<string>();
		Presets = new OrderedDictionary<string, EngineOptions>();
	}
}

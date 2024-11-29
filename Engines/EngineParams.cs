namespace Scabine.Engines;

using System;

public class EngineParams
{
	public string Path;
	public string Name;
	public string Author;
	public string Arguments;
	public string[] Commands;

	public EngineParams(string path, string name, string author)
	{
		Path = path;
		Name = name;
		Author = author;
		Arguments = "";
		Commands = Array.Empty<string>();
	}
}

# CliUtility

Provides command line argument parsing, usage screen generation, word wrapping and stale file checking useful for CLI apps

A simple word wrapping application:
```cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using Cli;

[assembly: AssemblyDescription("An example of command line argument processing")]

internal class Program
{
	[CmdArg(Ordinal = 0)]
	static TextReader[] inputs = { Console.In };
	[CmdArg(Optional = true, Description = "The width to wrap to in characters")]
	static int wrap = Console.WindowWidth;
	static void Main()
	{
		try
		{

			using (var result = CliUtility.ParseValidateAndSet(typeof(Program)))
			{
				foreach (var input in inputs)
				{
					Console.WriteLine();
					Console.WriteLine(CliUtility.WordWrap(input.ReadToEnd(), wrap));

				}
			}
		}
		catch (Exception ex)
		{
			Console.Error.WriteLine(ex.Message);

		}
	}
}
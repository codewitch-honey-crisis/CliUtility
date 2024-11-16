# CliUtility

Provides command line argument parsing, usage screen generation, word wrapping and stale file checking useful for CLI apps

Nuget: https://www.nuget.org/packages?q=htcw_CliUtility

## Command Line Parsing

There are two ways to do command line argument definitions. One way is to build a list of `CmdSwitch` instances.
The other way is to use the `[CmdArg(...)]` attribute to mark up static fields or properties on a type (typically your main Program class). Those fields can then be automatically filled in by this library.

the function `ParseArguments()` can be used to retrieve the parsed information or `ParseAndSet()` if using the `[CmdArg()]` method which handles reflecting, parsing, and setting the fields.

## Simple example
A simple word wrapping application that takes a series of input files and an optional switch "wrap" that specifies the columns to wrap to:
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

			using (var result = CliUtility.ParseAndSet(typeof(Program)))
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
```

The following using screen is produced:

```
Example v1.0.0.0

An example of command line argument processing

Usage: Example { <inputfile1>,  <inputfile2>, ... } [ /wrap <item> ]

    <inputfile> The input file. Defaults to <stdin>
    <item>      The width to wrap to in characters. Defaults to 120
```
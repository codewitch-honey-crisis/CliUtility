# CliUtility

Provides command line argument parsing, usage screen generation, word wrapping and stale file checking useful for CLI apps

A simple word wrapping application:
```cs
using Cli;
internal class Program
{
	[CmdArg(Ordinal = 0)]
	static TextReader[] inputs = { Console.In };
	[CmdArg(Optional = true, Description = "The width to wrap to in characters")]
	static int wrap = Console.WindowWidth;
	static void Main()
	{
		var switches = CliUtility.GetSwitches(typeof(Program));
		CliUtility.PrintUsage(switches, wrap);
		using (var result = CliUtility.ParseArguments(switches))
		{
			CliUtility.SetValues(switches, result,typeof(Program));
			foreach (var input in inputs)
			{
				Console.WriteLine();
				Console.WriteLine(CliUtility.WordWrap(input.ReadToEnd(), wrap));

			}
		}
	}
}
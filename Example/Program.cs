
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using Cli;
[assembly: AssemblyDescription("An example of command line argument processing")]

namespace Example
{

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
		/*
				static void Main()
				{
					var switches = new List<CmdSwitch>();

					var sw = CmdSwitch.Empty;
					sw.Ordinal = 1;
					sw.Optional = false;
					sw.ElementName = "inputfile";
					sw.Type = CmdSwitchType.List;
					sw.Description = "The input files";
					sw.Default = new TextReader[] { Console.In };
					switches.Add(sw);

					sw = CmdSwitch.Empty;
					sw.Ordinal = -1;
					sw.Name = "bool";
					sw.Type = CmdSwitchType.Simple;
					sw.Description = "The bool argument";
					switches.Add(sw);

					sw = CmdSwitch.Empty;
					sw.Ordinal = -1;
					sw.Name = "string";
					sw.ElementName = "value";
					sw.Optional = true;
					sw.Type = CmdSwitchType.OneArg;
					sw.Description = "The string argument";
					switches.Add(sw);

					sw = CmdSwitch.Empty;
					sw.Ordinal = -1;
					sw.Name = "strings";
					sw.ElementName = "value";
					sw.Optional = true;
					sw.Type = CmdSwitchType.List;
					sw.Description = "The strings argument";
					switches.Add(sw);

					sw = CmdSwitch.Empty;
					sw.Ordinal = -1;
					sw.Name = "int";
					sw.Optional = true;
					sw.ElementType = typeof(int);
					sw.ElementName = "number";
					sw.Type = CmdSwitchType.OneArg;
					sw.Description = "The int argument";
					switches.Add(sw);

					using (var result = CliUtility.ParseArguments(switches))
					{
						CliUtility.PrintUsage(switches);
					}
				}
		*/
	}
}

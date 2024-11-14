// if defined use manual command parsing
#define MANUAL
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using Cli;

namespace Example
{

	internal class Program
	{
		// automatic usage
		[CmdArg(Ordinal = 0)]
		static TextReader[] inputs = { Console.In };
		[CmdArg(Optional = true, Description = "The width to wrap to in characters. Defaults to the terminal width",ElementName ="columns")]
		static int wrap = Console.WindowWidth;
		static void MainAuto()
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
		// manual usage
		static void MainManual()
		{
			var switches = new List<CmdSwitch>();

			var sw = CmdSwitch.Empty;
			sw.Ordinal = 0;
			sw.Optional = false;
			sw.ElementName = "inputfile";
			sw.ElementType = typeof(TextReader);
			sw.Type = CmdSwitchType.List;
			sw.Default = new TextReader[] { Console.In };
			switches.Add(sw);

			sw = CmdSwitch.Empty;
			sw.Ordinal = -1;
			sw.Name = "wrap";
			sw.Optional = true;
			sw.Type = CmdSwitchType.OneArg;
			sw.ElementType = typeof(int);
			sw.Description = "The width to wrap to in characters. Defaults to the terminal width";
			sw.ElementName = "columns";
			switches.Add(sw);
			try
			{

				using (var result = CliUtility.ParseArguments(switches))
				{
					foreach (var input in (TextReader[])result.OrdinalArguments[0])
					{
						Console.WriteLine();
						Console.WriteLine(CliUtility.WordWrap(input.ReadToEnd(), wrap));

					}
				}
			}
			catch (Exception ex)
			{
				CliUtility.PrintUsage(switches);
				Console.Error.WriteLine(ex.Message);

			}
			
		}
		static void Main()
		{
#if MANUAL
			MainManual();
#else
			MainAuto();
#endif
		}
	}
}

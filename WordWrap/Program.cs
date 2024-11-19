// if defined use manual command parsing
//#define MANUAL
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Reflection;

using Cli;

namespace Example
{
	class CommandLine
	{
		// automatic usage
		[CmdArg(Ordinal = 0, Optional = true)] // argument ordinal 0. The description is filled automatically in this case
		public List<TextReader> Inputs { get; private set; } = new List<TextReader>() { Console.In }; // defaults to stdin. Is a list, so it takes multiple values
																				// Ordinal not specified, so the name of the switch is taken from the field
																				// We're using the custom type converter, and a description
		[CmdArg(Name = "wrap", Optional = true, ElementConverter = "Example.WrapConverter", Description = "The width to wrap to in characters or \"none\". Defaults to the terminal width", ElementName = "columns")]
		// it's an integer that defaults to the console width
		public int Wrap { get; private set; } = Console.WindowWidth;
		[CmdArg(Name = "help",Description = "Displays this screen", Group = "help")]
		public bool Help { get; private set; } = false;
	}
	// we want to accept a positive integer or none
	class WrapConverter : Int32Converter
	{
		private const string msg = "The value must be a positive integer or \"none\".";
		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			
			if (value is string vs)
			{
				if (vs == "none")
				{
					return 0;
				} else
				{
					value = base.ConvertFrom(context, culture, value);
					if(value is int vi)
					{
						if (vi < 1)
						{
							throw new ArgumentException(msg);
						}
						return vi;
					} else
					{
						return value;
					}
					
				}
			}
			return base.ConvertFrom(context, culture, value);
		}
		
	}
	internal class Program
	{
		
		static void MainAuto(string[] args)
		{
#if !DEBUG
			try
			{
#endif
				var cl = new CommandLine();
				// parse the arguments and set the fields on Program
				using (var result = CliUtility.ParseAndSet(args, cl))
				{
					if(cl.Help) { CliUtility.PrintUsage(CliUtility.GetSwitches(cl)); return; }
					// get our inputs
					foreach (var input in cl.Inputs)
					{
						// write each one to the console
						Console.WriteLine();
						if (cl.Wrap > 0)
						{
							Console.WriteLine(CliUtility.WordWrap(input.ReadToEnd(), cl.Wrap));
						} else
						{
							Console.WriteLine(input.ReadToEnd());
						}
					}
				}
#if !DEBUG
			}
			catch (Exception ex)
			{
				// on error, write the message and exit. Usage will be printed as well.
				Console.Error.WriteLine(ex.Message);


			}
#endif
		}
		// manual usage
		static void MainManual(string[] args)
		{
			var switches = new List<CmdSwitch>();

			var sw = CmdSwitch.Empty;
			// argument position 0. (unnamed)
			sw.Ordinal = 0;
			// required argument
			sw.Optional = false;
			// the type of the element for the argument
			sw.ElementType = typeof(TextReader);
			// list type. This means an array of elements
			sw.Type = CmdSwitchType.List;
			// the default value, single textreader, stdin in an array
			sw.Default = new TextReader[] { Console.In };
			switches.Add(sw);

			sw = CmdSwitch.Empty;
			// named argument (no ordinal position)
			sw.Ordinal = -1;
			// argument switch is "wrap"
			sw.Name = "wrap";
			// argument not required
			sw.Optional = true;
			// switch w/ single value
			sw.Type = CmdSwitchType.OneArg;
			// defaults to the console width
			sw.Default = Console.WindowWidth;
			// it's an integer element
			sw.ElementType = typeof(int);
			// custom type converter so we can accept "none" or an integer
			sw.ElementConverter = new WrapConverter();
			// because description contains "default" no automatic default will be generated for the description.
			sw.Description = "The width to wrap to in characters or \"none\". Defaults to the terminal width";
			// argument element is # of columns
			sw.ElementName = "columns";
			switches.Add(sw);

			sw = CmdSwitch.Empty;
			// named argument (no ordinal position)
			sw.Ordinal = -1;
			// argument switch is "help"
			sw.Name = "help";
			// argument required
			sw.Optional = false;
			// switch w/ no value
			sw.Type = CmdSwitchType.Simple;
			// because description contains "default" no automatic default will be generated for the description.
			sw.Description = "Displays this screen";
			// argument is part of the "help" group
			sw.Group = "help";
			switches.Add(sw);

#if !DEBUG
			try
			{
#endif
			// parse the arguments into a CmdParseResult
			using (var result = CliUtility.ParseArguments(switches,args))
				{
					if(result.Group=="help") { CliUtility.PrintUsage(switches); return; }
					// get our input textreader array (argument ordinal 0)
					foreach (var input in (TextReader[])result.OrdinalArguments[0])
					{
						// write each one to the console
						Console.WriteLine();
						var w = (int)result.NamedArguments["wrap"];
						if (w > 0)
						{
							Console.WriteLine(CliUtility.WordWrap(input.ReadToEnd(), w));
						}
						else
						{
							Console.WriteLine(input.ReadToEnd());
						}
					}
				}
#if !DEBUG
			}
			catch (Exception ex)
			{
				// on error print usage, the exception, and exit
				CliUtility.PrintUsage(switches);
				Console.Error.WriteLine();
				Console.Error.WriteLine(ex.Message);

			}
#endif
		}
		static void Main(string[] args)
		{
#if MANUAL
			MainManual(args);
#else
			MainAuto(args);
#endif
		}
	}
}

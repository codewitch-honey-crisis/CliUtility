// if defined use manual command parsing
// #define MANUAL
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Reflection;

using Cli;

namespace Example
{
	
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
		
		// automatic usage
		[CmdArg(Ordinal = 0)] // argument ordinal 0. The description is filled automatically in this case
		static List<TextReader> inputs = new List<TextReader>() { Console.In }; // defaults to stdin. Is an array, so it takes multiple values
		// Ordinal not specified, so the name of the switch is taken from the field
		// We're using the custom type converter, and a description
		[CmdArg(Optional = true, ElementConverter = "Example.WrapConverter", Description = "The width to wrap to in characters or \"none\". Defaults to the terminal width",ElementName ="columns")]
		// it's an integer that defaults to the console width
		static int wrap = Console.WindowWidth;
		static void MainAuto(string[] args)
		{
#if !DEBUG
			try
			{
#endif
				// parse the arguments and set the fields on Program
				using (var result = CliUtility.ParseValidateAndSet(typeof(Program),args))
				{
					// get our inputs
					foreach (var input in inputs)
					{
						// write each one to the console
						Console.WriteLine();
						if (wrap > 0)
						{
							Console.WriteLine(CliUtility.WordWrap(input.ReadToEnd(), wrap));
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
#if !DEBUG
			try
			{
#endif
				// parse the arguments into a CmdParseResult
				using (var result = CliUtility.ParseArguments(switches,args))
				{
					// get our input textreader array (argument ordinal 0)
					foreach (var input in (TextReader[])result.OrdinalArguments[0])
					{
						// write each one to the console
						Console.WriteLine();
						if (wrap > 0)
						{
							Console.WriteLine(CliUtility.WordWrap(input.ReadToEnd(), wrap));
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

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

	class WrapConverter : Int32Converter
	{
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			if (sourceType == typeof(String))
			{
				return true;
			}
			return base.CanConvertFrom(context, sourceType);
		}
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			if (destinationType == typeof(String))
			{
				return true;
			}
			return base.CanConvertTo(context, destinationType);
		}
		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			if (value is string)
			{
				if (((string)value) == "none")
				{
					return 0;
				}
			}
			else if (value is int)
			{
				if (((int)value) == 0)
				{
					return "none";
				}
				return value.ToString();
			}

			return base.ConvertFrom(context, culture, value);
		}
		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (value is string)
			{
				if (((string)value) == "none")
				{
					return 0;
				}
			}
			else if (value is int)
			{
				if (((int)value) == 0)
				{
					return "none";
				}
				return value.ToString();
			}
			return base.ConvertTo(context, culture, value, destinationType);
		}
	}
	internal class Program
	{
		
		// automatic usage
		[CmdArg(Ordinal = 0)]
		static TextReader[] inputs = { Console.In };
		[CmdArg(Optional = true, ElementConverter = "Example.WrapConverter", Description = "The width to wrap to in characters or \"none\". Defaults to the terminal width",ElementName ="columns")]
		static int wrap = Console.WindowWidth;
		static void MainAuto(string[] args)
		{
			try
			{

				using (var result = CliUtility.ParseValidateAndSet(typeof(Program),args))
				{
					foreach (var input in inputs)
					{
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
			}
			catch (Exception ex)
			{
				Console.Error.WriteLine(ex.Message);

			}
			
		}
		// manual usage
		static void MainManual(string[] args)
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
			sw.Default = Console.WindowWidth;
			sw.ElementType = typeof(int);
			sw.ElementConverter = new WrapConverter();
			sw.Description = "The width to wrap to in characters or \"none\". Defaults to the terminal width";
			sw.ElementName = "columns";
			switches.Add(sw);
			try
			{

				using (var result = CliUtility.ParseArguments(switches,args))
				{
					foreach (var input in (TextReader[])result.OrdinalArguments[0])
					{
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
			}
			catch (Exception ex)
			{
				CliUtility.PrintUsage(switches);
				Console.Error.WriteLine(ex.Message);

			}
			

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

# CliUtility

Provides command line argument parsing, usage screen generation, word wrapping and stale file checking useful for CLI apps

Nuget: https://www.nuget.org/packages?q=htcw_CliUtility

## Command Line Parsing

There are two ways to do command line argument definitions. One way is to build a list of `CmdSwitch` instances.
The other way is to use the `[CmdArg(...)]` attribute to mark up static fields or properties on a type (typically your main Program class). Those fields can then be automatically filled in by this library.

the function `ParseArguments()` can be used to retrieve the parsed information or `ParseValidateAndSet()` if using the `[CmdArg()]` method which handles reflecting, parsing, and setting the fields.

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
	[CmdArg(Optional = true, Description = "The width to wrap to in characters", ElementName ="columns")]
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
```

The following using screen is produced:

```
Example v1.0.0.0

An example of command line argument processing

Usage: Example { <inputfile1>,  <inputfile2>, ... } [ /wrap <columns> ]

    <inputfile> The input file. Defaults to <stdin>
    <columns>      The width to wrap to in characters. Defaults to 120
```

## More Complete Example (Automatic/Reflection usage)

```cs
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Reflection;
using Cli;
```

In order to provide for a custom conversion for the /wrap (--wrap) switch that can accept "none" or a positive integer, we use the .NET `TypeConverter` infrastructure:

```cs
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
```
This derives from an existing type converter `Int32Converter` which can convert from a string to an integer. We want to use the basic functionality of it, but slightly alter it so it will accept `none` and return `0` for that, or otherwise throw an error if it's less than `1`.

```cs
internal class Program
{
		
	// automatic usage
	[CmdArg(Ordinal = 0)] // argument ordinal 0. The description is filled automatically in this case
	static TextReader[] inputs = { Console.In }; // defaults to stdin. Is an array, so it takes multiple values
	// Ordinal not specified, so the name of the switch is taken from the field
	// We're using the custom type converter, and a description
	[CmdArg(Optional = true, ElementConverter = "WrapConverter", Description = "The width to wrap to in characters or \"none\". Defaults to the terminal width",ElementName ="columns")]
	// it's an integer that defaults to the console width
	static int wrap = Console.WindowWidth;
	static void Main(string[] args)
	{
		try
		{
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
		}
		catch (Exception ex)
		{
			// on error, write the message and exit. Usage will be printed as well.
			Console.Error.WriteLine(ex.Message);

		}
	}
}
```
Above we've specified the typename of the converter to the `ElementConverter` parameter. If the type is not in the entry assembly and not intrinsic, you will need to give the assembly qualified name.

Note that we manually added the information about the default value. The code will detect the word "default" present in the description and it will not generate a default value description if it finds it.

## Advanced Example (Manual usage)

Rather than reflecting off of fields using `CmdArgAttribute` you can define the switches yourself. The following is equivalent to the above (the WrapConverter has been omitted below, but is the same as above):

```cs
internal class Program
{
    // manual usage
    static void Main(string[] args)
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
        try
        {
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
        }
        catch (Exception ex)
        {
            // on error print usage, the exception, and exit
            CliUtility.PrintUsage(switches);
            Console.Error.WriteLine(ex.Message);

        }
    }
}
```

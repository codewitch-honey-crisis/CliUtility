using Cli;
namespace CheckStale
{
	internal class Program
	{
		[CmdArg(Ordinal =0, Optional = true)]
		static TextReader input = Console.In;
		[CmdArg(Optional = true)]
		static TextWriter output = Console.Out;
		static void Main(string[] args)
		{
			using (var result = CliUtility.ParseValidateAndSet(typeof(Program), args))
			{
				if (CliUtility.IsStale(input, output))
				{
					var fni = CliUtility.GetFilename(input);
					fni ??= "<stdin>";
					var fno = CliUtility.GetFilename(output);
					fno ??= "<stdout>";
					Console.Error.WriteLine("Copying \"" + fni + "\" to \"" + fno + "\".");
					// line at a time so works with stdin
					var line = input.ReadLine();
					while (line != null)
					{
						output.WriteLine(line);
						line = input.ReadLine();
					}
					Console.Error.WriteLine("Done");
				} else
				{
					Console.Error.WriteLine("Skipping copy because input has not changed");
				}
			}
		}
	}
}

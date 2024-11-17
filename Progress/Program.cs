using Cli;
const int iterations = 10;
for(var i = 0;i<iterations;++i)
{
	// i can be any value as long
	// as you keep incrementing it
	// second arg must be false 
	// on the first call, otherwise
	// true
	CliUtility.WriteProgress(i, i != 0);
	Thread.Sleep(100);
}
Console.WriteLine();
for (var i = 0; i < iterations; ++i)
{
	// i should be 0 to 100
	// second arg must be false 
	// on the first call, otherwise
	// true
	CliUtility.WriteProgressBar(i*(100/iterations), i != 0);
	Thread.Sleep(100);
}
// write 100% explicitly since the loop
// doesn't get to 100%
CliUtility.WriteProgressBar(100, true);
Console.WriteLine();
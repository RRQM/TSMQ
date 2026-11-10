using BenchmarkConsoleApp;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;

var quick = args.Contains("--quick");
var config = DefaultConfig.Instance;
if (quick)
{
    config = config.AddJob(Job.ShortRun
        .WithWarmupCount(3)
                .WithIterationCount(5)
                .WithInvocationCount(1)
                .WithUnrollFactor(1)
        .WithId("Quick"));
}
else
{
    config = config.AddJob(Job.Default.WithInvocationCount(1).WithUnrollFactor(1));
}
var benchmarkArgs = args.Where(argument => argument != "--quick").ToArray();
if (benchmarkArgs.Length == 0)
{
    benchmarkArgs = ["--filter", "*"];
}
var summaries = BenchmarkSwitcher.FromAssembly(typeof(BenchmarkMqtt).Assembly).Run(benchmarkArgs, config);
if (summaries.Any(summary => summary.Reports.Any(report => !report.Success)))
{
    Environment.ExitCode = 1;
}

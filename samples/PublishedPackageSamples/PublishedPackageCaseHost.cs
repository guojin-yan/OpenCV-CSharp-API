using System;
using System.Linq;
using System.Reflection;

namespace JYPPX.OpenCvSharp.Samples.ConsoleSamples
{
    internal static class PublishedPackageCaseHost
    {
        public static void Run(string[] args)
        {
            string packageVersion = Assembly.GetExecutingAssembly()
                .GetCustomAttributes<AssemblyMetadataAttribute>()
                .Single(attribute => attribute.Key == "PublishedOpenCvPackageVersion")
                .Value ?? "unknown";
            Console.WriteLine("Published packages: " + packageVersion);
            Console.WriteLine("Managed namespace: JYPPX.OpenCvSharp");
            Console.WriteLine("Runtime package: JYPPX.OpenCV.runtime.win-x64");

            string[] runnerArgs = args.Length > 0 &&
                (string.Equals(args[0], "tutorial", StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(args[0], "showcase", StringComparison.OrdinalIgnoreCase))
                ? args.Skip(1).ToArray()
                : args;
            ShowcaseRunner.Run(runnerArgs.Length == 0 ? new[] { "all" } : runnerArgs);
        }
    }
}

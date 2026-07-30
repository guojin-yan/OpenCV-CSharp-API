using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

internal static class Program
{
    private const string CurrentNativeLibrary = "JYPPX.OpenCV.Native";
    private const string NeutralPrefix = "jyppx_ocv_";
    private static readonly StringComparer Ordinal = StringComparer.Ordinal;

    private sealed record Options(
        string RepositoryRoot,
        string AssemblyPath,
        string ManifestPath,
        string SourceRoot,
        string OutputPath,
        string SummaryPath,
        bool Check);

    private sealed record ImportMethod(string EntryPoint, string Method);
    private sealed record SourceReference(string EntryPoint, string Reference);
    private sealed record BindingRow(string EntryPoint, string Classification, string Methods, string Sources);

    private sealed class MappingSummary
    {
        public int SchemaVersion { get; init; } = 1;
        public string Generator { get; init; } = "tools/NativeManagedBindingMap";
        public string AssemblyName { get; init; } = string.Empty;
        public string TargetFramework { get; init; } = string.Empty;
        public string NativeManifestPath { get; init; } = string.Empty;
        public string ManagedSourceRoot { get; init; } = string.Empty;
        public string MappingPath { get; init; } = string.Empty;
        public string MappingSha256 { get; init; } = string.Empty;
        public int NativeFunctionCount { get; init; }
        public int ManagedEntryPointCount { get; init; }
        public int ManagedImportMethodCount { get; init; }
        public int ManagedSourceDeclarationCount { get; init; }
        public int ManagedBoundCount { get; init; }
        public int NativeInfrastructureCount { get; init; }
        public int CompatibilityOnlyCount { get; init; }
        public int UnboundCount { get; init; }
        public int ManagedOnlyCount { get; init; }
    }

    private static int Main(string[] args)
    {
        try
        {
            Options options = ParseOptions(args);
            string repositoryRoot = Path.GetFullPath(options.RepositoryRoot);
            string assemblyPath = Path.GetFullPath(options.AssemblyPath);
            string manifestPath = Path.GetFullPath(options.ManifestPath);
            string sourceRoot = Path.GetFullPath(options.SourceRoot);
            string outputPath = Path.GetFullPath(options.OutputPath);
            string summaryPath = Path.GetFullPath(options.SummaryPath);

            string[] manifestFunctions = ReadManifestFunctions(manifestPath);
            Assembly assembly = Assembly.LoadFrom(assemblyPath);
            ImportMethod[] imports = ReadManagedImports(assembly);
            SourceReference[] sourceReferences = ReadSourceReferences(repositoryRoot, sourceRoot);

            Dictionary<string, string[]> methodsByEntryPoint = imports
                .GroupBy(value => value.EntryPoint, Ordinal)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(value => value.Method).Distinct(Ordinal).OrderBy(value => value, Ordinal).ToArray(),
                    Ordinal);
            Dictionary<string, string[]> sourcesByEntryPoint = sourceReferences
                .GroupBy(value => value.EntryPoint, Ordinal)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(value => value.Reference).Distinct(Ordinal).OrderBy(value => value, Ordinal).ToArray(),
                    Ordinal);

            var rows = new List<BindingRow>(manifestFunctions.Length);
            foreach (string entryPoint in manifestFunctions)
            {
                methodsByEntryPoint.TryGetValue(entryPoint, out string[]? methods);
                sourcesByEntryPoint.TryGetValue(entryPoint, out string[]? sources);
                bool bound = methods is { Length: > 0 } && sources is { Length: > 0 };
                rows.Add(new BindingRow(
                    entryPoint,
                    bound ? "managed-bound" : "unbound",
                    bound ? string.Join(";", methods!) : "-",
                    bound ? string.Join(";", sources!) : "-"));
            }

            HashSet<string> nativeSet = manifestFunctions.ToHashSet(Ordinal);
            string[] managedOnly = methodsByEntryPoint.Keys
                .Where(entryPoint => !nativeSet.Contains(entryPoint))
                .OrderBy(entryPoint => entryPoint, Ordinal)
                .ToArray();

            string mappingText = BuildMappingText(rows, managedOnly, methodsByEntryPoint, sourcesByEntryPoint);
            var summary = new MappingSummary
            {
                AssemblyName = assembly.GetName().Name ?? string.Empty,
                TargetFramework = GetTargetFramework(assembly),
                NativeManifestPath = RelativePath(repositoryRoot, manifestPath),
                ManagedSourceRoot = RelativePath(repositoryRoot, sourceRoot),
                MappingPath = RelativePath(repositoryRoot, outputPath),
                MappingSha256 = Sha256(mappingText),
                NativeFunctionCount = manifestFunctions.Length,
                ManagedEntryPointCount = methodsByEntryPoint.Count,
                ManagedImportMethodCount = imports.Select(value => value.Method).Distinct(Ordinal).Count(),
                ManagedSourceDeclarationCount = sourceReferences.Length,
                ManagedBoundCount = rows.Count(value => value.Classification == "managed-bound"),
                NativeInfrastructureCount = rows.Count(value => value.Classification == "native-infrastructure"),
                CompatibilityOnlyCount = rows.Count(value => value.Classification == "compatibility-only"),
                UnboundCount = rows.Count(value => value.Classification == "unbound"),
                ManagedOnlyCount = managedOnly.Length
            };
            string summaryText = JsonSerializer.Serialize(summary, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true,
                NewLine = "\n"
            }) + "\n";

            WriteOrCheck(outputPath, mappingText, options.Check);
            WriteOrCheck(summaryPath, summaryText, options.Check);

            Console.WriteLine(
                "NATIVE_MANAGED_BINDING_MAP_OK native={0} bound={1} unbound={2} managed_only={3} imports={4} sha256={5} mode={6}",
                summary.NativeFunctionCount,
                summary.ManagedBoundCount,
                summary.UnboundCount,
                summary.ManagedOnlyCount,
                summary.ManagedImportMethodCount,
                summary.MappingSha256,
                options.Check ? "check" : "write");
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception.Message);
            return 1;
        }
    }

    private static Options ParseOptions(string[] args)
    {
        var values = new Dictionary<string, string>(Ordinal);
        bool check = false;
        for (int index = 0; index < args.Length; index++)
        {
            if (args[index] == "--check")
            {
                check = true;
                continue;
            }

            if (!args[index].StartsWith("--", StringComparison.Ordinal) || index + 1 >= args.Length)
            {
                throw new ArgumentException("Expected --name value arguments and optional --check.");
            }

            values.Add(args[index], args[++index]);
        }

        string Required(string name) => values.TryGetValue(name, out string? value) && !string.IsNullOrWhiteSpace(value)
            ? value
            : throw new ArgumentException("Missing required argument " + name + ".");

        return new Options(
            Required("--repository"),
            Required("--assembly"),
            Required("--manifest"),
            Required("--source-root"),
            Required("--output"),
            Required("--summary"),
            check);
    }

    private static string[] ReadManifestFunctions(string manifestPath)
    {
        string[] values = File.ReadAllLines(manifestPath)
            .Where(line => line.Length > 0 && !line.StartsWith("#", StringComparison.Ordinal) &&
                !line.StartsWith("[", StringComparison.Ordinal) && line.Contains('|'))
            .Select(line => line.Split('|')[0])
            .ToArray();
        if (values.Length == 0 || values.Distinct(Ordinal).Count() != values.Length)
        {
            throw new InvalidDataException("Native ABI manifest is empty or contains duplicate primary entrypoints.");
        }

        string[] sorted = values.OrderBy(value => value, Ordinal).ToArray();
        if (!values.SequenceEqual(sorted, Ordinal) || values.Any(value => !value.StartsWith(NeutralPrefix, StringComparison.Ordinal)))
        {
            throw new InvalidDataException("Native ABI manifest must use ordinal, version-neutral primary entrypoint ordering.");
        }

        return values;
    }

    private static ImportMethod[] ReadManagedImports(Assembly assembly)
    {
        var imports = new List<ImportMethod>();
        foreach (Type type in assembly.GetTypes().OrderBy(value => value.FullName, Ordinal))
        {
            foreach (MethodInfo method in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
                .Where(value => !value.Name.StartsWith("<", StringComparison.Ordinal))
                .OrderBy(MethodIdentity, Ordinal))
            {
                CustomAttributeData[] attributes = method.CustomAttributes
                    .Where(value => value.AttributeType.FullName is "System.Runtime.InteropServices.DllImportAttribute" or
                        "System.Runtime.InteropServices.LibraryImportAttribute")
                    .ToArray();
                if (attributes.Length == 0)
                {
                    continue;
                }

                string[] entryPoints = attributes.Select(ReadEntryPoint).Distinct(Ordinal).ToArray();
                string[] libraries = attributes.Select(ReadLibrary).Distinct(Ordinal).ToArray();
                if (entryPoints.Length != 1 || libraries.Length != 1 || libraries[0] != CurrentNativeLibrary)
                {
                    throw new InvalidDataException("Managed import metadata drifted for " + MethodIdentity(method) + ".");
                }
                if (!entryPoints[0].StartsWith(NeutralPrefix, StringComparison.Ordinal))
                {
                    throw new InvalidDataException("Managed import uses a non-neutral entrypoint: " + entryPoints[0] + ".");
                }

                imports.Add(new ImportMethod(entryPoints[0], MethodIdentity(method)));
            }
        }

        return imports.OrderBy(value => value.EntryPoint, Ordinal).ThenBy(value => value.Method, Ordinal).ToArray();
    }

    private static string ReadEntryPoint(CustomAttributeData attribute)
    {
        CustomAttributeNamedArgument argument = attribute.NamedArguments.SingleOrDefault(value => value.MemberName == "EntryPoint");
        return argument.TypedValue.Value as string ?? throw new InvalidDataException("Managed import is missing EntryPoint metadata.");
    }

    private static string ReadLibrary(CustomAttributeData attribute)
    {
        return attribute.ConstructorArguments.Count > 0 && attribute.ConstructorArguments[0].Value is string value
            ? value
            : throw new InvalidDataException("Managed import is missing native library metadata.");
    }

    private static SourceReference[] ReadSourceReferences(string repositoryRoot, string sourceRoot)
    {
        var references = new List<SourceReference>();
        var attributeRegex = new Regex(@"\[(?:DllImport|LibraryImport)\((?<args>[^\]]*)\)\]", RegexOptions.Singleline);
        var entryPointRegex = new Regex("EntryPoint\\s*=\\s*\"(?<name>[^\"]+)\"");
        foreach (string path in Directory.GetFiles(sourceRoot, "*.cs", SearchOption.AllDirectories).OrderBy(value => value, Ordinal))
        {
            if (path.Contains(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) ||
                path.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            string text = File.ReadAllText(path);
            foreach (Match attribute in attributeRegex.Matches(text))
            {
                Match entryPoint = entryPointRegex.Match(attribute.Groups["args"].Value);
                if (!entryPoint.Success)
                {
                    continue;
                }

                int line = 1;
                for (int index = 0; index < attribute.Index; index++)
                {
                    if (text[index] == '\n')
                    {
                        line++;
                    }
                }

                references.Add(new SourceReference(
                    entryPoint.Groups["name"].Value,
                    RelativePath(repositoryRoot, path) + ":" + line));
            }
        }

        return references.OrderBy(value => value.EntryPoint, Ordinal).ThenBy(value => value.Reference, Ordinal).ToArray();
    }

    private static string BuildMappingText(
        IReadOnlyList<BindingRow> rows,
        IReadOnlyList<string> managedOnly,
        IReadOnlyDictionary<string, string[]> methodsByEntryPoint,
        IReadOnlyDictionary<string, string[]> sourcesByEntryPoint)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# Native-to-managed binding map");
        builder.AppendLine("schema-version=1");
        builder.AppendLine("primary-prefix=jyppx_ocv_");
        builder.AppendLine("classification-order=managed-bound,native-infrastructure,compatibility-only,unbound");
        builder.AppendLine("[bindings]");
        foreach (BindingRow row in rows)
        {
            builder.Append(row.EntryPoint).Append('|')
                .Append(row.Classification).Append('|')
                .Append(row.Methods).Append('|')
                .AppendLine(row.Sources);
        }

        builder.AppendLine("[managed-only]");
        foreach (string entryPoint in managedOnly)
        {
            methodsByEntryPoint.TryGetValue(entryPoint, out string[]? methods);
            sourcesByEntryPoint.TryGetValue(entryPoint, out string[]? sources);
            builder.Append(entryPoint).Append('|')
                .Append(methods is { Length: > 0 } ? string.Join(";", methods) : "-").Append('|')
                .AppendLine(sources is { Length: > 0 } ? string.Join(";", sources) : "-");
        }

        return builder.ToString().Replace("\r\n", "\n", StringComparison.Ordinal);
    }

    private static string MethodIdentity(MethodInfo method)
    {
        string parameters = string.Join(",", method.GetParameters().Select(value => TypeIdentity(value.ParameterType)));
        return (method.DeclaringType?.FullName ?? string.Empty) + "." + method.Name + "(" + parameters + ")";
    }

    private static string TypeIdentity(Type type)
    {
        if (type.IsByRef)
        {
            return TypeIdentity(type.GetElementType()!) + "&";
        }
        if (type.IsPointer)
        {
            return TypeIdentity(type.GetElementType()!) + "*";
        }
        if (type.IsArray)
        {
            return TypeIdentity(type.GetElementType()!) + "[]";
        }
        return type.FullName ?? type.Name;
    }

    private static string GetTargetFramework(Assembly assembly)
    {
        CustomAttributeData? attribute = assembly.CustomAttributes.FirstOrDefault(value =>
            value.AttributeType.FullName == "System.Runtime.Versioning.TargetFrameworkAttribute");
        return attribute?.ConstructorArguments.FirstOrDefault().Value as string ?? string.Empty;
    }

    private static string RelativePath(string root, string path)
    {
        return Path.GetRelativePath(root, path).Replace('\\', '/');
    }

    private static string Sha256(string text)
    {
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))).ToLowerInvariant();
    }

    private static void WriteOrCheck(string path, string content, bool check)
    {
        if (check)
        {
            if (!File.Exists(path) || NormalizeNewLines(File.ReadAllText(path)) != NormalizeNewLines(content))
            {
                throw new InvalidDataException("Generated binding-map file is out of date: " + path);
            }
            return;
        }

        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, content, new UTF8Encoding(false));
    }

    private static string NormalizeNewLines(string value)
    {
        return value.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n');
    }
}

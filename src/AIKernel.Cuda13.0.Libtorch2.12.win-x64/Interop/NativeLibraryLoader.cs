namespace AIKernel.Cuda13.Libtorch2_12.WinX64.Interop;

using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;

internal static class NativeLibraryLoader
{
    private const string DefaultLibraryName = "libtorch_bridge.dll";
    private const string LoaderEnvironmentVariable =
        "AIKERNEL_CUDA13_LIBTORCH2_12_WIN_X64_LOADER";

    private static readonly object SyncRoot = new();
    private static IntPtr s_handle;

    internal static IntPtr Resolve(
        string libraryName)
    {
        if (!string.Equals(
            libraryName,
            NativeMethods.LibraryName,
            StringComparison.Ordinal) &&
            !string.Equals(
                libraryName,
                DefaultLibraryName,
                StringComparison.OrdinalIgnoreCase))
        {
            return IntPtr.Zero;
        }

        if (s_handle != IntPtr.Zero)
            return s_handle;

        lock (SyncRoot)
        {
            if (s_handle != IntPtr.Zero)
                return s_handle;

            var config = LoaderConfig.Load();
            RegisterRuntimeSearchPaths(config);

            foreach (var candidate in BuildNativeLibraryCandidates(config))
            {
                if (!File.Exists(candidate))
                    continue;

                s_handle = NativeLibrary.Load(candidate);
                return s_handle;
            }

            throw new DllNotFoundException(
                $"Unable to locate {DefaultLibraryName}. Set {LoaderEnvironmentVariable} " +
                "to a loader.json file or place the native bridge under runtimes/win-x64/native.");
        }
    }

    private static IEnumerable<string> BuildNativeLibraryCandidates(
        LoaderConfig config)
    {
        var nativeLibrary = string.IsNullOrWhiteSpace(config.NativeLibrary)
            ? DefaultLibraryName
            : config.NativeLibrary;

        foreach (var basePath in config.BasePaths())
        {
            yield return Path.Combine(basePath, nativeLibrary);

            if (!string.IsNullOrWhiteSpace(config.NativeAbiPath))
            {
                yield return Path.Combine(
                    basePath,
                    NormalizeSeparators(config.NativeAbiPath),
                    nativeLibrary);
            }
        }
    }

    private static void RegisterRuntimeSearchPaths(
        LoaderConfig config)
    {
        var paths = config.ResolveRuntimeSearchPaths()
            .Where(Directory.Exists)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (paths.Length == 0)
            return;

        if (OperatingSystem.IsWindows())
        {
            _ = SetDefaultDllDirectories(LoadLibrarySearchDefaultDirs);
            foreach (var path in paths)
            {
                _ = AddDllDirectory(path);
            }
        }

        var currentPath = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        var nextPath = string.Join(Path.PathSeparator, paths.Concat([currentPath]));
        Environment.SetEnvironmentVariable("PATH", nextPath);
    }

    private static string NormalizeSeparators(
        string value)
        => value.Replace('/', Path.DirectorySeparatorChar)
            .Replace('\\', Path.DirectorySeparatorChar);

    [DllImport("kernel32", SetLastError = true)]
    private static extern bool SetDefaultDllDirectories(
        int directoryFlags);

    [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern IntPtr AddDllDirectory(
        string newDirectory);

    private const int LoadLibrarySearchDefaultDirs = 0x00001000;

    private sealed record LoaderConfig(
        [property: JsonPropertyName("nativeLibrary")]
        string? NativeLibrary,
        [property: JsonPropertyName("nativeAbiPath")]
        string? NativeAbiPath,
        [property: JsonPropertyName("runtimeSearchPaths")]
        IReadOnlyList<string>? RuntimeSearchPaths,
        string LoaderDirectory)
    {
        internal static LoaderConfig Load()
        {
            var loaderPath = FindLoaderPath();
            if (loaderPath is null)
            {
                return new LoaderConfig(
                    DefaultLibraryName,
                    "runtimes/win-x64/native",
                    [],
                    AppContext.BaseDirectory);
            }

            var json = File.ReadAllText(loaderPath);
            var parsed = JsonSerializer.Deserialize<LoaderConfigDto>(json) ??
                new LoaderConfigDto();

            return new LoaderConfig(
                parsed.NativeLibrary,
                parsed.NativeAbiPath,
                parsed.RuntimeSearchPaths ?? [],
                Path.GetDirectoryName(loaderPath) ?? AppContext.BaseDirectory);
        }

        internal IEnumerable<string> BasePaths()
        {
            yield return AppContext.BaseDirectory;
            yield return LoaderDirectory;

            var assemblyDirectory = Path.GetDirectoryName(
                Assembly.GetExecutingAssembly().Location);
            if (!string.IsNullOrWhiteSpace(assemblyDirectory))
                yield return assemblyDirectory;
        }

        internal IEnumerable<string> ResolveRuntimeSearchPaths()
        {
            foreach (var rawPath in RuntimeSearchPaths ?? [])
            {
                var expanded = ExpandEnvironmentVariables(rawPath);
                if (string.IsNullOrWhiteSpace(expanded))
                    continue;

                var normalized = NormalizeSeparators(expanded);
                if (Path.IsPathFullyQualified(normalized))
                {
                    yield return normalized;
                    continue;
                }

                yield return Path.GetFullPath(Path.Combine(LoaderDirectory, normalized));
                yield return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, normalized));
            }
        }

        private static string ExpandEnvironmentVariables(
            string value)
        {
            var expanded = Environment.ExpandEnvironmentVariables(value);
            return expanded.Contains('%', StringComparison.Ordinal)
                ? string.Empty
                : expanded;
        }

        private static string? FindLoaderPath()
        {
            var explicitPath = Environment.GetEnvironmentVariable(LoaderEnvironmentVariable);
            if (!string.IsNullOrWhiteSpace(explicitPath) && File.Exists(explicitPath))
                return explicitPath;

            var candidates = new[]
            {
                Path.Combine(AppContext.BaseDirectory, "loader.json"),
                Path.Combine(AppContext.BaseDirectory, "AIKernel.Cuda13.0.Libtorch2.12.win-x64.loader.json"),
                Path.Combine(
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ??
                    AppContext.BaseDirectory,
                    "loader.json"),
                Path.Combine(Environment.CurrentDirectory, "loader.json")
            };

            return candidates.FirstOrDefault(File.Exists);
        }
    }

    private sealed class LoaderConfigDto
    {
        [JsonPropertyName("nativeLibrary")]
        public string? NativeLibrary { get; set; }

        [JsonPropertyName("nativeAbiPath")]
        public string? NativeAbiPath { get; set; }

        [JsonPropertyName("runtimeSearchPaths")]
        public IReadOnlyList<string>? RuntimeSearchPaths { get; set; }
    }
}

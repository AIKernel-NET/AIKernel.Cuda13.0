param(
    [string]$Configuration = "Release",
    [int]$LocalPackageBuildNumber = 1,
    [switch]$RequireFreshNativeBridge
)

$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$projectPath = Join-Path $repoRoot "src/AIKernel.Cuda13.0.Libtorch2.12.win-x64/AIKernel.Cuda13.0.Libtorch2.12.win-x64.csproj"
$outputDir = Join-Path $repoRoot "artifacts/package-smoke/v0.1.3-dev$LocalPackageBuildNumber"
$nativeBridgePath = Join-Path $repoRoot "native/build/win-x64/Release/libtorch_bridge.dll"
$nativeSourcePaths = @(
    (Join-Path $repoRoot "native/libtorch_bridge.cpp"),
    (Join-Path $repoRoot "native/libtorch_bridge.h")
)

if (Test-Path $nativeBridgePath) {
    $nativeBridge = Get-Item -LiteralPath $nativeBridgePath
    $newerSource = $nativeSourcePaths |
        Where-Object { Test-Path -LiteralPath $_ } |
        Get-Item |
        Where-Object { $_.LastWriteTimeUtc -gt $nativeBridge.LastWriteTimeUtc } |
        Select-Object -First 1

    if ($null -ne $newerSource) {
        $message = "native-bridge-freshness: stale; source $($newerSource.FullName) is newer than $nativeBridgePath"
        if ($RequireFreshNativeBridge) {
            throw $message
        }

        Write-Warning $message
    }
    else {
        Write-Host "native-bridge-freshness: ok"
    }
}
else {
    Write-Host "native-bridge-freshness: skipped; bridge not built"
}

New-Item -ItemType Directory -Force -Path $outputDir | Out-Null

dotnet pack $projectPath `
    --configuration $Configuration `
    --output $outputDir `
    /p:UseLocalPackageVersion=true `
    /p:LocalPackageBuildNumber=$LocalPackageBuildNumber

$packagePath = Get-ChildItem -Path $outputDir -Filter "AIKernel.Cuda13.0.Libtorch2.12.win-x64.*.nupkg" |
    Sort-Object LastWriteTime -Descending |
    Select-Object -First 1

if ($null -eq $packagePath) {
    throw "AIKernel.Cuda13 package was not produced in $outputDir"
}

Add-Type -AssemblyName System.IO.Compression.FileSystem
$zip = [System.IO.Compression.ZipFile]::OpenRead($packagePath.FullName)
try {
    $requiredEntries = [System.Collections.Generic.List[string]]::new()
    $requiredEntries.Add("loader.json")
    $requiredEntries.Add("contentFiles/any/any/loader.json")

    if (Test-Path $nativeBridgePath) {
        $requiredEntries.Add("runtimes/win-x64/native/libtorch_bridge.dll")
    }

    foreach ($entryName in $requiredEntries) {
        $entry = $zip.GetEntry($entryName)
        if ($null -eq $entry) {
            throw "Missing package entry: $entryName"
        }

        Write-Host "package-entry: $entryName ok"
    }
}
finally {
    $zip.Dispose()
}

Write-Host "package: $($packagePath.FullName)"
Write-Host "AIKernel.Cuda13 native package smoke: ok"

param(
    [string]$Configuration = "Debug",
    [int]$LocalPackageBuildNumber = 1,
    [string]$BridgePath,
    [string]$LibTorchPath,
    [string]$CudaRuntimePath,
    [string]$ToolsRepoRoot,
    [string]$LocalFeed,
    [string]$NuGetConfigPath,
    [switch]$NoBuildTools,
    [switch]$AllowLoadFailure
)

$ErrorActionPreference = "Stop"

$isWindowsHost = ($PSVersionTable.PSEdition -eq "Desktop") -or
    (Get-Variable -Name IsWindows -ValueOnly -ErrorAction SilentlyContinue)

if (-not $isWindowsHost) {
    throw "AIKernel.Cuda13.0 native library verification is currently supported only on Windows win-x64."
}

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$workspaceRoot = Resolve-Path (Join-Path $repoRoot "..")

if ([string]::IsNullOrWhiteSpace($BridgePath)) {
    $BridgePath = Join-Path $repoRoot "native\build\win-x64\Release\libtorch_bridge.dll"
}

if ([string]::IsNullOrWhiteSpace($ToolsRepoRoot)) {
    $ToolsRepoRoot = Join-Path $workspaceRoot "AIKernel.Tools"
}

if ([string]::IsNullOrWhiteSpace($LocalFeed)) {
    $LocalFeed = Join-Path $workspaceRoot "artifacts\local-nuget\v0.1.3-dev$LocalPackageBuildNumber"
}

if ([string]::IsNullOrWhiteSpace($NuGetConfigPath)) {
    $NuGetConfigPath = Join-Path $workspaceRoot "artifacts\NuGet.rev3-local.v0.1.3-dev$LocalPackageBuildNumber.config"
}

function Resolve-FirstExistingPath {
    param(
        [string]$Kind,
        [string[]]$Candidates,
        [string[]]$RequiredRelativePaths = @()
    )

    foreach ($candidate in $Candidates) {
        if ([string]::IsNullOrWhiteSpace($candidate)) {
            continue
        }

        if (Test-Path -LiteralPath $candidate) {
            $resolved = (Resolve-Path $candidate).Path
            $runtimeMarker = $RequiredRelativePaths |
                Where-Object { Test-Path -LiteralPath (Join-Path $resolved $_) } |
                Select-Object -First 1

            if ($RequiredRelativePaths.Count -gt 0 -and $null -eq $runtimeMarker) {
                Write-Host "dependency-probe: $Kind placeholder-or-incomplete; path=$resolved"
                continue
            }

            Write-Host "dependency-probe: $Kind found; path=$resolved"
            return $resolved
        }

        Write-Host "dependency-probe: $Kind missing; path=$candidate"
    }

    Write-Host "dependency-probe: $Kind unresolved"
    return $null
}

if ([string]::IsNullOrWhiteSpace($LibTorchPath)) {
    $LibTorchPath = Resolve-FirstExistingPath `
        -Kind "libtorch" `
        -RequiredRelativePaths @(
            "lib\c10.dll",
            "lib\torch_cpu.dll",
            "bin\c10.dll",
            "c10.dll"
        ) `
        -Candidates @(
            $env:AIKERNEL_LIBTORCH_PATH,
            (Join-Path $repoRoot "runtime\win-x64\libtorch"),
            (Join-Path $workspaceRoot "ref\libtorch-win-shared-with-deps-2.12.0+cu130\libtorch"),
            (Join-Path $workspaceRoot "runtime\win-x64\libtorch"),
            "D:\AIKernel\runtime\win-x64\libtorch",
            "D:\AIKernel\ref\libtorch-win-shared-with-deps-2.12.0+cu130\libtorch"
        )
}
else {
    Write-Host "dependency-probe: libtorch explicit; path=$LibTorchPath"
    $libTorchMarkers = @("lib\c10.dll", "lib\torch_cpu.dll", "bin\c10.dll", "c10.dll")
    if (-not ($libTorchMarkers | Where-Object { Test-Path -LiteralPath (Join-Path $LibTorchPath $_) } | Select-Object -First 1)) {
        Write-Warning "dependency-probe: libtorch explicit path does not contain expected runtime DLL markers."
    }
}

if ([string]::IsNullOrWhiteSpace($CudaRuntimePath)) {
    $programFilesCuda13 = if ([string]::IsNullOrWhiteSpace(${env:ProgramFiles})) {
        $null
    }
    else {
        Join-Path ${env:ProgramFiles} "NVIDIA GPU Computing Toolkit\CUDA\v13.0"
    }

    $CudaRuntimePath = Resolve-FirstExistingPath `
        -Kind "cuda-runtime" `
        -RequiredRelativePaths @(
            "bin\x64\cudart64_13.dll",
            "bin\cudart64_13.dll",
            "lib\x64\cudart.lib"
        ) `
        -Candidates @(
            $env:CUDA_PATH_V13_0,
            $env:CUDA_PATH,
            $programFilesCuda13,
            "D:\AIKernel\runtime\cuda\v13.0"
        )
}
else {
    Write-Host "dependency-probe: cuda-runtime explicit; path=$CudaRuntimePath"
    $cudaMarkers = @("bin\x64\cudart64_13.dll", "bin\cudart64_13.dll", "lib\x64\cudart.lib")
    if (-not ($cudaMarkers | Where-Object { Test-Path -LiteralPath (Join-Path $CudaRuntimePath $_) } | Select-Object -First 1)) {
        Write-Warning "dependency-probe: cuda-runtime explicit path does not contain expected CUDA 13 runtime markers."
    }
}

if (-not $AllowLoadFailure) {
    if ([string]::IsNullOrWhiteSpace($LibTorchPath)) {
        throw "LibTorch runtime path was not resolved. Set AIKERNEL_LIBTORCH_PATH or pass -LibTorchPath for strict native library verification."
    }

    if ([string]::IsNullOrWhiteSpace($CudaRuntimePath)) {
        throw "CUDA 13 runtime path was not resolved. Set CUDA_PATH_V13_0/CUDA_PATH or pass -CudaRuntimePath for strict native library verification."
    }
}

if (-not (Test-Path -LiteralPath $BridgePath)) {
    throw "Cuda13 native bridge was not found: $BridgePath"
}

if (-not (Test-Path -LiteralPath $ToolsRepoRoot)) {
    throw "AIKernel.Tools repository was not found: $ToolsRepoRoot"
}

if (-not (Test-Path -LiteralPath $LocalFeed)) {
    throw "Local package feed was not found: $LocalFeed"
}

if (-not (Test-Path -LiteralPath $NuGetConfigPath)) {
    $configScript = Join-Path $ToolsRepoRoot "scripts\new-rev3-local-nuget-config.ps1"
    if (-not (Test-Path -LiteralPath $configScript)) {
        throw "rev3 local NuGet config was not found and generator script is missing: $NuGetConfigPath"
    }

    & $configScript `
        -LocalPackageBuildNumber $LocalPackageBuildNumber `
        -WorkspaceRoot $workspaceRoot `
        -OutputPath $NuGetConfigPath
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to generate rev3 local NuGet config: $NuGetConfigPath"
    }
}

$pathEntries = [System.Collections.Generic.List[string]]::new()
foreach ($candidateRoot in @($LibTorchPath, $CudaRuntimePath)) {
    if ([string]::IsNullOrWhiteSpace($candidateRoot)) {
        continue
    }

    if (-not (Test-Path -LiteralPath $candidateRoot)) {
        throw "Runtime dependency path was not found: $candidateRoot"
    }

    $resolvedRoot = (Resolve-Path $candidateRoot).Path
    $pathEntries.Add($resolvedRoot)

    foreach ($child in @("bin", "bin\x64", "lib", "lib\x64")) {
        $childPath = Join-Path $resolvedRoot $child
        if (Test-Path -LiteralPath $childPath) {
            $pathEntries.Add((Resolve-Path $childPath).Path)
        }
    }
}

$toolsProject = Join-Path $ToolsRepoRoot "src\AIKernel.CLI\AIKernel.CLI.csproj"
$cliAssembly = Join-Path $ToolsRepoRoot "src\AIKernel.CLI\bin\$Configuration\net10.0\aik.dll"

if (-not $NoBuildTools) {
    dotnet restore $toolsProject `
        --configfile $NuGetConfigPath `
        -p:UseLocalPackageVersion=true `
        -p:LocalPackageBuildNumber=$LocalPackageBuildNumber

    if ($LASTEXITCODE -ne 0) {
        throw "AIKernel.Tools CLI restore failed."
    }

    dotnet build $toolsProject `
        -c $Configuration `
        --no-restore `
        -p:UseLocalPackageVersion=true `
        -p:LocalPackageBuildNumber=$LocalPackageBuildNumber

    if ($LASTEXITCODE -ne 0) {
        throw "AIKernel.Tools CLI build failed."
    }
}

if (-not (Test-Path -LiteralPath $cliAssembly)) {
    throw "AIKernel.Tools CLI assembly was not found: $cliAssembly"
}

$previousPath = $env:PATH
try {
    if ($pathEntries.Count -gt 0) {
        $env:PATH = (($pathEntries | Select-Object -Unique) -join ";") + ";" + $previousPath
        Write-Host "dependency-paths: $($pathEntries.Count) entries prepended"
    }
    else {
        Write-Host "dependency-paths: none supplied; using existing PATH"
    }

    $output = & dotnet $cliAssembly gpu verify-native --provider cuda13 --library $BridgePath 2>&1
    $exitCode = $LASTEXITCODE
    $output | ForEach-Object { Write-Host $_ }

    if ($exitCode -ne 0) {
        $joined = ($output | Out-String)
        if ($AllowLoadFailure -and
            $joined.IndexOf("missing-dependent-module", [StringComparison]::OrdinalIgnoreCase) -ge 0) {
            Write-Host "native-library-smoke: dependency load failure accepted by -AllowLoadFailure"
            $global:LASTEXITCODE = 0
            return
        }

        throw "Cuda13 native library verification failed with exit code $exitCode."
    }

    Write-Host "AIKernel.Cuda13 native library smoke: ok"
}
finally {
    $env:PATH = $previousPath
}

﻿$SolutionDir = Join-Path $PSScriptRoot '..\src'
$ArtifactsDir = Join-Path $PSScriptRoot '..\artifacts'
$Projects = @("NullGC.Abstractions", "NullGC.Allocators", "NullGC.Collections", "NullGC.Linq");
$Tests = @("NullGC.Allocators.Tests", "NullGC.Collections.Tests", "NullGC.Linq.Tests");
$BenchmarkArtifactsDir = Join-Path $PSScriptRoot '..\artifacts\BenchmarkArtifacts'
$BenchmarkResultPageDir = Join-Path $PSScriptRoot '..\artifacts\BenchmarkResultPage'
if (!$Env:GitVersion_NuGetVersionV2) {
    # local run
    if (!(& dotnet tool list | Out-String).Contains('gitversion.tool')) { dotnet tool restore }
    $gitver = (& dotnet gitversion -output json) | ConvertFrom-Json
    $Env:GitVersion_NuGetVersionV2 = $gitver.NuGetVersionV2;
}

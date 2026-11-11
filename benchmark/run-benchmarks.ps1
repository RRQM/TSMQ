<#
.SYNOPSIS
Runs the MQTT benchmarks with BenchmarkDotNet.

.EXAMPLE
./run-benchmarks.ps1 -Quick

.EXAMPLE
./run-benchmarks.ps1 -Filter '*BenchmarkBroker*'

.EXAMPLE
./run-benchmarks.ps1 -Quick -Filter '*ForwardMessages*'
#>
[CmdletBinding()]
param(
    [Parameter()]
    [switch] $Quick,

    [Parameter()]
    [ValidateSet('Release', 'Debug')]
    [string] $Configuration = 'Release',

    [Parameter()]
    [string[]] $Filter = @(),

    [Parameter()]
    [string[]] $BenchmarkDotNetArgs = @()
)

$ErrorActionPreference = 'Stop'

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    throw 'The .NET SDK was not found. Install it and ensure dotnet is on PATH.'
}

$projectPath = Join-Path $PSScriptRoot 'src/BenchmarkConsoleApp/BenchmarkConsoleApp.csproj'
if (-not (Test-Path -LiteralPath $projectPath -PathType Leaf)) {
    throw "Benchmark project not found: $projectPath"
}

$dotnetArgs = @(
    'run'
    '--project'
    $projectPath
    '--configuration'
    $Configuration
    '--'
)

if ($Quick) {
    $dotnetArgs += '--quick'
}

foreach ($filterPattern in $Filter) {
    $dotnetArgs += @('--filter', $filterPattern)
}

if ($BenchmarkDotNetArgs) {
    $dotnetArgs += $BenchmarkDotNetArgs
}

& dotnet @dotnetArgs
exit $LASTEXITCODE

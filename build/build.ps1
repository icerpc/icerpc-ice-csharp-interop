# Copyright (c) ZeroC, Inc.

param (
    $action="build",
    $config="debug",
    $iceVersion = "3.7.9",
    $icerpcVersion = "0.1.0",
    [switch]$examples,
    [switch]$srcdist,
    [switch]$coverage,
    [switch]$help
)

function Build($config) {
    $dotnetConfiguration = DotnetConfiguration($config)
    RunCommand "dotnet" @('build', '--configuration', $dotnetConfiguration, "/p:IceVersion=$iceVersion", "/p:IceRpcVersion=$icerpcVersion")
}

function Clean($config) {
    $dotnetConfiguration = DotnetConfiguration($config)
    RunCommand "dotnet" @('clean', '--configuration', $dotnetConfiguration)
}

function Rebuild($config, $examples, $srcdist) {
    Clean $config
    Build $config
}

function Test($config, $coverage) {
    $dotnetConfiguration = DotnetConfiguration($config)
    $arguments = @('test', '--no-build', '--configuration', $dotnetConfiguration)
    RunCommand "dotnet" $arguments
}

function RunCommand($command, $arguments) {
    Write-Host $command $arguments
    & $command $arguments
    if ($lastExitCode -ne 0) {
        exit 1
    }
}

function DotnetConfiguration($config) {
    if ($config -eq 'release') {
        'Release'
    } else {
        'Debug'
    }
}

function Get-Help() {
    Write-Host "Usage: build [command] [arguments]"
    Write-Host "Commands (defaults to build):"
    Write-Host "  build                     Build tests."
    Write-Host "  clean                     Clean tests."
    Write-Host "  rebuild                   Rebuild tests."
    Write-Host "  test                      Runs tests."
    Write-Host "Arguments:"
    Write-Host "  -config                   Build configuration: debug or release, the default is debug."
    Write-Host "  -iceVersion               Build tests using the given Ice version default is (3.7.9)."
    Write-Host "  -icerpcVersion            Build tests using the given IceRPC vresion default is (0.1.0)"
    Write-Host "  -help                     Print help and exit."
}

$configs = "debug","release"
if ( $configs -notcontains $config ) {
    Get-Help
    throw new-object system.ArgumentException "config must debug or release"
}

if ( $help ) {
    Get-Help
    exit 0
}

switch ( $action ) {
    "build" {
        Build $config
    }
    "rebuild" {
        Rebuild $config
    }
    "clean" {
        Clean $config
    }
    "test" {
       Test $config
    }
    default {
        Write-Error "Invalid action value" $action
        Get-Help
        exit 1
    }
}
exit 0

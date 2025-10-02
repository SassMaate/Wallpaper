<#
.SYNOPSIS
    Installs .NET runtimes for Sucrose projects into specified directories.

.DESCRIPTION
    Downloads and installs specified .NET runtime versions (x86, x64, ARM64) using dotnet-install.ps1.
    Can be run independently or triggered from a publish script.
    
.PARAMETER DotNetVersion
    The version of .NET to install. Default: 9.0.305

.PARAMETER BaseDir
    Base directory for installation. Default: script folder.

.PARAMETER RunFromPublish
    If true, this script was called from the publish script. Default: false.
#>

param (
    [string]$PlatformTarget,
    [switch]$RunFromPublish,
	[string]$TargetFramework,
    [string]$DotNetVersion = "9.0.305",
    [string]$InstallDir = "..\src\Sucrose\Package",
    [string]$BaseDir = (Split-Path -Parent $MyInvocation.MyCommand.Definition)
)

# ----- Set console input/output encoding -----
[Console]::InputEncoding = [System.Text.Encoding]::UTF8
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

# ----- Paths -----
$dotnetInstallScript = Join-Path $BaseDir "dotnet-install.ps1"

if (-not (Test-Path $dotnetInstallScript)) {
    throw "dotnet-install.ps1 not found in $BaseDir"
}

# ----- Install runtimes -----
$archs = @("x86", "x64", "arm64")

foreach ($arch in $archs) {
    $installDir = Join-Path $BaseDir InstallDir
    Write-Host "$(Get-Date -Format 'HH:mm:ss') - Installing .NET $DotNetVersion ($arch) into $installDir ..." -ForegroundColor Cyan

    & $dotnetInstallScript -Version $DotNetVersion -Architecture $arch -InstallDir $installDir

    if ($LASTEXITCODE -ne 0) {
        Write-Host "$(Get-Date -Format 'HH:mm:ss') - Installation failed for $arch" -ForegroundColor Red
        if (-not $RunFromPublish) { throw "Installation failed for $arch" }
    } else {
        Write-Host "$(Get-Date -Format 'HH:mm:ss') - Installation succeeded for $arch" -ForegroundColor Green
    }
}

if ($RunFromPublish) {
    Write-Host "$(Get-Date -Format 'HH:mm:ss') - Runtime installation completed as part of publish" -ForegroundColor Green
} else {
    Write-Host "$(Get-Date -Format 'HH:mm:ss') - Runtime installation completed" -ForegroundColor Green
}
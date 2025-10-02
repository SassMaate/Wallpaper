<#
.SYNOPSIS
    Production-ready solution-level publish script for Sucrose projects.

.DESCRIPTION
    Publishes multiple .NET projects sequentially.
    Automatically detects Configuration, PlatformTarget, RuntimeIdentifier, TargetFramework if not provided.
    Cleans destination folders, logs output, retries failed publishes.

.PARAMETER PlatformTarget
    Target platform (x64, x86, ARM64). Default: auto-detected or x64

.PARAMETER SelfContained
    Whether the publish should be self-contained. Default: false

.PARAMETER Configuration
    Build configuration (Release/Debug). Default: auto-detected or Release

.PARAMETER RuntimeIdentifier
    Runtime identifier for publish. Default: auto-detected based on PlatformTarget

.PARAMETER PublishDir
    Base publish folder. Default: Sucrose\Package

.PARAMETER TargetFramework
    Target framework. Default: auto-detected from first project or net9.0-windows

.PARAMETER PublishBaseDir
    Base directory of the solution. Default: script folder

.PARAMETER MaxAttempts
    Number of retry attempts per project. Default: 3

.PARAMETER RetryDelay
    Delay between retries in seconds. Default: 2
#>

param (
    [string]$PlatformTarget,
    [string]$SelfContained = "false",
    [string]$Configuration,
    [string]$RuntimeIdentifier,
    [string]$PublishDir = "..\src\Sucrose\Package",
    [string]$TargetFramework,
    [string]$PublishBaseDir = (Split-Path -Parent $MyInvocation.MyCommand.Definition),
    [int]$MaxAttempts = 3,
    [int]$RetryDelay = 2
)

# ----- Set console input/output encoding to UTF-8 to properly handle non-ASCII characters -----
[Console]::InputEncoding = [System.Text.Encoding]::UTF8
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

# ----- Auto-detect Configuration -----
if (-not $Configuration) {
    $Configuration = $env:Configuration
    if (-not $Configuration) { $Configuration = "Release" }
}

# ----- Auto-detect PlatformTarget & RuntimeIdentifier -----
if (-not $PlatformTarget) {
    $PlatformTarget = $env:Platform
    if (-not $PlatformTarget) { $PlatformTarget = "x64" }
}

if (-not $RuntimeIdentifier) {
    switch ($PlatformTarget.ToLower()) {
        "x64" { $RuntimeIdentifier = "win-x64" }
        "x86" { $RuntimeIdentifier = "win-x86" }
        "arm64" { $RuntimeIdentifier = "win-arm64" }
        default { throw "Unsupported PlatformTarget: $PlatformTarget. Cannot determine RuntimeIdentifier." }
    }
}

# ----- Projects to publish -----
$projects = @(
    "..\src\Launcher\Sucrose.Launcher\Sucrose.Launcher.csproj",
    "..\src\..\src\Live\Sucrose.Live.Aurora\Sucrose.Live.Aurora.csproj",
    "..\src\Live\Sucrose.Live.CefSharp\Sucrose.Live.CefSharp.csproj",
    "..\src\Live\Sucrose.Live.MpvPlayer\Sucrose.Live.MpvPlayer.csproj",
    "..\src\Live\Sucrose.Live.Nebula\Sucrose.Live.Nebula.csproj",
    "..\src\Live\Sucrose.Live.Vexana\Sucrose.Live.Vexana.csproj",
    "..\src\Live\Sucrose.Live.WebView\Sucrose.Live.WebView.csproj",
    "..\src\Live\Sucrose.Live.Xavier\Sucrose.Live.Xavier.csproj",
    "..\src\Localizer\Sucrose.Localizer\Sucrose.Localizer.csproj",
    "..\src\Portal\Sucrose.Portal\Sucrose.Portal.csproj",
    "..\src\Project\Sucrose.Backgroundog\Sucrose.Backgroundog.csproj",
    "..\src\Project\Sucrose.Commandog\Sucrose.Commandog.csproj",
    "..\src\Project\Sucrose.Property\Sucrose.Property.csproj",
    "..\src\Project\Sucrose.Reportdog\Sucrose.Reportdog.csproj",
    "..\src\Project\Sucrose.Undo\Sucrose.Undo.csproj",
    "..\src\Project\Sucrose.Watchdog\Sucrose.Watchdog.csproj",
    "..\src\Update\Sucrose.Update\Sucrose.Update.csproj"
)

# ----- Detect TargetFramework from first project if not set -----
if (-not $TargetFramework) {
    function Get-TargetFramework($csproj) {
		[xml]$projXml = Get-Content $csproj

		$tf = $null

        # Try TargetFramework first
		foreach ($pg in $projXml.Project.PropertyGroup) {
			if ($pg.TargetFramework) {
				$tf = $pg.TargetFramework
				break
			} elseif ($pg.TargetFrameworks) {
				$tf = $pg.TargetFrameworks
				break
			}
		}

		# Normalize: remove all extra whitespace
		$tf = -join ($tf.ToCharArray() | Where-Object { $_ -ne " " })
		$tf = $tf -replace '\s+', ''

        if ([string]::IsNullOrWhiteSpace($tf)) {
            throw "No TargetFramework or TargetFrameworks found in $csproj"
        }
		
		if ($tf -like "*;*") {
			# Split by ; if exists, trim entries, take first non-empty
			$tfArray = $tf -split ";" | ForEach-Object { $_.Trim() } | Where-Object { $_ -ne "" }
			return $tfArray[0]
		} else {
			return $tf
		}
    }

    $TargetFramework = Get-TargetFramework (Join-Path $PublishBaseDir $projects[0])
}

# ----- Helper: publish a single project -----
function Publish-Project {
    param (
        [string]$ProjectPath,
        [string]$ProjectName
    )

    $destination = Join-Path $PublishBaseDir $PublishDir
    if ($ProjectName -eq "Sucrose.Undo") { $destination = Join-Path $destination "Cache" }
    $destination = Join-Path $destination "$TargetFramework\$PlatformTarget\$ProjectName"

    if (Test-Path $destination) {
        Write-Host "$(Get-Date -Format 'HH:mm:ss') - Cleaning $destination ..." -ForegroundColor Yellow
        Remove-Item $destination -Recurse -Force
    }
    New-Item -ItemType Directory -Path $destination -Force | Out-Null

    $logFile = Join-Path $destination "Publish.log"

    Write-Host "$(Get-Date -Format 'HH:mm:ss') - Publishing $ProjectName ..." -ForegroundColor Cyan

    $attempt = 0
    do {
        $attempt++
        Write-Host "$(Get-Date -Format 'HH:mm:ss') - Attempt $attempt ..." -ForegroundColor Gray

        dotnet publish $ProjectPath `
            -c $Configuration `
            /p:PlatformTarget=$PlatformTarget `
            -r $RuntimeIdentifier `
            -f $TargetFramework `
            --self-contained $SelfContained `
            -o $destination *>&1 | Tee-Object -FilePath $logFile

        $exitCode = $LASTEXITCODE
        if ($exitCode -ne 0) {
            Write-Host "$(Get-Date -Format 'HH:mm:ss') - Publish failed for $ProjectName. Retrying in $RetryDelay seconds..." -ForegroundColor Red
            Start-Sleep -Seconds $RetryDelay
        }

    } while ($exitCode -ne 0 -and $attempt -lt $MaxAttempts)

    if ($exitCode -eq 0) {
        Write-Host "$(Get-Date -Format 'HH:mm:ss') - Publish succeeded for $ProjectName" -ForegroundColor Green
    } else {
        Write-Host "$(Get-Date -Format 'HH:mm:ss') - Publish failed for $ProjectName after $MaxAttempts attempts" -ForegroundColor Red
        throw "Publish failed for $ProjectName. See log: $logFile"
    }
}

# ----- Publish all projects sequentially -----
foreach ($proj in $projects) {
    $projName = [System.IO.Path]::GetFileNameWithoutExtension($proj)
    $fullPath = Join-Path $PublishBaseDir $proj
    Publish-Project -ProjectPath $fullPath -ProjectName $projName
}
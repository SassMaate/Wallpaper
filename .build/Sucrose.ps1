<#
.SYNOPSIS
    Production-ready solution-level publish script for Sucrose projects with optional runtime installation and compression.

.DESCRIPTION
    Publishes multiple .NET projects sequentially.
    Automatically detects Configuration, PlatformTarget, RuntimeIdentifier, TargetFramework if not provided.
    Cleans destination folders, logs output, retries failed publishes.
    Optionally installs .NET runtimes (x86, x64, ARM64) after publish.
    Optionally compresses published package using platform-specific 7zip.

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

.PARAMETER InstallRuntimeAfterPublish
    If specified, installs .NET runtimes after publish (x86, x64, ARM64). Default: true

.PARAMETER CompressSucrosePackage
    If specified, compresses the published package using 7zip. Default: true

.PARAMETER DotNetVersion
    Version of .NET to install. Default: 9.0.305
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
    [int]$RetryDelay = 2,
    [string]$InstallRuntimeAfterPublish = "true",
    [string]$CompressSucrosePackage = "true",
    [string]$DotNetVersion = "9.0.305"
)

# ----- Set PowerShell execution policy for current process -----
try {
    $currentPolicy = Get-ExecutionPolicy -Scope Process
    if ($currentPolicy -eq "Restricted" -or $currentPolicy -eq "AllSigned") {
        Write-Host "$(Get-Date -Format 'HH:mm:ss') - Current execution policy: $currentPolicy. Setting to Bypass for this process..." -ForegroundColor Yellow
        Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass -Force
        Write-Host "$(Get-Date -Format 'HH:mm:ss') - Execution policy set to Bypass for current process" -ForegroundColor Green
    } else {
        Write-Host "$(Get-Date -Format 'HH:mm:ss') - Current execution policy: $currentPolicy (OK)" -ForegroundColor Green
    }
} catch {
    Write-Host "$(Get-Date -Format 'HH:mm:ss') - Warning: Could not check/set execution policy: $($_.Exception.Message)" -ForegroundColor Yellow
}

# ----- Set console input/output encoding -----
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
        "x86" { 
            $PlatformTarget = "x86"
            $RuntimeIdentifier = "win-x86" 
        }
        "x64" { 
            $PlatformTarget = "x64"
            $RuntimeIdentifier = "win-x64" 
        }
        "arm64" { 
            $PlatformTarget = "ARM64"
            $RuntimeIdentifier = "win-arm64" 
        }
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

        foreach ($pg in $projXml.Project.PropertyGroup) {
            if ($pg.TargetFramework) {
                $tf = $pg.TargetFramework
                break
            } elseif ($pg.TargetFrameworks) {
                $tf = $pg.TargetFrameworks
                break
            }
        }

        $tf = -join ($tf.ToCharArray() | Where-Object { $_ -ne " " })
        $tf = $tf -replace '\s+', ''

        if ([string]::IsNullOrWhiteSpace($tf)) {
            throw "No TargetFramework or TargetFrameworks found in $csproj"
        }

        if ($tf -like "*;*") {
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

# ----- Optional: Install .NET runtime -----
if ($InstallRuntimeAfterPublish -eq "true") {
    $dotnetInstallScript = Join-Path $PublishBaseDir "dotnet-install.ps1"
    if (-not (Test-Path $dotnetInstallScript)) {
        throw "dotnet-install.ps1 not found in $PublishBaseDir"
    }

    # Fixed target directory: inside publish folder net9.0-windows/x64
    $runtimeInstallDir = Join-Path $PublishBaseDir $PublishDir
    $runtimeInstallDir = Join-Path $runtimeInstallDir "$TargetFramework\$PlatformTarget\Sucrose.Runtime"

    if (Test-Path $runtimeInstallDir) {
        Write-Host "$(Get-Date -Format 'HH:mm:ss') - Cleaning $runtimeInstallDir ..." -ForegroundColor Yellow
        Remove-Item $runtimeInstallDir -Recurse -Force
    }
    New-Item -ItemType Directory -Path $runtimeInstallDir -Force | Out-Null

    Write-Host "$(Get-Date -Format 'HH:mm:ss') - Installing .NET $DotNetVersion into $runtimeInstallDir ..." -ForegroundColor Cyan

    # Install .NET (x64, x86, arm64)
    & $dotnetInstallScript -Version $DotNetVersion -NoPath -Architecture $PlatformTarget -InstallDir $runtimeInstallDir

    if ($LASTEXITCODE -ne 0) {
        Write-Host "$(Get-Date -Format 'HH:mm:ss') - Installation failed" -ForegroundColor Red
        throw "Installation failed"
    } else {
        Write-Host "$(Get-Date -Format 'HH:mm:ss') - Installation succeeded" -ForegroundColor Green
    }

    Write-Host "$(Get-Date -Format 'HH:mm:ss') - Runtime installation completed" -ForegroundColor Green

	# ----- Clean unnecessary runtime files -----
	$filesToRemove = @(
		"dotnet.exe",
		"LICENSE.txt",
		"ThirdPartyNotices.txt"
	)

	$dirsToRemove = @(
		"templates",
		"sdk-manifests",
		"sdk",
		"packs",
		"shared\Microsoft.AspNetCore.App"
	)

	foreach ($file in $filesToRemove) {
		$path = Join-Path $runtimeInstallDir $file
		if (Test-Path $path) {
			Write-Host "$(Get-Date -Format 'HH:mm:ss') - Removing file $path ..." -ForegroundColor Yellow
			Remove-Item $path -Force
		}
	}

	foreach ($dir in $dirsToRemove) {
		$path = Join-Path $runtimeInstallDir $dir
		if (Test-Path $path) {
			Write-Host "$(Get-Date -Format 'HH:mm:ss') - Removing directory $path ..." -ForegroundColor Yellow
			Remove-Item $path -Recurse -Force
		}
	}

	Write-Host "$(Get-Date -Format 'HH:mm:ss') - Unnecessary files and folders removed" -ForegroundColor Green
}

# ----- Helper: compress published package -----
function Compress-SucrosePackage {
    param (
        [string]$BasePath = "$PublishBaseDir\$PublishDir",
        [string]$OutputPath = "$PublishBaseDir\$PublishDir\Compressed",
        [string]$TargetFramework = $TargetFramework
    )

    Write-Host "$(Get-Date -Format 'HH:mm:ss') - Starting package compression process ..." -ForegroundColor Cyan

    # ----- Detect OS architecture -----
    #$arch = switch ($PlatformTarget.ToLower()) {
    #    "x86"   { "x86" }
    #    "x64"   { "x64" }
    #    "arm64" { "ARM64" }
    #    default { throw "Unsupported PlatformTarget: $PlatformTarget" }
    #}
    $arch = if ([Environment]::Is64BitOperatingSystem) {
        switch ([Environment]::GetEnvironmentVariable("PROCESSOR_ARCHITECTURE").ToLower()) {
            "x86"   { "x86" }
            "amd64" { "x64" }
            "arm64" { "ARM64" }
            default { throw "Unsupported architecture" }
        }
    } else {
        "x86"
    }

    Write-Host "$(Get-Date -Format 'HH:mm:ss') - Detected architecture for compression: $arch" -ForegroundColor Cyan

    # ----- Determine 7zip executable -----
    $sevenZipExe = Join-Path $PublishBaseDir "..\src\Bundle\Sucrose.Bundle\SevenZip\7z-$arch\7z.exe"
    Write-Host "$(Get-Date -Format 'HH:mm:ss') - Looking for 7zip executable: $sevenZipExe" -ForegroundColor Gray
    if (-not (Test-Path $sevenZipExe)) {
        throw "7z executable not found: $sevenZipExe"
    }

    # ----- Prepare output directory -----
    $zipDir = Join-Path $OutputPath $TargetFramework
    if (Test-Path $zipDir) {
        Write-Host "$(Get-Date -Format 'HH:mm:ss') - Cleaning existing output directory: $zipDir" -ForegroundColor Yellow
        Remove-Item $zipDir -Recurse -Force
    }
    New-Item -ItemType Directory -Path $zipDir -Force | Out-Null

    $zipFile = Join-Path $zipDir "Sucrose-$PlatformTarget.7z"
    Write-Host "$(Get-Date -Format 'HH:mm:ss') - Target archive file: $zipFile" -ForegroundColor Gray

    $BasePath = Join-Path $BasePath "$TargetFramework\$PlatformTarget"
    Write-Host "$(Get-Date -Format 'HH:mm:ss') - Source path for compression: $BasePath" -ForegroundColor Gray

    # ----- Build command -----
    $excludeFolders = @("Sucrose.Bundle","Sucrose.Localizer")
    Write-Host "$(Get-Date -Format 'HH:mm:ss') - Excluding folders: $($excludeFolders -join ', ')" -ForegroundColor Gray
    $excludeArgs = $excludeFolders | ForEach-Object { "-x!$BasePath\$_" }

    $arguments = @("a", "-t7z", "-m0=lzma2", "-mx=9", "-mfb=64", "-ms=64m", "`"$zipFile`"", "`"$BasePath\*`"") + $excludeArgs

    Write-Host "$(Get-Date -Format 'HH:mm:ss') - Starting compression: $BasePath -> $zipFile" -ForegroundColor Cyan

    # ----- Execute 7zip -----
    & $sevenZipExe @arguments
    if ($LASTEXITCODE -ne 0) {
        Write-Host "$(Get-Date -Format 'HH:mm:ss') - Compression failed with exit code $LASTEXITCODE" -ForegroundColor Red
        throw "Compression failed"
    } else {
        Write-Host "$(Get-Date -Format 'HH:mm:ss') - Compression succeeded: $zipFile" -ForegroundColor Green
    }
}

# ----- Optional: Compress published package -----
if ($CompressSucrosePackage -eq "true") {
    Compress-SucrosePackage
}
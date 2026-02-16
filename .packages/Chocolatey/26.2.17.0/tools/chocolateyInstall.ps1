$ErrorActionPreference = 'Stop'

Get-Process 'Sucrose*' | % {
	Write-Output ('Closing: {0}' -f $_.ProcessName)
	Stop-Process -InputObject $_ -Force
}

$packageArgs = @{
	packageName    = 'Sucrose Wallpaper Engine'
	checksumType   = 'sha256'
	fileType       = 'exe'
	silentArgs     = '/s'
	validExitCodes = @(0)
}

if ($env:PROCESSOR_ARCHITECTURE -eq 'ARM64') {
	$packageArgs['checksum'] = 'B11D757175A36C6429993C4CD490E2F8D6F9E11A2EDD2CBBFA8ED36A8870C32C'
	$packageArgs['url'] = 'https://github.com/Taiizor/Sucrose/releases/download/v26.2.17.0/Sucrose_Bundle_.NET_Framework_4.8_ARM64_26.2.17.0.exe'
} else {
	if ([Environment]::Is64BitOperatingSystem) {
		if ([System.Environment]::Is64BitProcess) {
			$packageArgs['checksum'] = '69E376305E2C216EBEAB2B514958DDB291F236499B59F042AFAA14495F53E6BD'
			$packageArgs['url'] = 'https://github.com/Taiizor/Sucrose/releases/download/v26.2.17.0/Sucrose_Bundle_.NET_Framework_4.8_x64_26.2.17.0.exe'
		} else {
			$packageArgs['checksum'] = 'C9971F4AD8E2B4EA62626B8CE8A7DE5614DEF14EA78AEF863F58E855991AA290'
			$packageArgs['url'] = 'https://github.com/Taiizor/Sucrose/releases/download/v26.2.17.0/Sucrose_Bundle_.NET_Framework_4.8_x86_26.2.17.0.exe'
		}
	} else {
		$packageArgs['checksum'] = 'C9971F4AD8E2B4EA62626B8CE8A7DE5614DEF14EA78AEF863F58E855991AA290'
		$packageArgs['url'] = 'https://github.com/Taiizor/Sucrose/releases/download/v26.2.17.0/Sucrose_Bundle_.NET_Framework_4.8_x86_26.2.17.0.exe'
	}
}

Install-ChocolateyPackage @packageArgs
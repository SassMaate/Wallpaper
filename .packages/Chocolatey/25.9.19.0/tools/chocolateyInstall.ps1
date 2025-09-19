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
	$packageArgs['checksum'] = '61E73788EAE06DBE18AF5C620FE945EDD3A39D633E8AC0DAEDCB9ED0DFD90140'
	$packageArgs['url'] = 'https://github.com/Taiizor/Sucrose/releases/download/v25.9.19.0/Sucrose_Bundle_.NET_Framework_4.8_ARM64_25.9.19.0.exe'
} else {
	if ([Environment]::Is64BitOperatingSystem) {
		if ([System.Environment]::Is64BitProcess) {
			$packageArgs['checksum'] = 'CA98154B604659D6813B38BA58A577F5E7BD4F62C292BFF18DEF4C9864247E52'
			$packageArgs['url'] = 'https://github.com/Taiizor/Sucrose/releases/download/v25.9.19.0/Sucrose_Bundle_.NET_Framework_4.8_x64_25.9.19.0.exe'
		} else {
			$packageArgs['checksum'] = '42054C0C725E6A994CAD4014CBEF8D7B7E530946E9FF9BA24138608C282E83E3'
			$packageArgs['url'] = 'https://github.com/Taiizor/Sucrose/releases/download/v25.9.19.0/Sucrose_Bundle_.NET_Framework_4.8_x86_25.9.19.0.exe'
		}
	} else {
		$packageArgs['checksum'] = '42054C0C725E6A994CAD4014CBEF8D7B7E530946E9FF9BA24138608C282E83E3'
		$packageArgs['url'] = 'https://github.com/Taiizor/Sucrose/releases/download/v25.9.19.0/Sucrose_Bundle_.NET_Framework_4.8_x86_25.9.19.0.exe'
	}
}

Install-ChocolateyPackage @packageArgs
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
	$packageArgs['checksum'] = '9AB04341F5AFA7AD9D6B9D5E655C2AF19515A521D9FEFBE395B165839CF3F34D'
	$packageArgs['url'] = 'https://github.com/Taiizor/Sucrose/releases/download/v25.11.20.0/Sucrose_Bundle_.NET_Framework_4.8_ARM64_25.11.20.0.exe'
} else {
	if ([Environment]::Is64BitOperatingSystem) {
		if ([System.Environment]::Is64BitProcess) {
			$packageArgs['checksum'] = 'BD8DB3E8568F02860F4373E1C519A2F7F055DD7DAA80F47D55BA6AA919D2FDA4'
			$packageArgs['url'] = 'https://github.com/Taiizor/Sucrose/releases/download/v25.11.20.0/Sucrose_Bundle_.NET_Framework_4.8_x64_25.11.20.0.exe'
		} else {
			$packageArgs['checksum'] = '9BEF5E76F1E620D292D3151333341BF712DE8FE502F1ED1A4532D1A60B3E959E'
			$packageArgs['url'] = 'https://github.com/Taiizor/Sucrose/releases/download/v25.11.20.0/Sucrose_Bundle_.NET_Framework_4.8_x86_25.11.20.0.exe'
		}
	} else {
		$packageArgs['checksum'] = '9BEF5E76F1E620D292D3151333341BF712DE8FE502F1ED1A4532D1A60B3E959E'
		$packageArgs['url'] = 'https://github.com/Taiizor/Sucrose/releases/download/v25.11.20.0/Sucrose_Bundle_.NET_Framework_4.8_x86_25.11.20.0.exe'
	}
}

Install-ChocolateyPackage @packageArgs
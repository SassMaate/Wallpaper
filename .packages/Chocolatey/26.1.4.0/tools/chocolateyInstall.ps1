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
	$packageArgs['checksum'] = '57BC9D8E88D6EA910253AB4922EA634895B4A8CE546C3EA366298AAD632B9547'
	$packageArgs['url'] = 'https://github.com/Taiizor/Sucrose/releases/download/v26.1.4.0/Sucrose_Bundle_.NET_Framework_4.8_ARM64_26.1.4.0.exe'
} else {
	if ([Environment]::Is64BitOperatingSystem) {
		if ([System.Environment]::Is64BitProcess) {
			$packageArgs['checksum'] = '82FB205CE72EBCB7D9C1758C8844A2CA7F148D85B9395F27D79E47C8C1C0E02D'
			$packageArgs['url'] = 'https://github.com/Taiizor/Sucrose/releases/download/v26.1.4.0/Sucrose_Bundle_.NET_Framework_4.8_x64_26.1.4.0.exe'
		} else {
			$packageArgs['checksum'] = 'DB0C7E82F06BFF1D4614204A7F87670610CD70D98061CAAA920EF6346147493D'
			$packageArgs['url'] = 'https://github.com/Taiizor/Sucrose/releases/download/v26.1.4.0/Sucrose_Bundle_.NET_Framework_4.8_x86_26.1.4.0.exe'
		}
	} else {
		$packageArgs['checksum'] = 'DB0C7E82F06BFF1D4614204A7F87670610CD70D98061CAAA920EF6346147493D'
		$packageArgs['url'] = 'https://github.com/Taiizor/Sucrose/releases/download/v26.1.4.0/Sucrose_Bundle_.NET_Framework_4.8_x86_26.1.4.0.exe'
	}
}

Install-ChocolateyPackage @packageArgs
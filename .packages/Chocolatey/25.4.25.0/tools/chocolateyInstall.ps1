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
	$packageArgs['checksum'] = 'A1E350DE5EBB6D2740BC71FDB5AAA1522183D15B6978399FA66867D1714A9686'
	$packageArgs['url'] = 'https://github.com/Taiizor/Sucrose/releases/download/v25.4.25.0/Sucrose_Bundle_.NET_Framework_4.8_ARM64_25.4.25.0.exe'
} else {
	if ([Environment]::Is64BitOperatingSystem) {
		if ([System.Environment]::Is64BitProcess) {
			$packageArgs['checksum'] = '5D1339D67E4442D5B1807A3DD620BFCA407FFE7A25A7678AB47AD39B63701D23'
			$packageArgs['url'] = 'https://github.com/Taiizor/Sucrose/releases/download/v25.4.25.0/Sucrose_Bundle_.NET_Framework_4.8_x64_25.4.25.0.exe'
		} else {
			$packageArgs['checksum'] = '331091682F75AB20EDC5958DD0DBB553A92CCDE24F1DC58A3D58652B91F3E8AB'
			$packageArgs['url'] = 'https://github.com/Taiizor/Sucrose/releases/download/v25.4.25.0/Sucrose_Bundle_.NET_Framework_4.8_x86_25.4.25.0.exe'
		}
	} else {
		$packageArgs['checksum'] = '331091682F75AB20EDC5958DD0DBB553A92CCDE24F1DC58A3D58652B91F3E8AB'
		$packageArgs['url'] = 'https://github.com/Taiizor/Sucrose/releases/download/v25.4.25.0/Sucrose_Bundle_.NET_Framework_4.8_x86_25.4.25.0.exe'
	}
}

Install-ChocolateyPackage @packageArgs
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
	$packageArgs['checksum'] = 'BDA6C1CCFFC4D117730BB21BD7A2941946938E7BB2425CABA8A5E4FF311FDA74'
	$packageArgs['url'] = 'https://github.com/Taiizor/Sucrose/releases/download/v26.6.4.0/Sucrose_Bundle_.NET_Framework_4.8_ARM64_26.6.4.0.exe'
} else {
	if ([Environment]::Is64BitOperatingSystem) {
		if ([System.Environment]::Is64BitProcess) {
			$packageArgs['checksum'] = 'F4F3B81C0CABAD45DEC6728E8B9000A36287875506EE12AF5F4BC9FD5487CCCA'
			$packageArgs['url'] = 'https://github.com/Taiizor/Sucrose/releases/download/v26.6.4.0/Sucrose_Bundle_.NET_Framework_4.8_x64_26.6.4.0.exe'
		} else {
			$packageArgs['checksum'] = '017F4FB1AC37B1A0D85BED2E3A78B4F94DE8A133802BAE2A25DC9C64CDEA1E63'
			$packageArgs['url'] = 'https://github.com/Taiizor/Sucrose/releases/download/v26.6.4.0/Sucrose_Bundle_.NET_Framework_4.8_x86_26.6.4.0.exe'
		}
	} else {
		$packageArgs['checksum'] = '017F4FB1AC37B1A0D85BED2E3A78B4F94DE8A133802BAE2A25DC9C64CDEA1E63'
		$packageArgs['url'] = 'https://github.com/Taiizor/Sucrose/releases/download/v26.6.4.0/Sucrose_Bundle_.NET_Framework_4.8_x86_26.6.4.0.exe'
	}
}

Install-ChocolateyPackage @packageArgs
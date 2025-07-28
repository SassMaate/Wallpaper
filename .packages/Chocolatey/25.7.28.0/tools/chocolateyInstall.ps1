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
	$packageArgs['checksum'] = '906803D356B9514029AA154CA9E5757A1F6D59BC11CE02BB8594FC683C3F858D'
	$packageArgs['url'] = 'https://github.com/Taiizor/Sucrose/releases/download/v25.7.28.0/Sucrose_Bundle_.NET_Framework_4.8_ARM64_25.7.28.0.exe'
} else {
	if ([Environment]::Is64BitOperatingSystem) {
		if ([System.Environment]::Is64BitProcess) {
			$packageArgs['checksum'] = '094B64348D3BDFACAB71CF9B8489E31B88E9210991D4273B8BE45D55D6E1634D'
			$packageArgs['url'] = 'https://github.com/Taiizor/Sucrose/releases/download/v25.7.28.0/Sucrose_Bundle_.NET_Framework_4.8_x64_25.7.28.0.exe'
		} else {
			$packageArgs['checksum'] = '607FBA76771CAC8A55838204C40DE492C91502955C74BC1B22BEA8D8A086F51F'
			$packageArgs['url'] = 'https://github.com/Taiizor/Sucrose/releases/download/v25.7.28.0/Sucrose_Bundle_.NET_Framework_4.8_x86_25.7.28.0.exe'
		}
	} else {
		$packageArgs['checksum'] = '607FBA76771CAC8A55838204C40DE492C91502955C74BC1B22BEA8D8A086F51F'
		$packageArgs['url'] = 'https://github.com/Taiizor/Sucrose/releases/download/v25.7.28.0/Sucrose_Bundle_.NET_Framework_4.8_x86_25.7.28.0.exe'
	}
}

Install-ChocolateyPackage @packageArgs
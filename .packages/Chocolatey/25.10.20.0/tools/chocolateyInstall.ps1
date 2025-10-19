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
	$packageArgs['checksum'] = '161BFDE40DB784EA46129C211C12CBB490817A9DFEFC23BAE469367FBDA5F5D4'
	$packageArgs['url'] = 'https://github.com/Taiizor/Sucrose/releases/download/v25.10.20.0/Sucrose_Bundle_.NET_Framework_4.8_ARM64_25.10.20.0.exe'
} else {
	if ([Environment]::Is64BitOperatingSystem) {
		if ([System.Environment]::Is64BitProcess) {
			$packageArgs['checksum'] = '5A3739FB217B02FEDD43DD39EE783F382B64AD57A842264BD42038A586C296B7'
			$packageArgs['url'] = 'https://github.com/Taiizor/Sucrose/releases/download/v25.10.20.0/Sucrose_Bundle_.NET_Framework_4.8_x64_25.10.20.0.exe'
		} else {
			$packageArgs['checksum'] = '563D43D87084A42ECAD1E97B0C50F6E90D536EC3DF3F6D271D501295373923F7'
			$packageArgs['url'] = 'https://github.com/Taiizor/Sucrose/releases/download/v25.10.20.0/Sucrose_Bundle_.NET_Framework_4.8_x86_25.10.20.0.exe'
		}
	} else {
		$packageArgs['checksum'] = '563D43D87084A42ECAD1E97B0C50F6E90D536EC3DF3F6D271D501295373923F7'
		$packageArgs['url'] = 'https://github.com/Taiizor/Sucrose/releases/download/v25.10.20.0/Sucrose_Bundle_.NET_Framework_4.8_x86_25.10.20.0.exe'
	}
}

Install-ChocolateyPackage @packageArgs
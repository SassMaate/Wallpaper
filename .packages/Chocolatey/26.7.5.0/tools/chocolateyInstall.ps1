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
	$packageArgs['checksum'] = '79EA9B36C018E906B1133774665B217C302CE3BF58E1308973444497575E4F0F'
	$packageArgs['url'] = 'https://github.com/Taiizor/Sucrose/releases/download/v26.7.5.0/Sucrose_Bundle_.NET_Framework_4.8_ARM64_26.7.5.0.exe'
} else {
	if ([Environment]::Is64BitOperatingSystem) {
		if ([System.Environment]::Is64BitProcess) {
			$packageArgs['checksum'] = '3EE62580DED7840E68E6314B1FC47433122C913D6F0C047DE7C40CAD7DFD048C'
			$packageArgs['url'] = 'https://github.com/Taiizor/Sucrose/releases/download/v26.7.5.0/Sucrose_Bundle_.NET_Framework_4.8_x64_26.7.5.0.exe'
		} else {
			$packageArgs['checksum'] = 'F80D173345F9E110D318D30186E388FFA09BEB0F2EDFDD49FF3D389316CE3DEC'
			$packageArgs['url'] = 'https://github.com/Taiizor/Sucrose/releases/download/v26.7.5.0/Sucrose_Bundle_.NET_Framework_4.8_x86_26.7.5.0.exe'
		}
	} else {
		$packageArgs['checksum'] = 'F80D173345F9E110D318D30186E388FFA09BEB0F2EDFDD49FF3D389316CE3DEC'
		$packageArgs['url'] = 'https://github.com/Taiizor/Sucrose/releases/download/v26.7.5.0/Sucrose_Bundle_.NET_Framework_4.8_x86_26.7.5.0.exe'
	}
}

Install-ChocolateyPackage @packageArgs
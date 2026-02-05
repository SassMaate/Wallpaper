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
	$packageArgs['checksum'] = '676D1CE903F77BB1B444D103EBC813477314832CCF36B5EC2233D6B71494ECB5'
	$packageArgs['url'] = 'https://github.com/Taiizor/Sucrose/releases/download/v26.2.5.0/Sucrose_Bundle_.NET_Framework_4.8_ARM64_26.2.5.0.exe'
} else {
	if ([Environment]::Is64BitOperatingSystem) {
		if ([System.Environment]::Is64BitProcess) {
			$packageArgs['checksum'] = '626ABD664210E3B61ECA98FB23C12A2C440CA2DAF9373755CD0FF5482D7961FC'
			$packageArgs['url'] = 'https://github.com/Taiizor/Sucrose/releases/download/v26.2.5.0/Sucrose_Bundle_.NET_Framework_4.8_x64_26.2.5.0.exe'
		} else {
			$packageArgs['checksum'] = '1494FEDDABDC439E63CAFC807BB667D36F89369874C169A3E33B2DC8DFE69DA9'
			$packageArgs['url'] = 'https://github.com/Taiizor/Sucrose/releases/download/v26.2.5.0/Sucrose_Bundle_.NET_Framework_4.8_x86_26.2.5.0.exe'
		}
	} else {
		$packageArgs['checksum'] = '1494FEDDABDC439E63CAFC807BB667D36F89369874C169A3E33B2DC8DFE69DA9'
		$packageArgs['url'] = 'https://github.com/Taiizor/Sucrose/releases/download/v26.2.5.0/Sucrose_Bundle_.NET_Framework_4.8_x86_26.2.5.0.exe'
	}
}

Install-ChocolateyPackage @packageArgs
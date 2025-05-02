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
	$packageArgs['checksum'] = 'EB5E7D9C1545804298F6FFA10F5ED5657C7CD10FF21EAA8395F39A85B5B82033'
	$packageArgs['url'] = 'https://github.com/Taiizor/Sucrose/releases/download/v25.5.2.0/Sucrose_Bundle_.NET_Framework_4.8_ARM64_25.5.2.0.exe'
} else {
	if ([Environment]::Is64BitOperatingSystem) {
		if ([System.Environment]::Is64BitProcess) {
			$packageArgs['checksum'] = '7095FEEBFFD70BE66866A1586F4AA7F725D056E67530D82B8FC68798C548576B'
			$packageArgs['url'] = 'https://github.com/Taiizor/Sucrose/releases/download/v25.5.2.0/Sucrose_Bundle_.NET_Framework_4.8_x64_25.5.2.0.exe'
		} else {
			$packageArgs['checksum'] = '098FA672CC1DFA204488BC1E2B16D59DBEB7A935A8538B4238945AA7DD2F9F4A'
			$packageArgs['url'] = 'https://github.com/Taiizor/Sucrose/releases/download/v25.5.2.0/Sucrose_Bundle_.NET_Framework_4.8_x86_25.5.2.0.exe'
		}
	} else {
		$packageArgs['checksum'] = '098FA672CC1DFA204488BC1E2B16D59DBEB7A935A8538B4238945AA7DD2F9F4A'
		$packageArgs['url'] = 'https://github.com/Taiizor/Sucrose/releases/download/v25.5.2.0/Sucrose_Bundle_.NET_Framework_4.8_x86_25.5.2.0.exe'
	}
}

Install-ChocolateyPackage @packageArgs
dotnet publish /p:PublishProfile=FolderProfile.pubxml

if ($LASTEXITCODE -eq 0) {
	$user = $env:USERNAME
	$exePath = Join-Path (Split-Path -Parent $MyInvocation.MyCommand.Path) "bin\Release\DesktopSwitcher.exe"
	Write-Host "Creating scheduled task at \$user\StartDesktopSwitcherOnLogin"
	schtasks /Create /TN "\$user\StartDesktopSwitcherOnLogin" /TR "$exePath" /SC ONLOGON /RL LIMITED /RU $user /F /IT
}

if ($LASTEXITCODE -eq 0) {
	Write-Host "Created scheduled task. Press Enter to continue..."
} else {
	Write-Host "Error. Press Enter to continue..."
}

$null = Read-Host
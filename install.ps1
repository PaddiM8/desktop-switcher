dotnet publish /p:PublishProfile=FolderProfile.pubxml

if ($LASTEXITCODE -ne 0) {
	Write-Host "Error. Press Enter to continue..."
	$null = Read-Host
	exit
}

$workingDirectory = Join-Path (Split-Path -Parent $MyInvocation.MyCommand.Path) "bin\Release\publish"
$exePath = Join-Path ($workingDirectory) "DesktopSwitcher.exe"
$shortUsername = $env:USERNAME
$longUsername = $UserName = [System.Security.Principal.WindowsIdentity]::GetCurrent().Name
$dateString = (Get-Date).ToString("o")
$sid = [System.Security.Principal.WindowsIdentity]::GetCurrent().User.Value
$taskDefinition = @"
<?xml version="1.0" encoding="UTF-16"?>
<Task version="1.2" xmlns="http://schemas.microsoft.com/windows/2004/02/mit/task">
  <RegistrationInfo>
    <Date>$dateString</Date>
    <Author>$longUsername</Author>
  </RegistrationInfo>
  <Triggers>
    <LogonTrigger>
      <Enabled>true</Enabled>
      <UserId>$longUsername</UserId>
    </LogonTrigger>
  </Triggers>
  <Principals>
    <Principal id="Author">
      <UserId>$sid</UserId>
      <LogonType>InteractiveToken</LogonType>
      <RunLevel>HighestAvailable</RunLevel>
    </Principal>
  </Principals>
  <Settings>
    <MultipleInstancesPolicy>IgnoreNew</MultipleInstancesPolicy>
    <DisallowStartIfOnBatteries>false</DisallowStartIfOnBatteries>
    <StopIfGoingOnBatteries>false</StopIfGoingOnBatteries>
    <AllowHardTerminate>true</AllowHardTerminate>
    <StartWhenAvailable>false</StartWhenAvailable>
    <RunOnlyIfNetworkAvailable>false</RunOnlyIfNetworkAvailable>
    <IdleSettings>
      <StopOnIdleEnd>true</StopOnIdleEnd>
      <RestartOnIdle>false</RestartOnIdle>
    </IdleSettings>
    <AllowStartOnDemand>true</AllowStartOnDemand>
    <Enabled>true</Enabled>
    <Hidden>false</Hidden>
    <RunOnlyIfIdle>false</RunOnlyIfIdle>
    <WakeToRun>false</WakeToRun>
    <ExecutionTimeLimit>PT0S</ExecutionTimeLimit>
    <Priority>7</Priority>
  </Settings>
  <Actions Context="Author">
    <Exec>
      <Command>$exePath</Command>
      <WorkingDirectory>$workingDirectory</WorkingDirectory>
    </Exec>
  </Actions>
</Task>
"@

$taskPath = "\$shortUsername\"
Register-ScheduledTask -TaskName "RunDesktopSwitcher" -TaskPath $taskPath -Xml $taskDefinition

Write-Host "Change project version"
Write-Host "----------------------"

$scriptpath = $MyInvocation.MyCommand.Path
$dir = Split-Path $scriptpath

& ((Split-Path $MyInvocation.InvocationName) + "\AdjustVersionsHelperClass.ps1")

$currentProjectVersion = [AutomotiveLighting.Tools.VersionHelper]::GetCurrentVersion($dir)
Write-host "Current project version is $currentProjectVersion" 

$newProjectVersion = Read-Host "Enter new version "
[AutomotiveLighting.Tools.VersionHelper]::SetNewVersion($dir, $newProjectVersion)
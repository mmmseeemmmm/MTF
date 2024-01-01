$scriptpath = $MyInvocation.MyCommand.Path
$dir = Split-Path $scriptpath

Write-host "My directory is $dir"

& ((Split-Path $MyInvocation.InvocationName) + "\AdjustVersionsHelperClass.ps1")

[AutomotiveLighting.Tools.VersionHelper]::Adjust($dir)

# Ver: 1.1
# MTF publish release
#
param(
	[string]$mtfSourcePath = "\\czjih1rad1.mmemea.marelliad.net\eap_prj\13_EET\13-Builds\MTF\"
)

begin
{
 # load the required dll's
    [void][System.Reflection.Assembly]::LoadWithPartialName("Microsoft.TeamFoundation.Client")
    [void][System.Reflection.Assembly]::LoadWithPartialName("Microsoft.TeamFoundation.VersionControl.Client")
}

process
{
	if (Test-Path -Path $mtfSourcePath)
	{
		$path = "$mtfSourcePath\Server\MTFCommon.dll"
    
        if(Test-Path -Path $path)
        {
		    $ver = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($path).FileVersion

		    # Creates Release branch
		    $destinationBranch = '$/Main Framework Testing/Branches/' + $ver
		    $server = New-Object Microsoft.TeamFoundation.Client.TeamFoundationServer("http://czjih1ndbl5:8080/tfs/DefaultCollection")
		    $vcServer = $server.GetService([Microsoft.TeamFoundation.VersionControl.Client.VersionControlServer]); 
            $title = "New branch from script ver. " + $ver
		    $changesetId = $vcServer.CreateBranch('$/Main Framework Testing/Trunk', $destinationBranch, [Microsoft.TeamFoundation.VersionControl.Client.VersionSpec]::Latest, $null, $title, $null, $null, $null)
		    
		    echo 'Created branch ' + $changesetId
        }
        else
        {
            Write-Output "MTFCommon.dll not existing '$path'"
        }
	}
	else
	{
		Write-Output "MTF not existing '$mtfSourcePath'"
	}
}
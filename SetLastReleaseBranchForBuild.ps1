# 
#	Get last release branch and set TFS variable.
#
#	Script needs set execution policy.
#
#	Set-ExecutionPolicy unrestricted
#
param(
 
)

begin
{
	# load the required dll's
    [void][System.Reflection.Assembly]::LoadWithPartialName("Microsoft.TeamFoundation.Client")
    [void][System.Reflection.Assembly]::LoadWithPartialName("Microsoft.TeamFoundation.VersionControl.Client")
}

process
{
	$destinationBranch = '$/LED_Module_Check_V2/Branches/Release_' + $date
	$server = New-Object Microsoft.TeamFoundation.Client.TeamFoundationServer("http://czjih1ndbl5:8080/tfs/DefaultCollection")
	$vcServer = $server.GetService([Microsoft.TeamFoundation.VersionControl.Client.VersionControlServer])
	$identification = New-Object Microsoft.TeamFoundation.VersionControl.Client.ItemSpec("$/Main Framework Testing/Branches/", [Microsoft.TeamFoundation.VersionControl.Client.RecursionType]::Full)
    $branchItems = $vcServer.GetItems($identification, [Microsoft.TeamFoundation.VersionControl.Client.VersionSpec]::Latest, [Microsoft.TeamFoundation.VersionControl.Client.DeletedState]::NonDeleted, [Microsoft.TeamFoundation.VersionControl.Client.ItemType]::Folder, [Microsoft.TeamFoundation.VersionControl.Client.GetItemsOptions]::IncludeBranchInfo)
    $lastReleaseBranch = [Linq.Enumerable]::LastOrDefault([Linq.Enumerable]::OrderBy([Linq.Enumerable]::Where($branchItems.Items, [Func[Microsoft.TeamFoundation.VersionControl.Client.Item, bool]] { $Args[0].IsBranch -eq 'True' }), [Func[Microsoft.TeamFoundation.VersionControl.Client.Item, bool]] { $Args[0].CheckinDate }))

    if($lastReleaseBranch)
    {
        Write-Host $lastReleaseBranch.ServerItem
        $sourceSolution = $lastReleaseBranch.ServerItem
		$branchName = $sourceSolution.Split("//") | Select-Object -Last 1
        write-output "##vso[task.setvariable variable=MTFSolution;]$sourceSolution"
		write-output "##vso[task.setvariable variable=BranchName;]$branchName"
    }
    else
    {
        Write-Host "Not existing last release branch"
    }
}
Get-Variable -Exclude PWD,*Preference | Remove-Variable -EA 0
# VERSION BUILD OPTIONS:

$IncreaseMajor = $false
$IncreaseMinor = $false
$IncreaseBuild = $false
$IncreaseRevision = $true

#$OverrideVersion = '0.1.1.1'

# BUILD SCRIPT BELOW:

$localPath=(Get-Location).toString() + "\"

if (Test-Path variable:OverrideVersion) {
	$NewVersion = $OverrideVersion
} else {
	msbuild Build.xml /t:BuildForVersionCheck

	.\Release\NoBinaries\Youtube-DL-App.exe --version | Out-Null

	$VersionFile = Get-Content ($localPath + "version.txt")

	$MajorVersion = $VersionFile[0] -as [int]
	$MinorVersion = $VersionFile[1] -as [int]
	$BuildVersion = $VersionFile[2] -as [int]
	$RevisionVersion = $VersionFile[3] -as [int]

	if ($IncreaseMajor) {
		$MajorVersion = $MajorVersion + 1;
		$MinorVersion = 0;
		$BuildVersion = 0;
		$RevisionVersion = 0;
	}
	if ($IncreaseMinor) {
		$MinorVersion = $MinorVersion + 1;
		$BuildVersion = 0;
		$RevisionVersion = 0;
	}
	if ($IncreaseBuild) {
		$BuildVersion = $BuildVersion + 1;
		$RevisionVersion = 0;
	}
	if ($IncreaseRevision) {
		$RevisionVersion = $RevisionVersion + 1;
	}

	$NewVersion = '{0}.{1}.{2}.{3}' -f $MajorVersion, $MinorVersion, $BuildVersion, $RevisionVersion
}

$NewVersion.Split('.') | Out-File ($localPath + "version.txt")
#Get-Content -Path ($localPath + "version.txt")

$AssemblyFiles = Get-ChildItem -Path .\ -Filter AssemblyInfo.cs -Recurse -ErrorAction SilentlyContinue -Force | % { $_.FullName }
$VersionText = 'Version("{0}")' -f $NewVersion

foreach ($file in $AssemblyFiles) {
	if (Test-Path $file -PathType leaf) {
		$content = Get-Content $file
		$content -replace 'Version\(".*"\)',$VersionText > $file
	}
}

if (Test-Path -Path "Binaries\") {
    msbuild Build.xml /p:Configuration=Release /p:VersionNumber=$NewVersion /t:ReleaseWithBinaries
} else {
    msbuild Build.xml /p:Configuration=Release /p:VersionNumber=$NewVersion /t:ReleaseNoBinaries
}

"`r`nBuilt new release with build version {0}.`r`n" -f $NewVersion

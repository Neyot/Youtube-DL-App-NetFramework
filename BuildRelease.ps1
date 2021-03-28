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
	}
	if ($IncreaseMinor) {
		$MinorVersion = $MinorVersion + 1;
	}
	if ($IncreaseBuild) {
		$BuildVersion = $BuildVersion + 1;
	}
	if ($IncreaseRevision) {
		$RevisionVersion = $RevisionVersion + 1;
	}

	$NewVersion = '{0}.{1}.{2}.{3}' -f $MajorVersion, $MinorVersion, $BuildVersion, $RevisionVersion
	$BuildVersion = '{0}.{1}.{2}' -f $MajorVersion, $MinorVersion, $BuildVersion
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
    msbuild Build.xml /p:Configuration=Release /p:VersionNumber=$BuildVersion /t:ReleaseWithBinaries
} else {
    msbuild Build.xml /p:Configuration=Release /p:VersionNumber=$BuildVersion /t:ReleaseNoBinaries
}

"`r`nBuilt new release with build version {0} (Full Version: {1}).`r`n" -f $BuildVersion, $NewVersion

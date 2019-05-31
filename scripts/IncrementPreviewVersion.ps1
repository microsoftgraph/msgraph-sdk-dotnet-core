# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

<#
.Synopsis
    Set or increment the preview version string based on whether there exists
    a publicly release preview version.

.Parameter packageName
    Specifies the package name of the package. For example, 'microsoft.graph.core'
    is a valid package name.

.Parameter projectPath
    Specifies the path to the project file.

#>

Param(
    [parameter(Mandatory = $true)]
    [string]$packageName,

    [parameter(Mandatory = $true)]
    [string]$projectPath
)

[xml]$xmlDoc = Get-Content $projectPath

# Assumption: VersionSuffix is set in the first property group.
$versionSuffixString = $xmlDoc.Project.PropertyGroup[0].VersionSuffix

# Don't do anything if a VersionSuffix has been set in the .csproj.
if ($versionSuffixString -ne '' -and $versionSuffixString -ne $null) {
    Write-Host "The VersionSuffix has been set as $versionSuffixString in the csproj file. `
    Skip the automatic setting or incrementing of the version suffix. Delete the value in `
    VersionSuffix to enable auto-incrementing the preview version." -ForegroundColor Yellow

    Exit 0
}

if ($versionSuffixString -eq $null) {
    Write-Host "The VersionSuffix element had been deleted from the csproj. Adding it back."
    $newVersionSuffixElement = $xmlDoc.CreateElement("VersionSuffix")
    $newElement = $xmlDoc.Project.PropertyGroup[0].AppendChild($newVersionSuffixElement)
}

# API is case-sensitive
$packageName = $packageName.ToLower()
$url = "https://api.nuget.org/v3/registration3/$packageName/index.json"

# Call the NuGet API for the package and get the highest SemVer 2.0.0 version for the package.
# Per rules https://semver.org/spec/v2.0.0.html#spec-item-11
$nugetIndex = Invoke-RestMethod -Uri $url -Method Get
$highestPublishedVersion = $nugetIndex.items[0].upper

# We do need to make sure it is listed. For example, we didn't properly suffix M.G.A so the
# highest reported version is incorrect.

# We assume that the Version takes the form of 'x.y.z' with an optional '-preview.n' appended for preview releases.
Write-Host "The highest published version of $packageName is $highestPublishedVersion"

# Set or increment the VersionSuffix.
if ($highestPublishedVersion.Indexof('-preview') -eq -1) {
    $versionSuffixString = 'preview.1' # Build applies the hyphen
}
else {
    # A preview has been previously released. Let's increment the VersionSuffix.
    $currentPreviewVersion = [int]$highestPublishedVersion.Split('-')[1].Split('.')[1]

    $incrementedPreviewVersion = ($currentPreviewVersion + 1).ToString()

    $versionSuffixString = "preview.{0}" -f $incrementedPreviewVersion
}

Write-Host "The preview version is now $versionSuffixString" -ForegroundColor Green

$xmlDoc.Project.PropertyGroup[0].VersionSuffix = $versionSuffixString
$xmlDoc.Save($projectPath)
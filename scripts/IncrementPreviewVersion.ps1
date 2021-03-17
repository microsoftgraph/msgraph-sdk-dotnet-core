# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

<#
.Synopsis
    Set or increment the preview version string based on whether there exists
    a publicly released preview version.

.Parameter packageName
    Specifies the package name of the package. For example, 'microsoft.graph.core'
    is a valid package name.

.Parameter projectPath
    Specifies the path to the project file.

#>

[CmdletBinding()]
Param(
    [parameter(Mandatory = $false)]
    [string]$packageName = 'microsoft.graph.core',

    [parameter(Mandatory = $false)]
    [string]$projectPath = '.\src\Microsoft.Graph.Core\Microsoft.Graph.Core.csproj'
)

$xmlDoc = New-Object System.Xml.XmlDocument
$xmlDoc.Load($projectPath)

# Assumption: VersionSuffix is set in the first property group.
$versionSuffixString = $xmlDoc.Project.PropertyGroup[0].VersionSuffix

# Don't do anything if a VersionSuffix has been set in the .csproj.
if ($versionSuffixString -ne '' -and $versionSuffixString -ne $null) {
    Write-Host "`tThe VersionSuffix has been set as $versionSuffixString in the csproj file. `
`tSkip the automatic setting or incrementing of the version suffix. Delete the value in `
`tVersionSuffix to enable auto-incrementing the preview version." -ForegroundColor Yellow

    Exit 0
}

if ($versionSuffixString -eq $null) {
    Write-Host "The VersionSuffix element had been deleted from the csproj. Adding it back."
    $newVersionSuffixElement = $xmlDoc.CreateElement("VersionSuffix")
    $newElement = $xmlDoc.Project.PropertyGroup[0].AppendChild($newVersionSuffixElement)
}

# API is case-sensitive
$packageName = $packageName.ToLower()
$url = "https://api.nuget.org/v3-flatcontainer/$packageName/index.json"

# Call the NuGet API for the package and get the highest SemVer 2.0.0 version for the package.
# Per rules https://semver.org/spec/v2.0.0.html#spec-item-11
$nugetIndex = Invoke-RestMethod -Uri $url -Method Get

# We need the versionPrefix from the csproj so that we target the 
# the highest version. This enables us to support v1.x.x and vNext.x.x
$versionPrefix = $xmlDoc.Project.PropertyGroup[0].VersionPrefix

# Only consider versions that match the version prefix in the csproj.
$versionmatches = $nugetIndex.versions -match $versionPrefix
if ($versionmatches.Count -eq 0) {
    # if the version hasn't been published, we need to add a preview versionSuffix.
    # Add an initial preview versionSuffix
    $versionSuffixString = 'preview.1' # Build applies the hyphen
}
else { 
    # We have published preview versions for this version prefix.

    # Only look at preview versions, also to referred to as versionSuffix
    $versionmatches = $versionmatches -match 'preview'

    # Assumption: the API returns the versions in order.
    $highestPublishedVersion = $versionmatches[$versionmatches.Count - 1]

    # We assume that the Version takes the form of 'x.y.z' with an optional '-preview.n' appended for preview releases.
    Write-Host "The highest published version of $packageName is $highestPublishedVersion"

    # if the version has been published with a versionSuffix, then increment the suffix.    
    # A preview has been previously released. Let's increment the VersionSuffix.
    $currentPreviewVersion = [int]$highestPublishedVersion.Split('-')[1].Split('.')[1]

    $incrementedPreviewVersion = ($currentPreviewVersion + 1).ToString()

    $versionSuffixString = "preview.{0}" -f $incrementedPreviewVersion
}

Write-Host "The preview version is now $versionPrefix-$versionSuffixString" -ForegroundColor Green

$xmlDoc.Project.PropertyGroup[0].VersionSuffix = $versionSuffixString
$xmlDoc.Save($projectPath)
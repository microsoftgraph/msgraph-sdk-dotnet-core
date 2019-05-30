# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

<#
.Synopsis
    Gets the latest production release version of the specified NuGet package.

.Description
    Gets the NuGet package version of latest production release and compares the
    version to the version set in the specified project file. If they match, this
    script will fail and indicate that the version needs to be updated.

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

# Assumption: VersionPrefix is set in the first property group.
$versionPrefixString = $xmlDoc.Project.PropertyGroup[0].VersionPrefix

# System.Version, get the version prefix.
$currentProjectVersion = [version]"$versionPrefixString"

# Get the current version in SDK.
$majorVersion = $currentProjectVersion.Major.ToString()
$minorVersion = $currentProjectVersion.Minor.ToString()
$patchVersion = $currentProjectVersion.Build.ToString()

# API is case-sensitive
$packageName = $packageName.ToLower()
$url = "https://api.nuget.org/v3/registration3/$packageName/index.json"

# Call the NuGet API for the package and get the current published version.
$nugetIndex = Invoke-RestMethod -Uri $url -Method Get
$currentPublishedVersion = $nugetIndex.items[0].upper

$publishedMajorVersion = $currentPublishedVersion.Split(".")[0]
$publishedMinorVersion = $currentPublishedVersion.Split(".")[1]
$publishedPatchVersion = $currentPublishedVersion.Split(".")[2]

# Validate that the version number has been updated.
if ($majorVersion -eq $publishedMajorVersion -and `
        $minorVersion -eq $publishedMinorVersion -and `
        $patchVersion -eq $publishedPatchVersion) {

    Write-Error "The current published version number, $currentPublishedVersion, and the version number `
               in the csproj file, $currentProjectVersion, match. You must increment the version `
               before you complete this pull request."
}
else
{
    Write-Host "Validated that the version has been updated from $currentPublishedVersion to $currentProjectVersion" -ForegroundColor Green
}
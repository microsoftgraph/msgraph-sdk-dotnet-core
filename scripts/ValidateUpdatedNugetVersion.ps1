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
    Specifies the package name of the package. For example, 'microsoft.kiota.abstractions'
    is a valid package name.

.Parameter projectPath
    Specifies the path to the project file.

.Parameter registrationsBaseUrl
    Base URL of the NuGet v3 registrations resource used to look up the latest published
    version. Defaults to the public nuget.org endpoint. In network-isolated pipelines (CFSClean)
    pass the CFS/Azure Artifacts feed registrations endpoint (which upstreams nuget.org) so the
    lookup does not egress to api.nuget.org. When the URL is an Azure Artifacts feed it requires
    authentication; set the SYSTEM_ACCESSTOKEN environment variable and it will be sent as a
    Bearer token.
#>

Param(
    [parameter(Mandatory = $true)]
    [string]$packageName,

    [parameter(Mandatory = $true)]
    [string]$projectPath,

    [parameter(Mandatory = $false)]
    [string]$registrationsBaseUrl = "https://api.nuget.org/v3/registration5-gz-semver2"
)

[xml]$xmlDoc = Get-Content $projectPath

# Assumption: VersionPrefix is set in the first property group.
$versionPrefixString = $xmlDoc.Project.PropertyGroup[0].VersionPrefix
if($xmlDoc.Project.PropertyGroup[0].VersionSuffix){
    $versionPrefixString = $versionPrefixString + "-"  + $xmlDoc.Project.PropertyGroup[0].VersionSuffix
}


# System.Version, get the version prefix.
$currentProjectVersion = [System.Management.Automation.SemanticVersion]"$versionPrefixString"

# API is case-sensitive
$packageName = $packageName.ToLower()
$url = "$($registrationsBaseUrl.TrimEnd('/'))/$packageName/index.json"

# Azure Artifacts feed endpoints require authentication; send the pipeline access token when
# available. Public nuget.org ignores the header.
$headers = @{}
if ($env:SYSTEM_ACCESSTOKEN) {
    $headers["Authorization"] = "Bearer $env:SYSTEM_ACCESSTOKEN"
}

# Call the NuGet API for the package and get the current published version.
Try {
    $nugetIndex = Invoke-RestMethod -Uri $url -Method Get -Headers $headers
}
Catch {
    $statusCode = $_.Exception.Response.StatusCode.value__
    if ($statusCode -eq 404 -or ($_.ErrorDetails.Message -and $_.ErrorDetails.Message.Contains("The specified blob does not exist."))) {
        Write-Host "No package exists. You will probably be publishing $packageName for the first time."
        Exit # exit gracefully
    }

    Write-Host $_
    Exit 1
}

$currentPublishedVersion = [System.Management.Automation.SemanticVersion]$nugetIndex.items[$nugetIndex.items.Count-1].upper

# Validate that the version number has been updated.
if ($currentProjectVersion -le $currentPublishedVersion) {

    Write-Error "The current published version number, $currentPublishedVersion, and the version number `
               in the csproj file, $currentProjectVersion, match. You must increment the version"
}
else {
    Write-Host "Validated that the version has been updated from $currentPublishedVersion to $currentProjectVersion" -ForegroundColor Green
}
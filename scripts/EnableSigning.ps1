# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

<#
.Synopsis
    Sets the project ready for signing.

.Description
    This allows us to not have to checkin .csproj files with DelaySign and SignAssembly set to to true.
    If the flag is set, then project is not debuggable with SignAssembly set to true.
    Assumption: working directory is /src/

.Parameter projectPath
    Specifies the path to the project file.
#>

Param(
    [parameter(Mandatory = $true)]
    [string]$projectPath
)

$doc = New-Object System.Xml.XmlDocument
$doc.Load($projectPath)

# Set the DelaySign element to 'true' so that delay signing is set.
$delaySign = $doc.SelectSingleNode("//DelaySign");
$delaySign.'#text' = "true"

# Set the SignAssembly element to 'true' so that we can sign the assemblies.
$signAssembly = $doc.SelectSingleNode("//SignAssembly");
$signAssembly.'#text' = "true"

$doc.Save($projectPath);

Write-Host "Updated the .csproj file so that we can sign the built assemblies." -ForegroundColor Green
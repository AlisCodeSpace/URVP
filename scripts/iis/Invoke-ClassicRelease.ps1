#Requires -Version 5.1
<#
.SYNOPSIS
    Classic Azure DevOps Release entry point. Finds Build.zip, extracts it, and publishes onto IIS.

    Artifact layout (from azure-pipelines.yml):
      FEA.URVP\FEA.URVP.Backend\   published site
      FEA.URVP\iis-scripts\        this folder
#>
[CmdletBinding()]
param(
    [string] $ZipPath,
    [string] $PhysicalPath = 'C:\inetpub\wwwroot\FEA.URVP',
    [string] $SiteName = 'FEA.URVP',
    [Parameter(Mandatory = $true)]
    [ValidateSet('Staging', 'Production')]
    [string] $EnvironmentName,
    [string] $PublicHostname
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Find-BuildZip {
    param([string] $Explicit)

    if ($Explicit) {
        if (-not (Test-Path -LiteralPath $Explicit)) {
            throw "Build.zip not found at $Explicit"
        }
        return (Resolve-Path -LiteralPath $Explicit).Path
    }

    $roots = @()
    if ($env:SYSTEM_DEFAULTWORKINGDIRECTORY) { $roots += $env:SYSTEM_DEFAULTWORKINGDIRECTORY }
    if ($env:AGENT_RELEASEDIRECTORY) { $roots += $env:AGENT_RELEASEDIRECTORY }
    if ($env:PIPELINE_WORKSPACE) { $roots += $env:PIPELINE_WORKSPACE }
    $roots += (Get-Location).Path

    foreach ($root in $roots) {
        if (-not $root -or -not (Test-Path -LiteralPath $root)) { continue }
        $hit = Get-ChildItem -LiteralPath $root -Filter 'Build.zip' -Recurse -File -ErrorAction SilentlyContinue |
            Select-Object -First 1
        if ($hit) { return $hit.FullName }
    }

    throw "Build.zip not found under $($roots -join ', '). Pass -ZipPath."
}

$zip = Find-BuildZip -Explicit $ZipPath
Write-Host "Using zip $zip"

$tempRoot = if ($env:AGENT_TEMPDIRECTORY) { $env:AGENT_TEMPDIRECTORY } else { $env:TEMP }
$extractRoot = Join-Path $tempRoot ("urvp-release-" + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Force -Path $extractRoot | Out-Null
try {
    Expand-Archive -LiteralPath $zip -DestinationPath $extractRoot -Force

    $source = Join-Path $extractRoot 'FEA.URVP\FEA.URVP.Backend'
    $dll = Join-Path $source 'FEA.URVP.Backend.dll'
    $index = Join-Path $source 'wwwroot\index.html'
    if (-not (Test-Path -LiteralPath $dll)) {
        throw "FEA.URVP.Backend.dll missing under $source — zip layout looks wrong."
    }
    if (-not (Test-Path -LiteralPath $index)) {
        throw "wwwroot\index.html missing under $source. The Next.js export was not packaged."
    }

    $publish = Join-Path $PSScriptRoot 'Publish-IisSite.ps1'
    if (-not (Test-Path -LiteralPath $publish)) {
        $publish = Join-Path $extractRoot 'FEA.URVP\iis-scripts\Publish-IisSite.ps1'
    }
    if (-not (Test-Path -LiteralPath $publish)) {
        throw "Publish-IisSite.ps1 not found next to this script or in the zip."
    }

    & $publish `
        -SourcePath $source `
        -PhysicalPath $PhysicalPath `
        -SiteName $SiteName `
        -EnvironmentName $EnvironmentName
}
finally {
    Remove-Item -LiteralPath $extractRoot -Recurse -Force -ErrorAction SilentlyContinue
}

if ($PublicHostname) {
    $urls = @(
        "https://$PublicHostname/health/live",
        "http://127.0.0.1/health/live"
    )
    $ok = $false
    foreach ($url in $urls) {
        try {
            $response = Invoke-WebRequest -UseBasicParsing -Uri $url -Headers @{ Host = $PublicHostname } -TimeoutSec 30
            if ($response.StatusCode -eq 200 -and $response.Content -match 'healthy') {
                Write-Host "OK $($response.StatusCode) $url"
                $ok = $true
                break
            }
            Write-Warning "$url returned $($response.StatusCode) $($response.Content)"
        }
        catch {
            Write-Warning "$url failed: $($_.Exception.Message)"
        }
    }
    if (-not $ok) {
        Write-Warning "Could not confirm /health/live. Check the IIS binding and certificate, then hit the URL from a browser."
    }
}

#Requires -Version 5.1
<#
.SYNOPSIS
    Optional IIS helper. Not a build artifact. Writes ASPNETCORE_ENVIRONMENT to applicationHost.config
    and web.config so a wipe-and-unzip cannot revert the site to Development.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidateSet('Staging', 'Production')]
    [string] $EnvironmentName,

    [Parameter(Mandatory = $true)]
    [string] $SiteName,

    [Parameter(Mandatory = $true)]
    [string] $WebConfigPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Set-WebConfigEnvironment {
    param(
        [string] $Path,
        [string] $Value
    )

    if (-not (Test-Path -LiteralPath $Path)) {
        throw "web.config not found at $Path"
    }

    [xml] $xml = Get-Content -LiteralPath $Path
    $aspNetCore = $xml.SelectSingleNode('//aspNetCore')
    if ($null -eq $aspNetCore) {
        throw "web.config at $Path has no <aspNetCore> element."
    }

    $vars = $aspNetCore.SelectSingleNode('environmentVariables')
    if ($null -eq $vars) {
        $vars = $xml.CreateElement('environmentVariables')
        [void] $aspNetCore.AppendChild($vars)
    }

    $existing = @($vars.SelectNodes("environmentVariable[@name='ASPNETCORE_ENVIRONMENT']"))
    foreach ($node in $existing) {
        [void] $vars.RemoveChild($node)
    }

    $el = $xml.CreateElement('environmentVariable')
    $el.SetAttribute('name', 'ASPNETCORE_ENVIRONMENT')
    $el.SetAttribute('value', $Value)
    [void] $vars.AppendChild($el)

    $xml.Save($Path)
    Write-Host "Set ASPNETCORE_ENVIRONMENT=$Value in $Path"
}

function Set-AppHostEnvironment {
    param(
        [string] $Site,
        [string] $Value
    )

    $appcmd = Join-Path $env:windir 'system32\inetsrv\appcmd.exe'
    if (-not (Test-Path -LiteralPath $appcmd)) {
        Write-Warning "appcmd.exe not found; skipped applicationHost.config update. IIS site-level environment will not persist across wipes until this runs as an administrator."
        return
    }

    $section = 'system.webServer/aspNetCore'
    $entry = "environmentVariables.[name='ASPNETCORE_ENVIRONMENT',value='$Value']"

    $null = & $appcmd set config $Site /section:$section "/-environmentVariables.[name='ASPNETCORE_ENVIRONMENT']" /commit:apphost 2>&1
    $add = & $appcmd set config $Site /section:$section "/+$entry" /commit:apphost 2>&1

    if ($LASTEXITCODE -ne 0) {
        Write-Warning "Could not persist ASPNETCORE_ENVIRONMENT in applicationHost.config for site '$Site'. The published web.config still has the value for this deploy. Output: $add"
        return
    }

    Write-Host "Set ASPNETCORE_ENVIRONMENT=$Value on IIS site '$Site' (applicationHost.config). This survives wipe-and-unzip."
}

Set-WebConfigEnvironment -Path $WebConfigPath -Value $EnvironmentName
Set-AppHostEnvironment -Site $SiteName -Value $EnvironmentName

#Requires -Version 5.1
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string] $SourcePath,

    [Parameter(Mandatory = $true)]
    [string] $PhysicalPath,

    [Parameter(Mandatory = $true)]
    [string] $SiteName,

    [Parameter(Mandatory = $true)]
    [ValidateSet('Staging', 'Production')]
    [string] $EnvironmentName
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if (-not (Test-Path -LiteralPath $SourcePath)) {
    throw "Publish output not found at $SourcePath"
}

$dll = Join-Path $SourcePath 'FEA.URVP.Backend.dll'
if (-not (Test-Path -LiteralPath $dll)) {
    throw "FEA.URVP.Backend.dll missing under $SourcePath — publish layout looks wrong."
}

$wwwroot = Join-Path $SourcePath 'wwwroot'
$index = Join-Path $wwwroot 'index.html'
if (-not (Test-Path -LiteralPath $index)) {
    throw "wwwroot\index.html missing under $SourcePath. The Next.js export was not copied; this would ship an API-only host."
}

New-Item -ItemType Directory -Force -Path $PhysicalPath | Out-Null

Import-Module WebAdministration -ErrorAction Stop

$site = Get-Website -Name $SiteName -ErrorAction SilentlyContinue
if ($null -eq $site) {
    throw "IIS site '$SiteName' does not exist. Create the site, bind the hostname, and assign the app-pool identity before deploying."
}

$preserveDir = Join-Path ([System.IO.Path]::GetTempPath()) ("urvp-preserve-" + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Force -Path $preserveDir | Out-Null

try {
    $localSettings = Get-ChildItem -LiteralPath $PhysicalPath -Filter 'appsettings.*.local.json' -ErrorAction SilentlyContinue
    foreach ($file in $localSettings) {
        Copy-Item -LiteralPath $file.FullName -Destination (Join-Path $preserveDir $file.Name) -Force
        Write-Host "Preserved $($file.Name)"
    }

    Write-Host "Stopping IIS site '$SiteName'"
    $poolName = $site.applicationPool
    if ($poolName) {
        try {
            Set-ItemProperty -Path "IIS:\AppPools\$poolName" -Name processModel.loadUserProfile -Value $true
            Write-Host "Ensured Load User Profile = true on app pool '$poolName' (required for Data Protection DPAPI)."
        }
        catch {
            Write-Warning "Could not set Load User Profile on app pool '$poolName'. Data Protection DPAPI will fail until it is enabled. $_"
        }

        $pool = Get-WebAppPoolState -Name $poolName -ErrorAction SilentlyContinue
        if ($null -ne $pool -and $pool.Value -ne 'Stopped') {
            Write-Host "Stopping app pool '$poolName'"
            Stop-WebAppPool -Name $poolName
            $waited = 0
            while ($waited -lt 30) {
                Start-Sleep -Seconds 1
                $waited++
                $state = (Get-WebAppPoolState -Name $poolName).Value
                if ($state -eq 'Stopped') {
                    break
                }
            }
        }
    }
    Stop-Website -Name $SiteName -ErrorAction SilentlyContinue

    Get-ChildItem -LiteralPath $PhysicalPath -Force | Where-Object {
        $_.Name -ne 'logs' -and $_.Name -notlike 'appsettings.*.local.json'
    } | Remove-Item -Recurse -Force

    Write-Host "Copying $SourcePath -> $PhysicalPath"
    Copy-Item -Path (Join-Path $SourcePath '*') -Destination $PhysicalPath -Recurse -Force

    foreach ($file in (Get-ChildItem -LiteralPath $preserveDir -ErrorAction SilentlyContinue)) {
        Copy-Item -LiteralPath $file.FullName -Destination (Join-Path $PhysicalPath $file.Name) -Force
        Write-Host "Restored $($file.Name)"
    }

    $logs = Join-Path $PhysicalPath 'logs'
    New-Item -ItemType Directory -Force -Path $logs | Out-Null
    & icacls.exe $logs /grant "IIS_IUSRS:(OI)(CI)M" /T | Out-Null

    $setEnv = Join-Path $PSScriptRoot 'Set-AspNetCoreEnvironment.ps1'
    & $setEnv -EnvironmentName $EnvironmentName -SiteName $SiteName -WebConfigPath (Join-Path $PhysicalPath 'web.config')

    Write-Host "Starting IIS site '$SiteName'"
    if ($poolName) {
        $state = (Get-WebAppPoolState -Name $poolName).Value
        if ($state -eq 'Stopped') {
            Start-WebAppPool -Name $poolName
        }
    }
    Start-Website -Name $SiteName
}
finally {
    Remove-Item -LiteralPath $preserveDir -Recurse -Force -ErrorAction SilentlyContinue
}

Write-Host "Deployed $EnvironmentName to '$SiteName' at $PhysicalPath"

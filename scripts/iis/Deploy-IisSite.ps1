Import-Module IISAdministration

$build = "$(System.ArtifactsDirectory)\_Provost.URVP (Build)\app-release\Build.zip"
$siteName      = "FEA.URVP"
$appPoolName   = "FEA.URVPAppPool"
$siteDnsName   = "urvp-staging.aub.edu.lb"
$sitesRootPath = "C:\inetpub\wwwroot\"
$sitePath      = $sitesRootPath + "FEA.URVP"
$sitePathAPI   = $sitesRootPath + "FEA.URVP\FEA.URVP.Backend"

# Locate wildcard cert
$thumbprint = Get-ChildItem -Path Cert:\LocalMachine\My |
              Where-Object { $_.Subject.StartsWith("CN=soldext.aub.edu.lb") } |
              Select-Object -ExpandProperty Thumbprint -First 1

if (-not $thumbprint) {
  Write-Error "No  soldext.aub.edu.lb certificate found in LocalMachine\My"
  exit 1
}

# Create site if missing
$site = appcmd list site $siteName
if ($site.Count -eq 0) {
  $binding = "http://" + $siteDnsName + ":80"
  Write-Host "Creating site '$siteName'"
  appcmd add site /name:"$siteName" /bindings:"$binding" /physicalPath:"$sitePathAPI"
}

# Create app pool if missing
$appPool = appcmd list apppool $appPoolName
if ($appPool.Count -eq 0) {
  Write-Host "Creating app pool '$appPoolName'"
  appcmd add apppool /name:$appPoolName /managedRuntimeVersion:"" /managedPipelineMode:"Integrated"
  appcmd set app "$siteName/" /applicationPool:"$appPoolName"
}

# Stop pool + site
$appPoolStarted = appcmd list apppool /name:$appPoolName /state:Started
if ($appPoolStarted.Count -gt 0) { appcmd stop apppool $appPoolName }

$siteStarted = appcmd list site /name:$siteName /state:Started
if ($siteStarted.Count -gt 0) { appcmd stop site $siteName }

# Add HTTPS binding if missing
$sslBindingCount = (Get-WebBinding -Name $siteName -Protocol "https").Count
if ($sslBindingCount -eq 0) {
  $sslBindingInformation = "*:443:" + $siteDnsName
  Write-Host "Adding HTTPS binding"
  New-IISSiteBinding -Name $siteName `
                     -BindingInformation $sslBindingInformation `
                     -CertificateThumbPrint $thumbprint `
                     -CertStoreLocation "Cert:\LocalMachine\My" `
                     -Protocol https
} else {
  Write-Host "HTTPS binding already exists"
}

# Swap files
if (Test-Path $sitePath) {
  Write-Host "Removing old deployment"
  Remove-Item -Path $sitePath -Recurse -Force
}

Write-Host "Extracting new build"
Expand-Archive -Path $build -DestinationPath $sitesRootPath -Force

# Start pool + site
appcmd start apppool $appPoolName
appcmd start site $siteName

# Cleanup
Remove-Item $build -Force
Write-Host "Deployment complete."

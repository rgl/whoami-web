Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'
trap {
    Write-Output "ERROR: $_"
    Write-Output (($_.ScriptStackTrace -split '\r?\n') -replace '^(.*)$','ERROR: $1')
    Write-Output (($_.Exception.ToString() -split '\r?\n') -replace '^(.*)$','ERROR EXCEPTION: $1')
    Exit 1
}

$serviceName = Split-Path -Leaf $PSScriptRoot
$serviceHome = "c:\$serviceName"

# uninstall the service.
if (Get-Service $serviceName -ErrorAction SilentlyContinue) {
    Write-Host "Stopping the $serviceName windows service..."
    Stop-Service $serviceName

    Write-Host "Deleting the $serviceName windows service..."
    $result = sc.exe delete $serviceName
    if ($result -ne '[SC] DeleteService SUCCESS') {
        throw "sc.exe delete failed with $result"
    }
}

# uninstall the binaries.
if (Test-Path Program.cs) {
    if (Test-Path $serviceHome) {
        Write-Host "Deleting the $serviceHome directory..."
        Remove-Item -Recurse $serviceHome
    }
}

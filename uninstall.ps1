Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'
trap {
    Write-Output "ERROR: $_"
    Write-Output (($_.ScriptStackTrace -split '\r?\n') -replace '^(.*)$','ERROR: $1')
    Write-Output (($_.Exception.ToString() -split '\r?\n') -replace '^(.*)$','ERROR EXCEPTION: $1')
    Exit 1
}

# uninstall the service.
if (Get-Service whoami -ErrorAction SilentlyContinue) {
    Stop-Service whoami
    $result = sc.exe delete whoami
    if ($result -ne '[SC] DeleteService SUCCESS') {
        throw "sc.exe delete failed with $result"
    }
}

# uninstall the binaries.
if (Test-Path Program.cs) {
    if (Test-Path c:\whoami-web) {
        Remove-Item -Recurse c:\whoami-web
    }
}

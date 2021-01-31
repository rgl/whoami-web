Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'
trap {
    Write-Output "ERROR: $_"
    Write-Output (($_.ScriptStackTrace -split '\r?\n') -replace '^(.*)$','ERROR: $1')
    Write-Output (($_.Exception.ToString() -split '\r?\n') -replace '^(.*)$','ERROR EXCEPTION: $1')
    Exit 1
}

# NB we use the debug version because its faster to build than doing a publish.
dotnet build
if ($LASTEXITCODE) {
    throw "dotnet build failed with exit code $LASTEXITCODE"
}

if (Get-Service whoami -ErrorAction SilentlyContinue) {
    Stop-Service whoami
}

if (Test-Path c:\whoami-web) {
    Remove-Item -Recurse c:\whoami-web
}

Copy-Item -Recurse 'bin\debug\net5.0\win10-x64' c:\whoami-web
Copy-Item -Recurse 'wwwroot' c:\whoami-web

if (Get-Service whoami -ErrorAction SilentlyContinue) {
    Start-Service whoami
}

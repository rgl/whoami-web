Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'
trap {
    Write-Output "ERROR: $_"
    Write-Output (($_.ScriptStackTrace -split '\r?\n') -replace '^(.*)$','ERROR: $1')
    Write-Output (($_.Exception.ToString() -split '\r?\n') -replace '^(.*)$','ERROR EXCEPTION: $1')
    Exit 1
}

# NB quite astoundingly dotnet build (or msbuild) sometimes does not correctly
#    set its exit code, so we have to inspected the output...
#    see https://github.com/dotnet/msbuild/issues/5689
#    see https://docs.microsoft.com/en-us/visualstudio/msbuild/msbuild-command-line-reference?view=vs-2019
# NB we use the debug version because its faster to build than doing a publish.
dotnet build -warnAsError -flp:verbosity=detailed -flp:logfile=build.log
if ((Get-Content -Tail 10 build.log) -notcontains 'Build succeeded.') {
    throw 'dotnet build failed. see the details in build.log'
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

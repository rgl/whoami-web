Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'
trap {
    Write-Output "ERROR: $_"
    Write-Output (($_.ScriptStackTrace -split '\r?\n') -replace '^(.*)$','ERROR: $1')
    Write-Output (($_.Exception.ToString() -split '\r?\n') -replace '^(.*)$','ERROR EXCEPTION: $1')
    Exit 1
}

# build the binaries.
# NB quite astoundingly dotnet build (or msbuild) sometimes does not correctly
#    set its exit code, so we have to inspected the output...
#    see https://github.com/dotnet/msbuild/issues/5689
#    see https://docs.microsoft.com/en-us/visualstudio/msbuild/msbuild-command-line-reference?view=vs-2019
# NB we use the debug version because its faster to build than doing a publish.
if (Test-Path Program.cs) {
    dotnet build -warnAsError -flp:verbosity=detailed -flp:logfile=build.log
    if ((Get-Content -Tail 10 build.log) -notcontains 'Build succeeded.') {
        throw 'dotnet build failed. see the details in build.log'
    }
    $installPath = 'c:\whoami-web'
} else {
    $installPath = $PSScriptRoot
}

# uninstall the existing version.
.\uninstall.ps1

# install the service.
$result = sc.exe create whoami `
    DisplayName= 'Who Am I?' `
    binPath= "$installPath\whoami.exe" `
    obj= 'EXAMPLE\whoami$' `
    start= auto
if ($result -ne '[SC] CreateService SUCCESS') {
    throw "sc.exe create failed with $result"
}
$result = sc.exe description whoami `
    'The introspective service'
if ($result -ne '[SC] ChangeServiceConfig2 SUCCESS') {
    throw "sc.exe description failed with $result"
}
$result = sc.exe failure whoami `
    reset= '0' `
    actions= 'restart/60000'
if ($result -ne '[SC] ChangeServiceConfig2 SUCCESS') {
    throw "sc.exe failure failed with $result"
}

# install the binaries.
if (Test-Path Program.cs) {
    Copy-Item -Recurse 'bin\debug\net5.0\win10-x64' $installPath
    Copy-Item -Recurse 'wwwroot' $installPath
}

# start the service.
Start-Service whoami

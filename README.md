# About

[![Build status](https://github.com/rgl/whoami-web/workflows/Build/badge.svg)](https://github.com/rgl/whoami-web/actions?query=workflow%3ABuild)

This is an example ASP.NET 5 application that shows the application user details and the authenticated user details (from integrated authentication).

This is intended to be run as a windows service inside the [rgl/windows-domain-controller-vagrant](https://github.com/rgl/windows-domain-controller-vagrant) windows test node.

## Usage

Install the windows service as the `whoami` gMSA:

```powershell
$result = sc.exe create whoami `
    DisplayName= 'Who Am I?' `
    binPath= "C:\whoami-web\whoami.exe" `
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
```

Install the binaries at `C:\whoami-web` and start the service:

```powershell
.\install.ps1
```

After you are done with the service, uninstall it:

```powershell
Stop-Service whoami
$result = sc.exe delete whoami
if ($result -ne '[SC] DeleteService SUCCESS') {
    throw "sc.exe delete failed with $result"
}
```

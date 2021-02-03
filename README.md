# About

[![Build status](https://github.com/rgl/whoami-web/workflows/Build/badge.svg)](https://github.com/rgl/whoami-web/actions?query=workflow%3ABuild)

This is an example ASP.NET 5 application that shows the application user details and the authenticated user details (from integrated authentication).

This is intended to be run as a windows service inside the [rgl/windows-domain-controller-vagrant](https://github.com/rgl/windows-domain-controller-vagrant) windows test node.

## Usage

Build the binaries, install them at `C:\whoami-web`, install the `whoami`
service as the `whoami` gMSA, and start it:

```powershell
.\install.ps1
```

After you are done with the service, uninstall it:

```powershell
.\uninstall.ps1
```

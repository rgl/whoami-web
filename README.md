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

## References

* [Group Managed Service Accounts Overview](https://docs.microsoft.com/en-us/windows-server/security/group-managed-service-accounts/group-managed-service-accounts-overview)
* [How the Kerberos Version 5 Authentication Protocol Works](https://docs.microsoft.com/en-us/previous-versions/windows/it-pro/windows-server-2003/cc772815(v=ws.10)?redirectedfrom=MSDN)
* [Impersonation Levels (Authorization)](https://docs.microsoft.com/en-us/windows/win32/secauthz/impersonation-levels)

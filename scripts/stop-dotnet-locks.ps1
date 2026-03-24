param(
    [Parameter(Mandatory = $true)]
    [string]$AppProcessName,

    [Parameter(Mandatory = $true)]
    [string]$DllPattern
)

$ErrorActionPreference = 'SilentlyContinue'

# Kill native apphost process if present.
Get-Process -Name $AppProcessName | Stop-Process -Force

# Kill dotnet hosts running this app's DLL from bin/Debug.
Get-CimInstance Win32_Process -Filter "Name = 'dotnet.exe'" |
    Where-Object { $_.CommandLine -and $_.CommandLine -like "*$DllPattern*" } |
    ForEach-Object { Stop-Process -Id $_.ProcessId -Force }

Start-Sleep -Milliseconds 500

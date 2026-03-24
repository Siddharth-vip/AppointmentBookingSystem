$ErrorActionPreference = 'Stop'

Write-Host 'Stopping existing API/UI processes...'
Get-Process Appointment.API,Appointment.UI,dotnet -ErrorAction SilentlyContinue |
    Where-Object { $_.Path -like '*AppointmentBookingSystem*' } |
    Stop-Process -Force -ErrorAction SilentlyContinue

Start-Sleep -Seconds 1

Write-Host 'Cleaning API output...'
Push-Location 'Appointment.API'
if (Test-Path 'bin') { Remove-Item -Recurse -Force 'bin' }
if (Test-Path 'obj') { Remove-Item -Recurse -Force 'obj' }
Pop-Location

Write-Host 'Building solution...'
dotnet build | Out-Host

Write-Host 'Starting API...'
Start-Process powershell -ArgumentList '-NoExit', '-Command', 'cd Appointment.API; dotnet run --no-build'

Write-Host 'Starting UI...'
Start-Process powershell -ArgumentList '-NoExit', '-Command', 'cd Appointment.UI; dotnet run --no-build'

Write-Host 'Done. API: http://localhost:5038  UI: http://localhost:5046'

# Test what Keys.C value is
Add-Type -AssemblyName System.Windows.Forms
$cKey = [System.Windows.Forms.Keys]::C
Write-Host "Keys.C value: $cKey"
Write-Host "Keys.C numeric: $([int]$cKey)"

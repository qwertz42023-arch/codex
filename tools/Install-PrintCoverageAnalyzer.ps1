param(
    [string]$InstallDir = "$env:ProgramFiles\PrintCoverageAnalyzer"
)

$ErrorActionPreference = 'Stop'

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$sourceExe = Join-Path $scriptDir 'PrintCoverageAnalyzer.exe'
$sourceBat = Join-Path $scriptDir 'Run-Analyse.bat'

if (-not (Test-Path $sourceExe)) {
    throw "PrintCoverageAnalyzer.exe wurde im selben Ordner wie das Install-Skript nicht gefunden."
}

New-Item -ItemType Directory -Path $InstallDir -Force | Out-Null
Copy-Item $sourceExe (Join-Path $InstallDir 'PrintCoverageAnalyzer.exe') -Force
Copy-Item $sourceBat (Join-Path $InstallDir 'Run-Analyse.bat') -Force

$desktop = [Environment]::GetFolderPath('CommonDesktopDirectory')
$shortcutPath = Join-Path $desktop 'PrintCoverageAnalyzer.lnk'
$targetPath = Join-Path $InstallDir 'Run-Analyse.bat'

$wsh = New-Object -ComObject WScript.Shell
$shortcut = $wsh.CreateShortcut($shortcutPath)
$shortcut.TargetPath = $targetPath
$shortcut.WorkingDirectory = $InstallDir
$shortcut.IconLocation = (Join-Path $InstallDir 'PrintCoverageAnalyzer.exe')
$shortcut.Save()

Write-Host "Installation abgeschlossen: $InstallDir"
Write-Host "Desktop-Shortcut erstellt: $shortcutPath"

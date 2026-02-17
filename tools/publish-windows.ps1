param(
    [ValidateSet('win-x64','win-arm64')]
    [string]$Runtime = 'win-x64',
    [string]$Configuration = 'Release',
    [string]$Version = '1.2.0'
)

$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
$publishDir = Join-Path $repoRoot "dist/publish/$Runtime"
$packageDir = Join-Path $repoRoot "dist/package"
$appDir = Join-Path $packageDir "PrintCoverageAnalyzer-$Runtime"

Write-Host "Publishing $Runtime ($Configuration) ..."
& dotnet publish (Join-Path $repoRoot 'PrintCoverageAnalyzer.csproj') `
  -c $Configuration `
  -r $Runtime `
  --self-contained true `
  /p:PublishSingleFile=true `
  /p:IncludeNativeLibrariesForSelfExtract=true `
  /p:PublishTrimmed=false `
  /p:Version=$Version `
  -o $publishDir

if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish fehlgeschlagen (ExitCode=$LASTEXITCODE)."
}

$exePath = Join-Path $publishDir 'PrintCoverageAnalyzer.exe'
if (-not (Test-Path $exePath)) {
    throw "EXE nicht gefunden nach Publish: $exePath"
}

if (Test-Path $appDir) {
    Remove-Item -Recurse -Force $appDir
}
New-Item -ItemType Directory -Path $appDir | Out-Null

Copy-Item $exePath $appDir
Copy-Item (Join-Path $repoRoot 'tools/Install-PrintCoverageAnalyzer.ps1') $appDir
Copy-Item (Join-Path $repoRoot 'README.md') $appDir

$zipPath = Join-Path $packageDir "PrintCoverageAnalyzer-$Runtime-v$Version.zip"
if (Test-Path $zipPath) {
    Remove-Item -Force $zipPath
}
Compress-Archive -Path "$appDir/*" -DestinationPath $zipPath

Write-Host "Done. Package:" $zipPath

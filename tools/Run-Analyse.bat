@echo off
setlocal

set EXE_DIR=%~dp0
set APP_EXE=%EXE_DIR%PrintCoverageAnalyzer.exe

if not exist "%APP_EXE%" (
  echo Fehler: PrintCoverageAnalyzer.exe wurde nicht gefunden.
  pause
  exit /b 1
)

start "" "%APP_EXE%"
exit /b 0

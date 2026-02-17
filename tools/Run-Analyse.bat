@echo off
setlocal

set EXE_DIR=%~dp0
set APP_EXE=%EXE_DIR%PrintCoverageAnalyzer.exe

if not exist "%APP_EXE%" (
  echo Fehler: PrintCoverageAnalyzer.exe wurde nicht gefunden.
  pause
  exit /b 1
)

if "%~1"=="" (
  echo Nutzung:
  echo   Datei(en) auf diese BAT ziehen ODER in CMD ausfuehren:
  echo   Run-Analyse.bat ^<datei1^> [datei2 ...] --json-out report.json
  echo.
  "%APP_EXE%" --help
  pause
  exit /b 0
)

"%APP_EXE%" %*
if errorlevel 1 (
  echo.
  echo Analyse mit Fehler beendet.
  pause
  exit /b 1
)

echo.
echo Analyse abgeschlossen.
pause
exit /b 0

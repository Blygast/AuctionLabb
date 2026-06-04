@echo off
REM ============================================================================
REM  AuctionLabb  -  one-command dev launcher (run.bat)
REM
REM  1. Verifies prerequisites (dotnet, node, npm)
REM  2. Restores .NET deps and (re-)installs npm deps
REM  3. Runs the test suites (backend + frontend lint)
REM  4. Starts backend + frontend in separate windows
REM  5. Opens the app in your default browser
REM
REM  Double-click, or run from anywhere: the script anchors at its own
REM  location. Close the two spawned cmd windows to stop the services.
REM ============================================================================

setlocal

REM --- Anchor at the repo root (this script's directory) -------------------
set "REPO_ROOT=%~dp0"
if "%REPO_ROOT:~-1%"=="\" set "REPO_ROOT=%REPO_ROOT:~0,-1%"

cls
echo.
echo ===================================================
echo  AuctionLabb  -  dev launcher
echo  Repo: %REPO_ROOT%
echo ===================================================
echo.

REM ============================================================================
REM  Step 1  -  Prerequisites
REM ============================================================================
echo [1/6] Checking prerequisites...
echo.

set "MISSING=0"
where dotnet >nul 2>&1
if errorlevel 1 (
    echo   [ERROR] .NET SDK not found in PATH.
    echo           Install from https://dotnet.microsoft.com/download
    set "MISSING=1"
)
where node >nul 2>&1
if errorlevel 1 (
    echo   [ERROR] Node.js not found in PATH.
    echo           Install from https://nodejs.org/
    set "MISSING=1"
)
where npm >nul 2>&1
if errorlevel 1 (
    echo   [ERROR] npm not found in PATH. Reinstall Node.js (it ships with npm).
    set "MISSING=1"
)

if "%MISSING%"=="1" goto :abort

echo   dotnet  -  OK
echo   node    -  OK
echo   npm     -  OK
echo.

REM ============================================================================
REM  Step 2  -  Install / update dependencies
REM ============================================================================
echo [2/6] Restoring .NET dependencies...
pushd "%REPO_ROOT%\api" >nul
call dotnet restore AuctionApi.slnx --nologo --verbosity quiet
if errorlevel 1 (
    echo   [ERROR] dotnet restore failed.
    popd
    goto :abort
)
popd >nul
echo   Done.
echo.

echo [2/6] Installing npm dependencies (npm install)...
pushd "%REPO_ROOT%\client" >nul
call npm install --no-audit --no-fund --loglevel=error
if errorlevel 1 (
    echo   [ERROR] npm install failed.
    popd
    goto :abort
)
popd >nul
echo   Done.
echo.

REM ============================================================================
REM  Step 3  -  Run tests
REM ============================================================================
echo [3/6] Running backend tests (unit + LocalDB integration)...
echo.
pushd "%REPO_ROOT%\api" >nul
call dotnet test AuctionApi.Core.Tests --nologo --verbosity normal
set "BACKEND_TEST_EXIT=%errorlevel%"
popd >nul
echo.
if not "%BACKEND_TEST_EXIT%"=="0" (
    echo   [WARN] Backend tests reported failures.
    echo          Press any key to continue and start the services anyway,
    echo          or close this window to abort.
    pause >nul
)
echo.

echo [3/6] Running frontend lint + type-check...
pushd "%REPO_ROOT%\client" >nul
call npm run lint
set "FRONTEND_LINT_EXIT=%errorlevel%"
popd >nul
if not "%FRONTEND_LINT_EXIT%"=="0" (
    echo   [WARN] Frontend lint reported issues.
    echo          Press any key to continue, or close to abort.
    pause >nul
)
echo.

REM ============================================================================
REM  Step 4  -  Start the services in their own windows
REM ============================================================================
echo [4/6] Starting backend in a new window...
start "AuctionLabb - Backend (https://localhost:5001)" /D "%REPO_ROOT%\api" cmd /k ^
  "echo === AuctionLabb Backend ===  & echo.  & echo URL: https://localhost:5001  & echo Swagger: https://localhost:5001/swagger  & echo.  & dotnet run --project AuctionApi --launch-profile https"

echo [4/6] Starting frontend in a new window...
start "AuctionLabb - Frontend (http://localhost:5173)" /D "%REPO_ROOT%\client" cmd /k ^
  "echo === AuctionLabb Frontend ===  & echo.  & echo URL: http://localhost:5173  & echo.  & npm run dev"

echo.
echo [5/6] Waiting 10 seconds for the services to bind their ports...
timeout /t 10 /nobreak >nul

REM ============================================================================
REM  Step 5  -  Open the browser
REM ============================================================================
echo [6/6] Opening browser to http://localhost:5173 ...
start "" "http://localhost:5173"

echo.
echo ===================================================
echo  AuctionLabb is up.
echo.
echo    Frontend  :  http://localhost:5173
echo    Backend   :  https://localhost:5001
echo    Swagger   :  https://localhost:5001/swagger
echo.
echo  Close the two spawned cmd windows to stop the services.
echo ===================================================
echo.
echo Press any key to close this launcher window.
pause >nul
exit /b 0

REM ============================================================================
REM  Abort label
REM ============================================================================
:abort
echo.
echo ===================================================
echo  Launcher aborted. Fix the issue above and re-run.
echo ===================================================
echo.
pause >nul
exit /b 1

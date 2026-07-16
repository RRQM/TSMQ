@echo off
setlocal EnableExtensions EnableDelayedExpansion

:: Clean-BuildArtifacts.cmd
:: Removes bin/obj/.vs folders beneath the script directory using pure CMD tooling.

pushd "%~dp0" >nul 2>&1

for /f "tokens=2 delims=: " %%C in ('chcp') do set "ORIGINAL_CODEPAGE=%%C"
if defined ORIGINAL_CODEPAGE set "ORIGINAL_CODEPAGE=%ORIGINAL_CODEPAGE: =%"
chcp 65001 >nul

set "TARGETS=bin obj .vs node_modules"
set "LOGLEVEL=Info"
set "NONINTERACTIVE=false"
set "WHATIF=false"
set "EXIT_CODE=0"
set "MATCHED=0"
set "REMOVED=0"
set "FAILED=0"
set "SIMULATED=0"

if "%~1"=="" goto AfterArgs
:ParseArgs
set "ARG=%~1"
if defined ARG (
    set "PREFIX=!ARG:~0,1!"
    if "!PREFIX!"=="/" (
        set "ARG=!ARG:~1!"
    ) else if "!PREFIX!"=="-" (
        if "!ARG:~1,1!"=="-" (
            set "ARG=!ARG:~2!"
        ) else (
            set "ARG=!ARG:~1!"
        )
    )
)
set "KEY="
set "VALUE="
for /f "tokens=1,2 delims==:" %%G in ("!ARG!") do (
    set "KEY=%%~G"
    set "VALUE=%%~H"
)
if /I "!KEY!"=="loglevel" (
    if not defined VALUE (
        call :Usage
        set "EXIT_CODE=1"
        goto Finalize
    )
    set "LOGLEVEL=!VALUE!"
) else if /I "!KEY!"=="noninteractive" (
    set "NONINTERACTIVE=true"
) else if /I "!KEY!"=="whatif" (
    set "WHATIF=true"
) else if /I "!KEY!"=="help" (
    call :Usage
    set "NONINTERACTIVE=true"
    goto Finalize
) else (
    echo Unknown argument: %~1
    call :Usage
    set "EXIT_CODE=1"
    goto Finalize
)
shift
if "%~1"=="" goto AfterArgs
goto ParseArgs

:AfterArgs
call :NormalizeLevel "%LOGLEVEL%" LOGLEVEL
if errorlevel 1 (
    set "EXIT_CODE=1"
    goto Finalize
)
call :GetLevelValue "%LOGLEVEL%" CURRENT_LEVEL
if errorlevel 1 (
    set "EXIT_CODE=1"
    goto Finalize
)

call :WriteLog Info "Starting cleanup in %CD%"
call :WriteLog Info "Targets: %TARGETS%"
if /I "%WHATIF%"=="true" call :WriteLog Warning "WhatIf enabled; directories will not be removed."

for /f "delims=" %%F in ('dir /a:d /b /s 2^>nul') do (
    for %%T in (%TARGETS%) do (
        call :MaybeHandle "%%~fF" "%%T"
    )
)

if "!MATCHED!"=="0" (
    call :WriteLog Info "No matching directories found."
) else (
    call :WriteLog Info "Matched !MATCHED! target directories."
)

call :WriteLog Info "Deleted: !REMOVED!  Simulated: !SIMULATED!  Failures: !FAILED!"
if !FAILED! gtr 0 set "EXIT_CODE=1"

:Finalize
if /I "%NONINTERACTIVE%"=="false" (
    echo.
    pause
)
if defined ORIGINAL_CODEPAGE chcp %ORIGINAL_CODEPAGE% >nul
popd >nul 2>&1

set "FINAL_CODE=%EXIT_CODE%"
endlocal & exit /b %FINAL_CODE%

:MaybeHandle
set "CHECK_PATH=%~1"
set "CHECK_TARGET=%~2"
for %%P in ("%CHECK_PATH%") do set "LEAF=%%~nxP"
if /I "!LEAF!"=="%CHECK_TARGET%" (
    set /a MATCHED+=1
    call :WriteLog Verbose "Found target: %CHECK_PATH%"
    call :ProcessDirectory "%CHECK_PATH%"
)
exit /b 0

:ProcessDirectory
set "TARGET_PATH=%~1"
if not exist "%TARGET_PATH%" (
    call :WriteLog Verbose "Already missing, skip: %TARGET_PATH%"
    exit /b 0
)
if /I "%WHATIF%"=="true" (
    call :WriteLog Warning "Would delete: %TARGET_PATH%"
    set /a SIMULATED+=1
    exit /b 0
)
rd /s /q "%TARGET_PATH%" 2>nul
if errorlevel 1 (
    call :WriteLog Error "Failed to delete: %TARGET_PATH%"
    set /a FAILED+=1
) else (
    call :WriteLog Warning "Deleted: %TARGET_PATH%"
    set /a REMOVED+=1
)
exit /b 0

:WriteLog
set "LEVEL=%~1"
set "MESSAGE=%~2"
call :GetLevelValue "%LEVEL%" __MSG_LEVEL
if errorlevel 1 exit /b 0
if %__MSG_LEVEL% gtr %CURRENT_LEVEL% exit /b 0
set "PREFIX=[%LEVEL%]"
echo %PREFIX% %MESSAGE%
exit /b 0

:NormalizeLevel
set "VALUE=%~1"
if /I "%VALUE%"=="ERROR" (set "%~2=Error" & exit /b 0)
if /I "%VALUE%"=="WARNING" (set "%~2=Warning" & exit /b 0)
if /I "%VALUE%"=="INFO" (set "%~2=Info" & exit /b 0)
if /I "%VALUE%"=="VERBOSE" (set "%~2=Verbose" & exit /b 0)
if /I "%VALUE%"=="SILENT" (set "%~2=Silent" & exit /b 0)
echo Invalid log level: %VALUE%
exit /b 1

:GetLevelValue
set "VALUE=%~1"
if /I "%VALUE%"=="Silent" (set "%~2=-1" & exit /b 0)
if /I "%VALUE%"=="Error" (set "%~2=0" & exit /b 0)
if /I "%VALUE%"=="Warning" (set "%~2=1" & exit /b 0)
if /I "%VALUE%"=="Info" (set "%~2=2" & exit /b 0)
if /I "%VALUE%"=="Verbose" (set "%~2=3" & exit /b 0)
echo Invalid log level map: %VALUE%
exit /b 1

:Usage
echo Clean-BuildArtifacts.cmd
echo   Recursively deletes bin/obj/.vs folders beneath this directory.
echo.
echo Usage:
echo   Clean-BuildArtifacts.cmd [options]
echo.
echo Options:
echo   -loglevel=Error^|Warning^|Info^|Verbose^|Silent   Controls log verbosity (default Info)
echo   -noninteractive                                   Skip pause before exit
echo   -whatif                                           List folders without deleting
echo   -help                                             Show this help
echo.
exit /b 0

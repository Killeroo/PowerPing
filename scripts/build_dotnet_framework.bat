@echo off

:: Set directory to location of the script
cd "%~dp0" 

:: Arguments check
::if [%~1]==[] (
::    echo.
::    echo ERROR: Not enough Arguments
::    echo USAGE: build.bat [C:\path\to\project]
::    exit /b 1
::)
::set projectPath=%~f1

:: Find appropriate msbuild path using vswhere
for /f "usebackq tokens=*" %%A in (`vswhere -version "[15.0,16.0)" -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe`) do SET msbuild_path=%%A

if "%msbuild_path%"=="" GOTO MSBUILD_NOT_FOUND

:: Run build command
call "%msbuild_path%" ..\PowerPing.sln /p:Configuration=Release /p:Platform="x64" /t:rebuild
if errorlevel 1 GOTO BUILD_FAILED
call "%msbuild_path%" ..\PowerPing.sln /p:Configuration=Release /p:Platform="x86" /t:rebuild
if errorlevel 1 GOTO BUILD_FAILED
goto:eof

:MSBUILD_NOT_FOUND
echo.
echo ERROR: Msbuild could not be found, cannot build project
exit /B 1
GOTO:eof

:BUILD_FAILED
echo.
echo ERROR: Msbuild failed, fix those errors
exit /B 1
GOTO:eof
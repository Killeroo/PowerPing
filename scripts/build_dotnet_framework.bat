@echo off

:: Arguments check
::if [%~1]==[] (
::    echo.
::    echo ERROR: Not enough Arguments
::    echo USAGE: build.bat [C:\path\to\project]
::    exit /b 1
::)
::set projectPath=%~f1

:: Set .NET 4.6 framework path
set msbuildPath="%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\"

:: Run build command
cd..
%msbuildPath%\msbuild.exe PowerPing.sln /p:configuration=release /p:Platform="x86"
%msbuildPath%\msbuild.exe PowerPing.sln /p:configuration=release /p:Platform="x64"

pause
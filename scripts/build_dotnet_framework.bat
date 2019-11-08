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

:: Run build command
call "%msbuild_path%" ..\PowerPing.sln /p:Configuration=Release /p:Platform="x64" /t:rebuild
call "%msbuild_path%" ..\PowerPing.sln /p:Configuration=Release /p:Platform="x86" /t:rebuild
@echo off

cd "%~dp0" 

:: Arguments check
::if [%~1]==[] (
::    echo.
::    echo ERROR: Not enough Arguments
::    echo USAGE: build.bat [C:\path\to\project]
::    exit /b 1
::)
::set projectPath=%~f1

:: Set .NET 4.6 framework path
set windows_msbuild_path="%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\"
set vs_msbuild_path="C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\amd64\msbuild.exe"

:: Run build command
cd..
::%windows_msbuild_path%\msbuild.exe PowerPing.sln /p:Configuration=Release /p:Platform="x86"
::%windows_msbuild_path%\msbuild.exe PowerPing.sln /p:Configuration=Release /p:Platform="x64"
%vs_msbuild_path% PowerPing.sln /p:Configuration=Release /p:Platform="x86"
%vs_msbuild_path% PowerPing.sln /p:Configuration=Release /p:Platform="x64"
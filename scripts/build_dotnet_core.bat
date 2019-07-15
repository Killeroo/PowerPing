@ECHO OFF

cd ..\src\PowerPing

dotnet build
dotnet publish -c Release -r win-x64
dotnet publish -c Release -r win-x86
dotnet publish -c Release -r osx-x64
dotnet publish -c Release -r linux-x64
dotnet publish -c Release -r debian-x64
dotnet publish -c Release -r ubuntu-x64
:: dotnet publish -c Release -r ubuntu.18.04-x64
:: dotnet publish -c Release -r debian.9-x64
:: dotnet publish -c Release -r osx.10.12-x64
:: dotnet publish -c Release -r osx.10.13-x64
:: dotnet publish -c Release -r win10-x64
:: dotnet publish -c Release -r win10-x86
:: dotnet publish -c Release -r win10-arm
:: dotnet publish -c Release -r win7-x64
:: dotnet publish -c Release -r win7-x86
:: dotnet publish -c Release -r win8-x64
:: dotnet publish -c Release -r win81-x64
pause
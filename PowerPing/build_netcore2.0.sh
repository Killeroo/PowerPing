#!/bin/bash

dotnet restore PowerPing_netcore2.0.csproj

dotnet build PowerPing_netcore2.0.csproj

dotnet publish PowerPing_netcore2.0.csproj -c Release -r win-x64
dotnet publish PowerPing_netcore2.0.csproj -c Release -r win-x86
dotnet publish PowerPing_netcore2.0.csproj -c Release -r osx.10.12-x64
dotnet publish PowerPing_netcore2.0.csproj -c Release -r linux-x64

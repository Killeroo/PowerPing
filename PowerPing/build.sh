#!/bin/bash

dotnet restore "PowerPing.csproj"

dotnet build "PowerPing.csproj"

dotnet publish "PowerPing.csproj" -c Release -r win-x64
dotnet publish "PowerPing.csproj" -c Release -r win-x86
dotnet publish "PowerPing.csproj" -c Release -r osx.10.12-x64
dotnet publish "PowerPing.csproj" -c Release -r linux-x64
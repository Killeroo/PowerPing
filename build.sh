#!/bin/bash

if [ $# -lt 1 ]; then
	echo "Not enough arguments: script [ dotnet OR mono ]";
	exit;
else
	if [ $1 = "dotnet" ]; then
		
		dotnet restore "src\PowerPing\PowerPing.csproj"

		dotnet build "src\PowerPing\PowerPing.csproj"

		dotnet publish "src\PowerPing\PowerPing.csproj" -c Release -r win-x64
		dotnet publish "src\PowerPing\PowerPing.csproj" -c Release -r win-x86
		dotnet publish "src\PowerPing\PowerPing.csproj" -c Release -r osx.10.12-x64
		dotnet publish "src\PowerPing\PowerPing.csproj" -c Release -r linux-x64
		
	elif [ $1 = "mono" ]; then
	
		nuget restore PowerPing.sln
		
		xbuild /p:Configuration=Release PowerPing.sln
	
	fi
fi
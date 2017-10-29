#!/bin/bash

dotnet restore PowerPing_netcore1.0.csproj

dotnet build PowerPing_netcore1.0.csproj

dotnet publish -c Release 

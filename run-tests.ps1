$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition

# build PowerPing
Write-Host "============ build project ============" -ForegroundColor Yellow
$buildScriptPath = $scriptPath + "\scripts\build_dotnet_framework.bat"
cmd.exe /C "$buildScriptPath"
if ($LASTEXITCODE -ne 0) {
    Write-Warning "Build failed, you will probably see alot of errors"
}

# Check architectures of builds are correct
Write-Host "============ build architecture check ============" -ForegroundColor Yellow
$powerping_x64_location = (split-path -parent $MyInvocation.MyCommand.Definition).ToString() + "\build\x64\PowerPing.exe"
$powerping_x86_location = (split-path -parent $MyInvocation.MyCommand.Definition).ToString() + "\build\x86\PowerPing.exe"
if ([reflection.assemblyname]::GetAssemblyName($powerping_x64_location).ProcessorArchitecture -eq "Amd64") {
    Write-Host("x64 build is correct architecture") -ForegroundColor Green
} else {
    Write-Warning("x64 build is not correct architecture. Detected: "+ [reflection.assemblyname]::GetAssemblyName($powerping_x64_location).ProcessorArchitecture)
}
if ([reflection.assemblyname]::GetAssemblyName($powerping_x86_location).ProcessorArchitecture -eq "X86") {
    Write-Host("x86 build is correct architecture") -ForegroundColor Green
} else {
    Write-Warning("x86 build is not correct architecture. Detected: "+ [reflection.assemblyname]::GetAssemblyName($powerping_x86_location).ProcessorArchitecture)
} 

# Run test scripts
& "$scriptPath\tests\test-argument-parsing.ps1"
& "$scriptPath\tests\test-lookup-functions.ps1"
& "$scriptPath\tests\test-core-functions.ps1"

#TODO: correlate errors
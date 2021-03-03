$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition

function work-out-test-result($test_name, $result_string) 
{
    Write-Host "$test_name : ".PadRight(25, " ") -NoNewline

    # Work out percentage tests passed
    $tests_performed = $result_string.Split("/")[1]
    $tests_passed = $result_string.Split("/")[0]
    if ($tests_passed -eq 0 -or $tests_performed -eq 0) {
        $percent_tests_passed = 0
    } else {
        $percent_tests_passed = [int] $(([int]$tests_passed / [int]$tests_performed) * 100)
    }

    # Draw colour coded percent 
    $percent_string = "$percent_tests_passed%".PadRight(5, " ")
    if ($percent_tests_passed -lt 50) {
        Write-Host $percent_string -NoNewline -ForegroundColor Red
    } elseif ($percent_tests_passed -lt 80) {
        Write-Host $percent_string -NoNewline -ForegroundColor Yellow
    } else {
        Write-Host $percent_string -NoNewline -ForegroundColor Green
    }
    
    # Draw some bars representing the percentage
    for ($x = 0; $x -lt $percent_tests_passed; $x += 10) {
        Write-Host "█" -NoNewline
    }
    Write-Host
}

## Build project
Write-Host "============ build project ============" -ForegroundColor Yellow
$buildScriptPath = $scriptPath + "\scripts\build_dotnet_framework.bat"
cmd.exe /C "$buildScriptPath"
if ($LASTEXITCODE -ne 0) {
    Write-Warning "Build failed, you will probably see alot of errors"
}

## Check architecture of build
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

## Run test scripts
Write-Host
$test_argument_parsing_result = & "$scriptPath\tests\test-argument-parsing.ps1"
Write-Host
$test_lookup_functions_result = & "$scriptPath\tests\test-lookup-functions.ps1" | select -Last 1
& "$scriptPath\tests\test-core-functions.ps1"

Write-Host
Write-Host "============ test results ============" -ForegroundColor Yellow
work-out-test-result "test-argument-parsing.ps1" $test_argument_parsing_result 
work-out-test-result "test-lookup-functions.ps1" $test_lookup_functions_result
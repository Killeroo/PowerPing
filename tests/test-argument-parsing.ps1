Write-Host "============ test-argument-parsing script ============" -ForegroundColor Yellow

$global:FailedTestDescriptions = @()

$script_path = (split-path -parent $MyInvocation.MyCommand.Definition)

# Executable locations
$powerping_x64_location = (split-path -parent $MyInvocation.MyCommand.Definition).ToString() + "\build\x64\PowerPing.exe"
$powerping_x86_location = (split-path -parent $MyInvocation.MyCommand.Definition).ToString() + "\build\x86\PowerPing.exe"

# Remove tests directoy from the path
$seperator = [IO.Path]::DirectorySeparatorChar
$powerping_x86_location = $powerping_x86_location.replace($seperator + "tests" + $seperator, $seperator)
$powerping_x64_location = $powerping_x64_location.replace($seperator + "tests" + $seperator, $seperator)

# Structure to store test results
$global:stats = @{
    TestsPerformed   = [uint64] 0;
    TestsPassed      = [uint64] 0;
    TestsFailed      = [uint64] 0;
}

# Function for running test
function Run-Test($description, $arguments, [int]$returnCode)
{
    Write-Host($description) -NoNewline -ForegroundColor White
    Write-Host(" (`"" + $arguments + "`") expects") -NoNewline 
    Write-Host(" [" + $returnCode + "]: ") -NoNewline -ForegroundColor Magenta
    
    Write-Host("[x64]") -NoNewline -ForegroundColor Yellow
    $stats.TestsPerformed += 1
    $Result = Start-Process -FilePath $powerping_x64_location -ArgumentList ($arguments) -PassThru -Wait
    if($Result.ExitCode -eq $returnCode) {
        Write-Host(" ==== Test passed ===== ") -NoNewline -ForegroundColor Green
        $stats.TestsPassed += 1
    } else {
        Write-Host(" --- Test Failed --- ") -NoNewline -ForegroundColor Red 
        $stats.TestsFailed += 1
    }

    Write-Host("[x86]") -NoNewline -ForegroundColor Yellow
    $stats.TestsPerformed += 1
    $Result = Start-Process -FilePath $powerping_x86_location -ArgumentList ($arguments) -PassThru -Wait
    if($Result.ExitCode -eq $returnCode) {
        Write-Host(" ==== Test passed =====") -ForegroundColor Green
        $stats.TestsPassed += 1
    } else {
        Write-Host(" --- Test Failed ---") -ForegroundColor Red 
        $stats.TestsFailed += 1
        $global:FailedTestDescriptions += $description
    }
}

Write-Host
Write-Host "Baseline tests"
Write-Host "----------------------"
Run-Test "Test noinput flag works (all tests reply on it)" "-noinput" 1
Run-Test "Test with just address" "8.8.8.8" 0
Run-Test "Run with no arguments or address" " " 0
Run-Test "Run help argument" "-help" 0
Run-Test "Test with no address" "-t" 1
Run-Test "Test with no address (but with agrument paramter" "-c 5" 1

Write-Host
Write-Host "Invalid arguments"
Write-Host "----------------------"
Run-Test "Test single bad argument" "-badargument" 1
Run-Test "Test 2 bad arguments" "-badargument -anotherbadarg" 1
Run-Test "Test bad and valid argument" "-badargument -t" 1
Run-Test "Test bad and valid argument with parameter" "-badargument -c 5" 1
Run-Test "Test bad and valid arguments with address" "-badargument -c 5 8.8.8.8" 1
Run-Test "Test bad strings with no argument prefix" "bad argument 8.8.8.8" 1

Write-Host
Write-Host "Address format"
Write-Host "----------------------"
Run-Test "Test valid IPv4 address" "8.8.8.8" 0
Run-Test "Test invalid IPv4 address" "8.8.8.8.8" 1
Run-Test "Test valid IPv4 address with port" "8.8.8.8:0" 1
Run-Test "Test valid url" "google.com" 0
Run-Test "Test invalid url" "thisisnevergoingtobe-reak.com" 1
Run-Test "Test url with path extension" "google.com/something" 1
Run-Test "Test url with file extension" "google.com/test.txt" 1
Run-Test "Test url with path and file extension" "-c 1 google.com/something/test.txt" 1
Run-Test "Test url with protocol" "-c 1 https://google.com" 1
Run-Test "Test full url" "https://en.wikipedia.org/w/index.php?search=harimau&title=Special%3ASearch&go=Go&ns0=1" 1

Write-Host
Write-Host "Test arguments"
Write-Host "----------------------"
Run-Test "Test `'count`' with parameter" "-c 1 8.8.8.8" 0
Run-Test "Test `'count`' with missing address" "-c 1" 1
Run-Test "Test `'count`' with empty parameter" "-c 8.8.8.8" 1
Run-Test "Test `'count`' with invalid positive parameter" "-c 100000000000000000000000000000000000000000000000000000000000000000000000000000000000 8.8.8.8" 1
Run-Test "Test `'count`' with invalid negative parameter" "-c -1 8.8.8.8" 1
Run-Test "Test `'limit`' with paramater" "-l 1 8.8.8.8" 0
Run-Test "Test `'limit`' with missing address" "-l 1" 1
Run-Test "Test `'limit`' with empty parameter" "-l 8.8.8.8" 1
Run-Test "Test `'limit`' with invalid positive parameter" "-l 4 8.8.8.8" 1
Run-Test "Test `'limit`' with invalid negative parameter" "-l -1 8.8.8.8" 1
Run-Test "Test `'decimals`' with parameter" "-dp 1 8.8.8.8" 0
Run-Test "Test `'decimals`' with missing address" "-dp 1" 1
Run-Test "Test `'decimals`' with empty argument" "-dp" 1
Run-Test "Test `'decimals`' with empty parameter" "-dp 8.8.8.8" 1
Run-Test "Test `'decimals`' with invalid positive parameter" "-dp 4 8.8.8.8" 1
Run-Test "Test `'decimals`' with invalid negative parameter" "-dp -1 8.8.8.8" 1
Run-Test "Test `'timing`' with parameter" "-ti 4 8.8.8.8" 0
Run-Test "Test `'timing`' with missing address" "-ti 4" 1
Run-Test "Test `'timing`' with empty argument" "-ti 8.8.8.8" 1
Run-Test "Test `'timing`' with invalid positive parameter" "-ti 8 8.8.8.8" 1
Run-Test "Test `'timing`' with invalid negative parameter" "-ti -1 8.8.8.8" 1
Run-Test "Test `'symbols`' with no arguments, and address at end" "-sym 8.8.8.8" 0
Run-Test "Test `'symbols`' with no arguments, and address at start" "8.8.8.8 -sym" 0
Run-Test "Test `'symbols`' with arguments, and address at end" "-sym 1 8.8.8.8" 0
Run-Test "Test `'symbols`' with arguments, and address at start" "8.8.8.8 -sym 1" 0
Run-Test "Test `'symbols`' with no arguments and leading argument" "-sym -c 1 8.8.8.8" 0
Run-Test "Test `'symbols`' with argument and leading argument" "-sym 1 -c 1 8.8.8.8" 0
Run-Test "Test `'symbols`' with invalid positive theme number" "-sym 1000 8.8.8.8" 0
Run-Test "Test `'symbols`' with invalid negative theme number" "-sym -1000 8.8.8.8" 0
Run-Test "Test `'symbols`' with valid theme number" "-sym 0 8.8.8.8" 0

Write-Host
Write-Host "Address location tests"
Write-Host "----------------------"
Run-Test "Test IPv4 at start" "8.8.8.8 -c 1 -ts" 0
Run-Test "Test url at start" "google.com -c 1 -ts" 0
Run-Test "Test IPv4 in middle" "-c 1 8.8.8.8 -ts" 1
Run-Test "Test url in middle" "-c 1 google.com -ts" 1
Run-Test "Test IPv4 at end" "-c 1 -ts 8.8.8.8" 0
Run-Test "Test url at end" "-c 1 -ts google.com" 0

Write-Host
Write-Host($stats.TestsPerformed.ToString() + " tests performed. " + $stats.TestsPassed.ToString() + " tests passed, " +$stats.TestsFailed.ToString() + " failed.")
if ($stats.TestsFailed -gt 0) {
    Write-Warning("One or more tests failed:");
    foreach ($test in $global:FailedTestDescriptions) {
        Write-Host("-> ") -NoNewline
        Write-Host($test) -ForegroundColor Yellow
    }
}

# Return results to caller script
return $stats.TestsPassed.ToString() + "/" + $stats.TestsPerformed.ToString()
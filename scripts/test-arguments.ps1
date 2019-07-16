$powershellLocation = (Get-Location).Path + "\PowerPing.exe"
Write-Warning $powershellLocation

function Run-Test($description, $arguments, [int]$returnCode)
{
    Write-Host($description) -NoNewline -ForegroundColor White
    Write-Host(" (`"" + $arguments + "`"): ") -NoNewline 

    $Result = Start-Process 'C:\Projects\PowerPing\src\PowerPing.Net45\bin\Debug\PowerPing.exe' -ArgumentList ('-noinput ' + $arguments) -PassThru -Wait
    if($Result.ExitCode -eq $returnCode) {
        Write-Host("==== Test passed =====") -ForegroundColor Green
    } else {
        Write-Host("--- Test Failed ---") -ForegroundColor Red 
    }

}

#$processPath = (Get-Location).Path+'C:\Projects\PowerPing\src\PowerPing.Net45\bin\Debug\PowerPing.exe'
#Sending input to running process https://stackoverflow.com/a/16100200

Run-Test "Test single bad argument" "-badargument" 1
Run-Test "Test 2 bad arguments" "-badargument -anotherbadarg" 1
Run-Test "Test valid IPv4 address" "-c 1 8.8.8.8" 0
Run-Test "Test invalid IPv4 address" "-c 1 8.8.8.8.8" 1
Run-Test "Test valid IPv4 address with port" "-c 1 8.8.8.8:0" 1
Run-Test "Test valid url" "-c 1 google.com" 0
Run-Test "Test invalid url" "-c 1 thisisnevergoingtobe-reak.com" 1
Run-Test "Test url with path extension" "-c 1 google.com/something" 1
Run-Test "Test url with file extension" "-c 1 google.com/test.txt" 1
Run-Test "Test url with path and file extension" "-c 1 google.com/something/test.txt" 1
Run-Test "Test url with protocol" "-c 1 https://google.com" 1
Run-Test "Test full url" "-c 1 https://en.wikipedia.org/w/index.php?search=harimau&title=Special%3ASearch&go=Go&ns0=1" 1

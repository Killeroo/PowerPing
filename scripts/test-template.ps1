#$processPath = (Get-Location).Path+'C:\Projects\PowerPing\src\PowerPing.Net45\bin\Debug\PowerPing.exe'
#Sending input to running process https://stackoverflow.com/a/16100200

$Result = Start-Process 'C:\Projects\PowerPing\src\PowerPing.Net45\bin\Debug\PowerPing.exe' -ArgumentList '-noinput -badargument' -PassThru -Wait

Write-Host "Test bad argument: " -NoNewline
if($Result.ExitCode -eq 0) {
	# True, last operation succeeded
    Write-Host("==== Test passed =====") -ForegroundColor DarkGreen
} else {
    Write-Host("--- Test Failed ---") -ForegroundColor DarkRed
}
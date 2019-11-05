# Load the PowerPing assembly into powershell
$lib = [Reflection.Assembly]::LoadFile("C:\Repos\PowerPing\PowerPing.exe");

Write-Host("Test GetLocalAddress(): ") -NoNewline;
$powershellLocalAddress = (ipconfig | findstr "IPv4").split(":").replace(" ", "")[1]
$powerpingLocalAddress = [PowerPing.Lookup]::GetLocalAddress();
if ($powershellLocalAddress -eq $powerpingLocalAddress) {
    Write-Host("==== Test passed =====") -ForegroundColor Green
} else {
    Write-Host("--- Test Failed ---") -ForegroundColor Red 
}
# Check the other arguments returned in ipconfig

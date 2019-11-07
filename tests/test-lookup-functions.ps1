# Load the PowerPing assembly into powershell
[Reflection.Assembly]::LoadFile("C:\Repos\PowerPing\PowerPing.exe");

# For storing test results
$global:stats = @{
    TestsPerformed   = [uint64] 0;
    TestsPassed      = [uint64] 0;
    TestsFailed      = [uint64] 0;
}

function Run-GetAddressLocationInfo-NotDetailed-Test($description, $address)
{
    Write-Host "[GetAddressLocationInfo()] [NotDetailed] " -NoNewline
    Write-Host $description -NoNewLine
    Write-Host " ("-NoNewline
    Write-Host $address -ForegroundColor Cyan -NoNewline
    Write-Host "):" -NoNewline 

    $output = [PowerPing.Lookup]::GetAddressLocationInfo($address, $false);
    $stats.TestsPerformed += 1

    if ($output.Contains("Location unavaliable")) {
        Write-Host(" --- Test Failed ---") -ForegroundColor Red 
        $stats.TestsFailed += 1
    } else {
        Write-Host(" ==== Test passed =====") -ForegroundColor Green
        $stats.TestsPassed += 1
    }
}

function Run-GetAddressLocationInfo-Detailed-Test($description, $address)
{
    Write-Host "[GetAddressLocationInfo()] [Detailed] " -NoNewline
    Write-Host $description -NoNewLine
    Write-Host " ("-NoNewline
    Write-Host $address -ForegroundColor Cyan -NoNewline
    Write-Host "):" -NoNewline 

    $output = [PowerPing.Lookup]::GetAddressLocationInfo($address, $true);
    $stats.TestsPerformed += 1

    if ($output.Contains("Location unavaliable")) {
        Write-Host(" --- Test Failed ---") -ForegroundColor Red 
        $stats.TestsFailed += 1
    } else {
        Write-Host(" ==== Test passed =====") -ForegroundColor Green
        $stats.TestsPassed += 1
    }
}

function Run-QueryDNS-Test($description, $address)
{
    Write-Host "[QueryDNS()] " -NoNewline
    Write-Host $description -NoNewLine
    Write-Host " ("-NoNewline
    Write-Host $address -ForegroundColor Cyan -NoNewline
    Write-Host "):" -NoNewline 

    $powerping_address = [PowerPing.Lookup]::QueryDNS($address, [Net.Sockets.AddressFamily]::InterNetwork);
    $our_address = $(nslookup $address | findstr Name).split(":")[1].replace(" ", "")
    $stats.TestsPerformed += 1
    Write-Warning $powerping_address

    if ($our_address -ne $powerping_address) {
        Write-Host(" --- Test Failed ---") -ForegroundColor Red 
        $stats.TestsFailed += 1
    } else {
        Write-Host(" ==== Test passed =====") -ForegroundColor Green
        $stats.TestsPassed += 1
    }
}

# Test the result of GetLocalAddress is the same as ipaddress found in powershell
Write-Host "Test GetLocalAddress(): " -NoNewline;
$powershellLocalAddress = (ipconfig | findstr "IPv4").split(":").replace(" ", "")[1]
$powerpingLocalAddress = [PowerPing.Lookup]::GetLocalAddress();
if ($powershellLocalAddress -eq $powerpingLocalAddress) {
    Write-Host("==== Test passed =====") -ForegroundColor Green
} else {
    Write-Host("--- Test Failed ---") -ForegroundColor Red 
}
# Check the other arguments returned in ipconfig


Run-GetAddressLocationInfo-NotDetailed-Test "Test valid ip address" "8.8.8.8"
Run-GetAddressLocationInfo-NotDetailed-Test "Test valid ip address" "1.1.1.1"
Run-GetAddressLocationInfo-NotDetailed-Test "Test valid URL" "www.google.com"
Run-GetAddressLocationInfo-NotDetailed-Test "Test valid URL" "en.wikipedia.org"
Run-GetAddressLocationInfo-NotDetailed-Test "Test valid URL" "www.iana.org"
Run-GetAddressLocationInfo-NotDetailed-Test "Test valid URL" "https://en.wikipedia.org"
Run-GetAddressLocationInfo-NotDetailed-Test "Test valid URL" "https://www.google.com"
Run-GetAddressLocationInfo-NotDetailed-Test "Test valid URL" "https://www.iana.org"

Run-GetAddressLocationInfo-Detailed-Test "Test valid ip address" "8.8.8.8"
Run-GetAddressLocationInfo-Detailed-Test "Test valid ip address" "1.1.1.1"
Run-GetAddressLocationInfo-Detailed-Test "Test valid URL" "www.google.com"
Run-GetAddressLocationInfo-Detailed-Test "Test valid URL" "en.wikipedia.org"
Run-GetAddressLocationInfo-Detailed-Test "Test valid URL" "www.iana.org"
Run-GetAddressLocationInfo-Detailed-Test "Test valid URL" "https://en.wikipedia.org"
Run-GetAddressLocationInfo-Detailed-Test "Test valid URL" "https://www.google.com"
Run-GetAddressLocationInfo-Detailed-Test "Test valid URL" "https://www.iana.org"

Run-QueryDNS-Test "test" "google.com"
Run-QueryDNS-Test "test" "8.8.8.8"

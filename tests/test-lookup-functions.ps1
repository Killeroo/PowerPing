Write-Host "============ test-lookup-functions script ============" -ForegroundColor Yellow

# Location of script
$script_path = (split-path -parent $MyInvocation.MyCommand.Definition)

# Locate x64 assembly and load into powershell
# TODO: Test x86 arch as well?
$seperator = [IO.Path]::DirectorySeparatorChar
$powerping_x64_location = (split-path -parent $MyInvocation.MyCommand.Definition).ToString() + "\build\x64\PowerPing.exe"
$powerping_x64_location = $powerping_x64_location.replace($seperator + "tests" + $seperator, $seperator)
[Reflection.Assembly]::LoadFile($powerping_x64_location)

# For storing test results
$global:stats = @{
    TestsPerformed   = [uint64] 0;
    TestsPassed      = [uint64] 0;
    TestsFailed      = [uint64] 0;
}

function Run-GetLocalAddress-Test($description)
{
    Write-Host "[GetLocalAddress()] " -NoNewline
    Write-Host $description -NoNewLine
    Write-Host ": " -NoNewline
    $powerping_local_address = $powerpingLocalAddress = [PowerPing.Lookup]::GetLocalAddress()
    $our_addresses = Get-NetIPAddress
    $match = $false
    $stats.TestsPerformed += 1

    # Loop through results in powershell
    foreach ($address in $our_addresses) {
        if ($powerping_local_address -eq $address.IPAddress) {
            $match = $true
            break
        }
    }

    # You know in alot of ways powershell accesses the same api as c# so how much of a test really is this
    if ($match -eq $true) {
        Write-Host "($powerping_local_address)" -BackgroundColor Green -NoNewline
        Write-Host " ==== Test passed =====" -ForegroundColor Green
        Write-Warning "This could be a virtual adapter address, please check its origin."
    } else {
        $stats.TestsFailed += 1
        Write-Host $powerping_local_address -BackgroundColor Red -NoNewline
        Write-Host "--- Test Failed ---" -ForegroundColor Red 
    }
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

    # See if powerping's returned address is in our list of resolved addresses
    $stats.TestsPerformed += 1
    $match = $false
    $powerping_address = [PowerPing.Lookup]::QueryDNS($address, [Net.Sockets.AddressFamily]::InterNetwork);
    $our_addresses = Resolve-DnsName $address
    Foreach ($addr in $our_addresses) {
        if ($addr.IPAddress -eq $powerping_address) {
            $match = $true
        }
    }

    if ($match -ne $true) {
        Write-Host(" --- Test Failed ---") -ForegroundColor Red 
        $stats.TestsFailed += 1
    } else {
        Write-Host(" ==== Test passed =====") -ForegroundColor Green
        $stats.TestsPassed += 1
    }
}

function Run-QueryHost-Test($description, $address)
{
    Write-Host "[QueryHost()] " -NoNewline
    Write-Host $description -NoNewline
    Write-Host " ("-NoNewline
    Write-Host $address -ForegroundColor Cyan -NoNewline
    Write-Host "):" -NoNewline 

    $stats.TestsPerformed += 1
    $our_hostname = Resolve-DnsName $address
    $powerping_hostname = [PowerPing.Lookup]::QueryHost($address)
    
    if ($powerping_hostname -eq $our_hostname.NameHost) {
        Write-Host(" ==== Test passed =====") -ForegroundColor Green
        $stats.TestsPassed += 1
    } else {
        Write-Host(" --- Test Failed ---") -ForegroundColor Red 
        $stats.TestsFailed += 1
    }
}

## Local address finding 
Run-GetLocalAddress-Test "Check local address matches powershell's local address"

## Address location functions
Run-GetAddressLocationInfo-NotDetailed-Test "Test valid ip address" "8.8.8.8"
Run-GetAddressLocationInfo-NotDetailed-Test "Test valid ip address" "1.1.1.1"
Run-GetAddressLocationInfo-NotDetailed-Test "Test valid URL" "www.google.com"
Run-GetAddressLocationInfo-NotDetailed-Test "Test valid URL" "en.wikipedia.org"
Run-GetAddressLocationInfo-NotDetailed-Test "Test valid URL" "www.iana.org"
Run-GetAddressLocationInfo-NotDetailed-Test "Test valid URL" "https://en.wikipedia.org"
Run-GetAddressLocationInfo-NotDetailed-Test "Test valid URL" "https://www.google.com"
Run-GetAddressLocationInfo-NotDetailed-Test "Test valid URL" "https://www.iana.org"
Run-GetAddressLocationInfo-NotDetailed-Test "Test full URL" "https://en.wikipedia.org/wiki/Wipeout_(video_game)"
Run-GetAddressLocationInfo-NotDetailed-Test "Test full URL" "https://www.iana.org/domains/idn-tables"

Run-GetAddressLocationInfo-Detailed-Test "Test valid ip address" "8.8.8.8"
Run-GetAddressLocationInfo-Detailed-Test "Test valid ip address" "1.1.1.1"
Run-GetAddressLocationInfo-Detailed-Test "Test valid URL" "www.google.com"
Run-GetAddressLocationInfo-Detailed-Test "Test valid URL" "en.wikipedia.org"
Run-GetAddressLocationInfo-Detailed-Test "Test valid URL" "www.iana.org"
Run-GetAddressLocationInfo-Detailed-Test "Test valid URL" "https://en.wikipedia.org"
Run-GetAddressLocationInfo-Detailed-Test "Test valid URL" "https://www.google.com"
Run-GetAddressLocationInfo-Detailed-Test "Test valid URL" "https://www.iana.org"
Run-GetAddressLocationInfo-Detailed-Test "Test full URL" "https://en.wikipedia.org/wiki/Wipeout_(video_game)"
Run-GetAddressLocationInfo-Detailed-Test "Test full URL" "https://www.iana.org/domains/idn-tables"

## DNS lookup
Run-QueryDNS-Test "Look up address for URL" "google.com"
Run-QueryDNS-Test "Look up address for URL" "www.iana.org"
Run-QueryDNS-Test "Look up address for URL" "en.wikipedia.org"
Run-QueryDNS-Test "Look up address for URL/DNS" "dns.msftncsi.com"
Run-QueryDNS-Test "Look up address for IPv4 address (this might fail as powerping does not query for ip addresses)" "8.8.8.8"
Run-QueryDNS-Test "Look up address for IPv4 address (this might fail as powerping does not query for ip addresses)" "1.1.1.1"

## Reverse DNS lookup
Run-QueryHost-Test "Test valid ip address" "8.8.8.8"
Run-QueryHost-Test "Test valid ip address" "8.8.4.4"
Run-QueryHost-Test "Test valid ip address" "1.1.1.1"
Run-QueryHost-Test "Test valid ip address" "131.107.255.255"
#Write-Host "Going to test a load of load addresses..." -BackgroundColor Yellow
#$our_addresses = Get-NetIPAddress
#Get-NetIPAddress | fl
#return
#foreach ($address in $our_addresses) {
#    #Run-QueryHost-Test "Test local address" $address.IPAddress
#}
#Run-QueryHost-Test "Test local address" "192.168.1.5"

Write-Host
Write-Host($stats.TestsPerformed.ToString() + " tests performed. " + $stats.TestsPassed.ToString() + " tests passed, " +$stats.TestsFailed.ToString() + " failed.")
if ($stats.TestsFailed -gt 0) {
    Write-Warning("One or more tests failed.");
}

# Return results to caller script
return $stats.TestsPassed.ToString() + "/" + $stats.TestsPerformed.ToString()
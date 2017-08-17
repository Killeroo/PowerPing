# PowerPing - Advanced Windows Ping 

[![version](https://img.shields.io/badge/version-1.1.8-green.svg)]() ![maintained](https://img.shields.io/maintenance/yes/2017.svg) ![](http://img.badgesize.io/Killeroo/PowerPing/master/build/PowerPing.exe.svg)

Small improved command line ICMP ping program lovingly inspired by windows and unix, written in C#.

Download it here: [[Current Build]](https://github.com/Killeroo/PowerPing/tree/master/build) [[Github]](https://github.com/Killeroo/PowerPing/releases) [[Softpedia]](http://www.softpedia.com/progDownload/PowerPing-Download-255701.html)
***
![](https://github.com/killeroo/PowerPing/Screenshots/readme_screenshot.png "PowerPing in action")

## Usage: 
     PowerPing [--?] | [--li] | [--whoami] | [--loc] | [--g] | [--cg] | [--fl] | 
               [--t] [--c count] [--w timeout] [--m message] [--i TTL] [--in interval] 
               [--pt type] [--pc code] [--dm] [--4] [--sh] [--nc] [--ti timing] target_name

## Arguments:
     --?             Displays this help message
     --t             Ping the target until stopped (Control-C to stop)
     --c count       Number of pings to send
     --w timeout     Time to wait for reply (in milliseconds)
     --m message     Ping packet message
     --i ttl         Time To Live
     --in interval   Interval between each ping (in milliseconds)
     --pt type       Use custom ICMP type
     --pc code       use custom ICMP code value
     --dm            Display ICMP messages
     --4             Force using IPv4
     --sh            Show less detailed replies
     --nc            No colour
     --ti timing     Timing level:
                     0 - Paranoid    4 - Nimble
                     1 - Sneaky      5 - Speedy
                     2 - Quiet       6 - Insane
                     3 - Polite
     
     --li            Listen for ICMP packets
     --fl            Send high volume of ICMP packets to address
     --g             Graph view
     --cg            Compact graph view
     --loc           Location info for an address
     --whoami        Location info for current host

## Examples:
     powerping 8.8.8.8                    -     Send ping to google DNS with default values (3000ms timeout, 5 pings)
     powerping github.com --w 500 --t     -     Send pings indefinitely to github.com with a 500ms timeout
     
     powerping 127.0.0.1 --m Meow         -     Send ping with packet message "Meow" to loopback address
     powerping 127.0.0.1 --pt 3 --pc 2    -     Send ping with ICMP type 3 (dest unreachable) and code 2
     
	 powerping 8.8.8.8 /c 5 -w 500 --sh   -     Different argument switches (/, - or --) can be used in any combination
	 powerping google.com /ti Paranoid    -     Sends using the 'Paranoid' timing option
	 powerping google.com /ti 1           -     Same as above
     
## Features

- [x] Colour coded response times
- [x] Displays type and code of each ICMP packets
- [x] Capture all ICMP communications for a computer
- [x] Customisable ping payloads
- [x] Detailed graph and statistical views
- [x] IP location querying and whoami 
- [x] Send pings with custom types and size
- [x] Ping flooding
- [ ] Local network scanning and host discovery
- [ ] Trace route functionality
- [ ] Full IPv6 support

## Screenshots

![](https://github.com/killeroo/PowerPing/Screenshots/readme_screenshot2.png "Powerping Graph view")
![](https://github.com/killeroo/PowerPing/Screenshots/readme_screenshot3.png "Powerping Listening")
![](https://github.com/killeroo/PowerPing/Screenshots/readme_screenshot4.png "Location functions") ![alt text](Screenshots/readme_screenshot5.png "PowerPing stress testing")

### Note: 
**Requires _Elevated Rights (Admininstrator)_ to Run**

*Written by Matthew Carney [matthewcarney64@gmail.com] =^-^=*

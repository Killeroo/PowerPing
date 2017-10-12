# PowerPing - Advanced Windows Ping 

[![version](https://img.shields.io/badge/version-1.1.9-brightgreen.svg)]() ![maintained](https://img.shields.io/maintenance/yes/2017.svg) ![](http://img.badgesize.io/Killeroo/PowerPing/master/build/PowerPing.exe.svg)

Small improved command line ICMP ping program lovingly inspired by windows and unix, written in C#.

Download it here: [[Current Build]](https://github.com/Killeroo/PowerPing/tree/master/build) [[Github]](https://github.com/Killeroo/PowerPing/releases) [[Softpedia]](http://www.softpedia.com/progDownload/PowerPing-Download-255701.html)
***
![alt text](Screenshots/readme_screenshot.png "PowerPing in action")

## Usage: 
     PowerPing [--?] | [--li] | [--whoami] | [--loc] | [--g] | [--cg] | [--sc] | [--fl] | 
               [--t] [--c count] [--w timeout] [--m message] [--i TTL] [--in interval] 
               [--pt type] [--pc code] [--dm] [--4] [--sh] [--nc] [--ts] [--ti timing] target_name

## Features

- ICMP Packet customisation
- Coloured output
- Customizable display
- Ping flooding
- ICMP packet capture
- Network discovery
- Graph view
- Location lookup

## Arguments:
     --help       [--?]            Displays this help message
     --examples   [--ex]           Shows example usage
     --infinite   [--t]            Ping the target until stopped (Control-C to stop)
     --displaymsg [--dm]           Display ICMP messages
     --shorthand  [--sh]           Show less detailed replies
     --nocolor    [--nc]           No colour
     --noinput    [--ni]           Require no user input
     --timestamp  [--ts]           Display timestamp
     --count      [--c]   number   Number of pings to send
     --timeout    [--w]   number   Time to wait for reply (in milliseconds)
     --ttl        [--i]   number   Time To Live
     --interval   [--in]  number   Interval between each ping (in milliseconds)
     --type       [--pt]  number   Use custom ICMP type
     --code       [--pc]  number   Use custom ICMP code value
     --message    [--m]   message  Ping packet message
     --timing     [--ti]  timing   Timing levels:
                                         0 - Paranoid    4 - Nimble
                                         1 - Sneaky      5 - Speedy
                                         2 - Quiet       6 - Insane
                                         3 - Polite
     
     --scan       [--sc]  address  Network scanning, specify range "127.0.0.1-55"
     --listen     [--li]  address  Listen for ICMP packets
     --flood      [--fl]  address  Send high volume of pings to address
     --graph      [--g]   address  Graph view
     --compact    [--cg]  address  Compact graph view
     --location   [--loc] address  Location info for an address
     --whoami                      Location info for current host

## Examples:
     powerping 8.8.8.8                    -     Send ping to google DNS with default values (3000ms timeout, 5 pings)
     powerping github.com --w 500 --t     -     Send pings indefinitely to github.com with a 500ms timeout
     
     powerping 127.0.0.1 --m Meow         -     Send ping with packet message "Meow" to loopback address
     powerping 127.0.0.1 --pt 3 --pc 2    -     Send ping with ICMP type 3 (dest unreachable) and code 2
     
     powerping 8.8.8.8 /c 5 -w 500 --sh   -     Different argument switches (/, - or --) can be used in any combination
     powerping google.com /ti Paranoid    -     Sends using the 'Paranoid' timing option
     powerping google.com /ti 1           -     Same as above
     
## Details

- [x] Colour coded response times
- [x] Displays type and code of each ICMP packets
- [x] Capture all ICMP communications for a computer
- [x] Customisable ping payloads
- [x] Detailed graph and statistical views
- [x] IP location querying and whoami 
- [x] Send pings with custom types and size
- [x] Ping flooding
- [x] Local network scanning and host discovery
- [ ] Trace route functionality
- [ ] Full IPv6 support

## Screenshots

![alt text](Screenshots/readme_screenshot2.png "Powerping Graph view")
![alt text](Screenshots/readme_screenshot3.png "Powerping Listening")
![alt text](Screenshots/readme_screenshot4.png "Location functions") ![alt text](Screenshots/readme_screenshot5.png "PowerPing stress testing")

### Note: 
**Requires _Elevated Rights (Admininstrator)_ to Run**

*Written by Matthew Carney [matthewcarney64@gmail.com] =^-^=*

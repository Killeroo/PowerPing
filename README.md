# PowerPing - Advanced Windows Ping 

Small improved command line ICMP ping program lovingly inspired by windows and unix, written in C#.
***
![alt text](Screenshots/readme_screenshot.png "PowerPing in action")

## Usage: 
     PowerPing [--?] | [--li] | [--whoami] | [--loc address] | [--g address] |
               [--cg address] | [--fl address] | [--t] [--c count] [--w timeout] 
               [--m message] [--i TTL] [--in interval] [--pt type] [--pc code] 
               [--dm] [--4] [--sh] [--nc] target_name

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
     
     --li            Listen for ICMP packets
     --fl address    Send high volume of ICMP packets to address
     --g address     Graph view
     --cg address    Compact graph view
     --loc address   Location info for an address
     --whoami        Location info for current host

## Examples:
     powerping 8.8.8.8                    -     Send ping to google DNS with default values (3000ms timeout, 5 pings)
     powerping github.com --w 500 --t     -     Send pings indefinitely to github.com with a 500ms timeout
     powerping 127.0.0.1 --m Meow         -     Send ping with packet message "Meow" to loopback address
     powerping 127.0.0.1 --pt 3 --pc 2    -     Send ping with ICMP type 3 (dest unreachable) and code 2
     
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

![alt text](Screenshots/readme_screenshot2.png "Powerping Graph view")
![alt text](Screenshots/readme_screenshot3.png "Powerping Listening")
![alt text](Screenshots/readme_screenshot4.png "Location functions") ![alt text](Screenshots/readme_screenshot5.png "PowerPing stress testing")

### Note: 
**Requires _Elevated Rights (Admininstrator)_ to Run**

*Written by Matthew Carney [matthewcarney64@gmail.com] =^-^=*

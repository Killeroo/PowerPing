# PowerPing - Advanced Windows Ping 

[![](https://img.shields.io/badge/stable%20version-1.4.0-brightgreen.svg)](https://github.com/Killeroo/PowerPing/releases) 

Small improved command line ICMP ping program.

![alt text](https://user-images.githubusercontent.com/9999745/74611062-8ad7e800-50f0-11ea-880c-17b7a76a0bab.png "PowerPing in action")

# Downloads
Stable releases can be downloaded [[here]](https://github.com/Killeroo/PowerPing/releases)

## Features

- Basic ping functionality
- Coloured output
- Display options
- [ICMP packet customisation](https://en.wikipedia.org/wiki/Internet_Control_Message_Protocol#Control_messages)
- [Scanning](https://en.wikipedia.org/wiki/Ping_sweep)
- [Flooding](https://en.wikipedia.org/wiki/Ping_flood)
- [ICMP packet capture (/listen)](docs/screenshots/screenshot3.png)
- [IP location lookup](docs/screenshots/screenshot4.png)
- [Whois lookup](https://en.wikipedia.org/wiki/WHOIS)
- [Graphing](docs/screenshots/screenshot2.png)

_Future features:_

- [Traceroute](https://en.wikipedia.org/wiki/Traceroute)
- [Tunnelling](https://en.wikipedia.org/wiki/ICMP_tunnel)
- [IPv6/ICMPv6](https://en.wikipedia.org/wiki/Internet_Control_Message_Protocol_version_6)

## Arguments:
    Ping Options:
        --infinite      [--t]            Ping the target until stopped (Ctrl-C to stop)
        --ipv4          [--4]            Force using IPv4
        --random        [--rng]          Generates random ICMP message
        --dontfrag      [--df]           Set 'Don't Fragment' flag
        --buffer        [--rb]  number   Sets recieve buffer size (default is 5096)
        --beep          [--b]   number   Beep on timeout (1) or on reply (2)
        --count         [--c]   number   Number of pings to send
        --timeout       [--w]   number   Time to wait for reply (in milliseconds)
        --ttl           [--i]   number   Time To Live for packet
        --interval      [--in]  number   Interval between each ping (in milliseconds)
        --type          [--pt]  number   Use custom ICMP type
        --code          [--pc]  number   Use custom ICMP code value
        --size          [--s]   number   Set size (in bytes) of packet (overwrites packet message)
        --message       [--m]   message  Ping packet message
        --timing        [--ti]  timing   Timing levels:
                                            0 - Paranoid    4 - Nimble
                                            1 - Sneaky      5 - Speedy
                                            2 - Quiet       6 - Insane
                                            3 - Polite      7 - Random
    
    Display Options:
        --shorthand     [--sh]           Show less detailed replies
        --displaymsg    [--dm]           Display ICMP message contents
        --timestamp     [--ts]           Display timestamps (add 'UTC' for Coordinated Universal Time)
        --fulltimestamp [--fts]          Display full timestamps with localised date and time
        --nocolor       [--nc]           No colour
        --symbols       [--sym]          Renders replies and timeouts as ASCII symbols (add '1' for alt theme)
        --requests      [--r]            Show request packets
        --notimeouts    [--nt]           Don't display timeout messages
        --quiet         [--q]            No output (only affects normal ping)
        --resolve       [--res]          Resolve hostname of response address from DNS
        --inputaddr     [--ia]           Show input address instead of revolved one
        --checksum      [--chk]          Display checksum of packet
        --requireinput  [--ri]           Always ask for user input upon completion 
        --limit         [--l]   number   Limits output to just replies (1), requests (2) or summary(3)
        --decimals      [--dp]  number   Num of decimal places to use (0 to 3)

    Modes:
        --scan          [--sc]  address  Network scanning, specify range "127.0.0.1-55"
                                         (listen command without address will listen on all local network adapters)
        --flood         [--fl]  address  Send high volume of pings to address
        --graph         [--g]   address  Graph view
        --location      [--loc] address  Location info for an address
        --listen        [--li]  address  Listen for ICMP packets on an address
        --listen        [--li]           Listen for ICMP packets on all local network adapters
        --whois                 address  Whois lookup for an address
        --whoami                         Location info for current host

    Others:
        --log           [--f]   path     Logs the ping output to a file 
        --help          [--?]            Displays this help message
        --version       [--v]            Shows version and build information

## Example usage:
     powerping 8.8.8.8                    -     Send ping to google DNS with default values (3000ms timeout, 5 pings)
     powerping github.com --w 500 --t     -     Send pings indefinitely to github.com with a 500ms timeout     
	 powerping --w 500 --t github.com     -     Address can also be specified at the end
     
     powerping 127.0.0.1 --m Meow         -     Send ping with packet message "Meow" to loopback address
     powerping 127.0.0.1 --pt 3 --pc 2    -     Send ping with ICMP type 3 (dest unreachable) and code 2
     
     powerping 8.8.8.8 /c 5 -w 500 --sh   -     Different argument switches (/, - or --) can be used in any combination
     powerping google.com /ti Paranoid    -     Sends using the 'Paranoid' timing option
     powerping google.com /ti 1           -     Same as above

## License

License under the MIT License:

Copyright (c) 2022 Matthew Carney <matthewcarney64@gmail.com>

### Note: 
**Requires _Elevated Rights (Administrator)_ to Run (more info [here](https://github.com/Killeroo/PowerPing/issues/110))**

## Screenshots

![alt text](https://user-images.githubusercontent.com/9999745/74611061-8a3f5180-50f0-11ea-978b-c9fe568c1f8c.png "powerping /g 8.8.8.8")
![alt text](https://user-images.githubusercontent.com/9999745/74611055-87446100-50f0-11ea-81ac-50551f948437.png "powerping /li")
![alt text](https://user-images.githubusercontent.com/9999745/74611057-88758e00-50f0-11ea-9259-7b1c8ac83e55.png "powerping /requests /random /displaymsg")
![alt text](https://user-images.githubusercontent.com/9999745/74611059-89a6bb00-50f0-11ea-9ed4-a2ec4f109dab.png "powerping /t /sym 8.8.8.8")
![alt text](https://user-images.githubusercontent.com/9999745/74611060-8a3f5180-50f0-11ea-839e-65f9cf03f020.png "powerping /t /sym 1 8.8.8.8")
![alt text](https://user-images.githubusercontent.com/9999745/74611058-890e2480-50f0-11ea-9ddb-ec79ecf9ce5b.png "powerping 8.8.8.8 /random /fts utc /displaymsg /nocolor /ti polite /t")

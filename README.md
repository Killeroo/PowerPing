# PowerPing - Advanced Windows Ping 

[![](https://img.shields.io/badge/version-1.2.1-brightgreen.svg)]() ![](https://img.shields.io/maintenance/yes/2017.svg) [![Build status](https://ci.appveyor.com/api/projects/status/fq7jnpmlejm7jych?svg=true)](https://ci.appveyor.com/project/Killeroo/powerping)

Small improved command line ICMP ping program lovingly inspired by windows and linux, written in C#.

Download it here: [[Stable Release]](https://github.com/Killeroo/PowerPing/releases) [[Current Build]](https://github.com/Killeroo/PowerPing/tree/master/build) [[Development Build]](https://github.com/Killeroo/PowerPing/blob/dev/build/PowerPing.exe)
***
![alt text](docs/screenshots/screenshot.png "PowerPing in action")

## Features

PowerPing contains the following features (with more to come...):

- [x] Basic ping functionality
- [x] Coloured output
- [x] Display options
- [x] [ICMP packet customisation](https://en.wikipedia.org/wiki/Internet_Control_Message_Protocol#Control_messages)
- [x] [Scanning](https://en.wikipedia.org/wiki/Ping_sweep)
- [x] [Flooding](https://en.wikipedia.org/wiki/Ping_flood)
- [x] [ICMP packet capture (/listen)](docs/screenshots/screenshot3.png)
- [x] [IP location lookup](docs/screenshots/screenshot4.png)
- [x] [Graphing](docs/screenshots/screenshot2.png)
- [ ] [Traceroute](https://en.wikipedia.org/wiki/Traceroute)
- [ ] [Tunnelling](https://en.wikipedia.org/wiki/ICMP_tunnel)
- [ ] [IPv6/icmpv6](https://en.wikipedia.org/wiki/Internet_Control_Message_Protocol_version_6)

## Usage: 
     PowerPing [--?] | [--li] | [--whoami] | [--loc] | [--g] | [--cg] | 
               [--fl] | [--sc] | [--t] [--c count] [--w timeout] [--dm]
               [--i TTL] [--in interval] [--pt type] [--pc code] [--b level]
	           [--4] [--short] [--nocolor] [--ts] [--ti timing] [--nt] target_name
               
## Arguments:
    Ping Options:
        --help       [--?]            Displays this help message"
        --version    [--v]            Shows version and build information
        --examples   [--ex]           Shows example usage
        --infinite   [--t]            Ping the target until stopped (Ctrl-C to stop)
        --displaymsg [--dm]           Display ICMP messages
        --ipv4       [--4]            Force using IPv4
        --random     [--rng]          Generates random ICMP message
        --beep       [--b]   number   Beep on timeout (1) or on reply (2)
        --count      [--c]   number   Number of pings to send
        --timeout    [--w]   number   Time to wait for reply (in milliseconds)
        --ttl        [--i]   number   Time To Live for packet
        --interval   [--in]  number   Interval between each ping (in milliseconds)
        --type       [--pt]  number   Use custom ICMP type
        --code       [--pc]  number   Use custom ICMP code value
        --message    [--m]   message  Ping packet message
        --timing     [--ti]  timing   Timing levels:
                                            0 - Paranoid    4 - Nimble
                                            1 - Sneaky      5 - Speedy
                                            2 - Quiet       6 - Insane
                                            3 - Polite
    
    Display Options:
        --shorthand  [--sh]           Show less detailed replies
        --timestamp  [--ts]           Display timestamp
        --nocolor    [--nc]           No colour
        --noinput    [--ni]           Require no user input
        --symbols    [--s]            Renders replies and timeouts as ASCII symbols
        --request    [--r]            Show request packets
        --notimeouts [--nt]           Don't display timeout messages
        --quiet      [--q]            No output, only shows summary upon completion or exit
        --limit      [--l]   number   Limits output to just replies (0) or requests (1)
        --decimals   [--dp]  number   Num of decimal places to use (0 to 3)

    Features:
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

## Screenshots

![alt text](docs/screenshots/screenshot12.png "Powerping's Graph view")
![alt text](docs/screenshots/screenshot3.png "Powerping Listening for ICMP activity")
![alt text](docs/screenshots/screenshot16.png "Powerping in action!")
![alt text](docs/screenshots/screenshot11.png "Powerping showing request packets and ICMP data")
![alt text](docs/screenshots/screenshot8.png "Powerping With no color")

[More screenshots](docs/screenshots/)

## Contribution

Feel free to [file issues or request features](https://github.com/Killeroo/PowerPing/issues) and submit [pull requests](https://github.com/Killeroo/PowerPing/pulls) – contributions are welcome.

## License

PowerPing is licensed under the [MIT license](LICENSE).

### Note: 
**Requires _Elevated Rights (Admininstrator)_ to Run**

*Written by Matthew Carney [matthewcarney64@gmail.com] =^-^=*

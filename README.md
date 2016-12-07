# PowerPing - Advanced Windows Ping 

Small improved ICMP ping program lovingly inspired by windows and unix, written in C#.

![alt text](Screenshots/readme_screenshot.png "PowerPing in action")

# Usage: 
     PowerPing [--t] [--c count] [--w timeout] [--m message] target_name

# Options:
     --t             Ping the target until stopped (Control-C to stop)
     --c count       Number of pings to send
     --w timeout     Time to wait for reply (in milliseconds)
     --m message     Ping packet message

# Example:
     powerping 8.8.8.8                    -     Send ping to google DNS with default values (3000ms timeout, 5 pings)
     powerping github.com --w 500 --t     -     Send pings indefinitely to github.com with a 500ms timeout
     powerping 127.0.0.1 --m Meow         -     Send ping with packet message "Meow" to loopback address
     
# Note: 
     Requires Administrator to Send Pings

Written by Matthew Carney [matthewcarney64@gmail.com] =^-^=

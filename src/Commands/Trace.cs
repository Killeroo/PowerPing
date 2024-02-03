/**************************************************************************
 * PowerPing - Advanced command line ping tool
 * Copyright (c) 2024 Matthew Carney [matthewcarney64@gmail.com]
 * https://github.com/Killeroo/PowerPing
 *************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPing 
{
    public class Trace 
    {
        // Gameplan:
        // 1. Ping object in its own thread (send thread)
        // 2. A socket setup similarly to Listen.cs (recieve thread)
        // 3. 1 ping each, 3 for timeout (option for full trace)
        // 4. Display with dns name, ip location and reply time
    }
}

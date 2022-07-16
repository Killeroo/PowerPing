/**************************************************************************
 * PowerPing - Advanced command line ping tool
 * Copyright (c) 2022 Matthew Carney [matthewcarney64@gmail.com]
 * https://github.com/Killeroo/PowerPing
 * ************************************************************************/

using System;

namespace PowerPing
{
    public struct PingError
    {
        public DateTime Timestamp;
        public string Message;
        public Exception Exception;
        public bool Fatal;
    }
}

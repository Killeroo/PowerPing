﻿/**************************************************************************
 * PowerPing - Advanced command line ping tool
 * Copyright (c) 2024 Matthew Carney [matthewcarney64@gmail.com]
 * https://github.com/Killeroo/PowerPing 
 *************************************************************************/

using System;

namespace PowerPing
{
    // Custom exceptions
    // (used in arguments parsing)
    public class ArgumentFormatException : Exception { }
    public class MissingArgumentException : Exception { }
    public class InvalidArgumentException : Exception { }
}

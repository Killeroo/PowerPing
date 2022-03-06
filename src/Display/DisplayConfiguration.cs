/**************************************************************************
 * PowerPing - Advanced command line ping tool
 * Copyright (c) 2022 Matthew Carney [matthewcarney64@gmail.com]
 * https://github.com/Killeroo/PowerPing
 *************************************************************************/

namespace PowerPing
{
    public class DisplayConfiguration
    {
        public bool Short { get; set; } = false;
        public bool NoColor { get; set; } = false;
        public bool UseSymbols { get; set; } = false;
        public bool ShowOutput { get; set; } = true;
        public bool ShowMessages { get; set; } = false;
        public bool ShowTimeStamp { get; set; } = false;
        public bool ShowtimeStampUTC { get; set; } = false;
        public bool ShowFullTimeStamp { get; set; } = false;
        public bool ShowFullTimeStampUTC { get; set; } = false;
        public bool ShowTimeouts { get; set; } = true;
        public bool ShowRequests { get; set; } = false;
        public bool ShowReplies { get; set; } = true;
        public bool ShowIntro { get; set; } = true;
        public bool ShowSummary { get; set; } = true;
        public bool ShowChecksum { get; set; } = false;
        public bool UseInputtedAddress { get; set; } = false;
        public bool UseResolvedAddress { get; set; } = false;
        public bool RequireInput { get; set; } = false;
        public int DecimalPlaces { get; set; } = 1;
    }
}
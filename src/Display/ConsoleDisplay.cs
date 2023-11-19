/**************************************************************************
 * PowerPing - Advanced command line ping tool
 * Copyright (c) 2023 Matthew Carney [matthewcarney64@gmail.com]
 * https://github.com/Killeroo/PowerPing
 *************************************************************************/

using System.Globalization;
using System.Reflection;
using System.Text;

namespace PowerPing
{
    /// <summary>
    /// Display class, responsible for displaying all console output from PowerPing
    /// (except for graph output)
    /// </summary>
    internal class ConsoleDisplay
    {
        public static DisplayConfiguration Configuration { get; set; } = new DisplayConfiguration();
        public static CancellationToken CancellationToken { get; set; }

        // Stores console cursor position, used for updating text at position
        private struct CursorPosition
        {
            public int Left;
            public int Top;

            public CursorPosition(int l, int t)
            {
                Left = l;
                Top = t;
            }

            public void SetToPosition()
            {
                Console.CursorLeft = Left;
                Console.CursorTop = Top;
            }
        };

        private struct ASCIIReplySymbols
        {
            public string LessThan100;
            public string LessThan250;
            public string LessThan500;
            public string GreaterThan500;
            public string Timeout;
        };

        private static ASCIIReplySymbols _replySymbols;

        // Packet type colours
        private static ConsoleColor[] _typeColors = new[] {
            ConsoleColor.Green, /* 0 */
            ConsoleColor.White, /* 1 */
            ConsoleColor.White, /* 2 */
            ConsoleColor.Red, /* 3 */
            ConsoleColor.Yellow, /* 4 */
            ConsoleColor.Blue, /* 5 */
            ConsoleColor.Yellow, /* 6 */
            ConsoleColor.White, /* 7 */
            ConsoleColor.Cyan, /* 8 */
            ConsoleColor.DarkCyan, /* 9 */
            ConsoleColor.Cyan, /* 10 */
            ConsoleColor.Red, /* 11 */
            ConsoleColor.Red, /* 12 */
            ConsoleColor.DarkBlue, /* 13 */
            ConsoleColor.Blue, /* 14 */
            ConsoleColor.DarkYellow, /* 15 */
            ConsoleColor.Yellow, /* 16 */
            ConsoleColor.DarkYellow, /* 17 */
            ConsoleColor.Yellow, /* 18 */
            ConsoleColor.White, /* 19 */
            ConsoleColor.White, /* 20 */
            ConsoleColor.White, /* 21 */
            ConsoleColor.White, /* 22 */
            ConsoleColor.White, /* 23 */
            ConsoleColor.White, /* 24 */
            ConsoleColor.White, /* 25 */
            ConsoleColor.White, /* 26 */
            ConsoleColor.White, /* 27 */
            ConsoleColor.White, /* 28 */
            ConsoleColor.White, /* 29 */
            ConsoleColor.Yellow, /* 30 */
            ConsoleColor.Yellow, /* 31 */
            ConsoleColor.Yellow, /* 32 */
            ConsoleColor.Cyan, /* 33 */
            ConsoleColor.Green, /* 34 */
            ConsoleColor.DarkYellow, /* 35 */
            ConsoleColor.Yellow, /* 36 */
            ConsoleColor.DarkYellow, /* 37 */
            ConsoleColor.Yellow, /* 38 */
            ConsoleColor.Yellow, /* 39 */
            ConsoleColor.Blue, /* 40 */
            ConsoleColor.Blue, /* 41 */
            ConsoleColor.White /* 41 */
        };

        #region Cursor position variables

        private static CursorPosition _sentPos = new CursorPosition(0, 0);
        private static CursorPosition _ppsPos = new CursorPosition(0, 0);
        private static CursorPosition _progBarPos = new CursorPosition(0, 0);
        private static CursorPosition _scanInfoPos = new CursorPosition(0, 0);
        private static CursorPosition _scanTimePos = new CursorPosition(0, 0);
        private static CursorPosition _perComplPos = new CursorPosition(0, 0);

        #endregion Cursor position variables

        public static void SetAsciiReplySymbolsTheme(int theme)
        {
            if (theme == 0)
            {
                _replySymbols.LessThan100 = ProgramStrings.REPLY_LT_100_MS_SYMBOL_1;
                _replySymbols.LessThan250 = ProgramStrings.REPLY_LT_250_MS_SYMBOL_1;
                _replySymbols.LessThan500 = ProgramStrings.REPLY_LT_500_MS_SYMBOL_1;
                _replySymbols.GreaterThan500 = ProgramStrings.REPLY_GT_500_MS_SYMBOL_1;
                _replySymbols.Timeout = ProgramStrings.REPLY_TIMEOUT_SYMBOL_1;
            }
            else
            {
                _replySymbols.LessThan100 = ProgramStrings.REPLY_LT_100_MS_SYMBOL_2;
                _replySymbols.LessThan250 = ProgramStrings.REPLY_LT_250_MS_SYMBOL_2;
                _replySymbols.LessThan500 = ProgramStrings.REPLY_LT_500_MS_SYMBOL_2;
                _replySymbols.GreaterThan500 = ProgramStrings.REPLY_GT_500_MS_SYMBOL_2;
                _replySymbols.Timeout = ProgramStrings.REPLY_TIMEOUT_SYMBOL_2;
            }
        }

        /// <summary>
        /// Displays current version number and build date
        /// </summary>
        public static void Version()
        {
            string assemblyVersion = Helper.GetVersionString();
            string assemblyName = Assembly.GetExecutingAssembly().GetName().Name ?? string.Empty;

            Console.WriteLine(assemblyName + " " + assemblyVersion);
        }

        /// <summary>
        /// Displays help message
        /// </summary>
        public static void Help()
        {
            // Print help message
            Console.WriteLine(Assembly.GetExecutingAssembly().GetName().Name + " " + Helper.GetVersionString());
            Console.WriteLine(ProgramStrings.HELP_MSG);
        }

        /// <summary>
        /// Display Initial ping message to screen, declaring simple info about the ping
        /// </summary>
        /// <param name="host">Resolved host address</param>
        /// <param name="ping">Ping object</param>
        public static void PingIntroMsg(PingAttributes attrs)
        {
            if (!Configuration.ShowOutput || !Configuration.ShowIntro)
            {
                return;
            }

            // TODO: I think this part is bullshit, check later
            if (Configuration.UseResolvedAddress)
            {
                try
                {
                    attrs.InputtedAddress = Helper.RunWithCancellationToken(() => Lookup.QueryHost(attrs.ResolvedAddress), CancellationToken);
                }
                catch (OperationCanceledException) { }

                if (attrs.InputtedAddress == "")
                {
                    // If reverse lookup fails just display whatever is in the address field
                    attrs.InputtedAddress = attrs.ResolvedAddress;
                }
            }

            // Construct string
            Console.WriteLine();
            Console.Write(ProgramStrings.INTRO_ADDR_TXT, attrs.InputtedAddress);
            if (!String.Equals(attrs.InputtedAddress, attrs.ResolvedAddress))
            {
                // Only show resolved address if inputted address and resolved address are different
                Console.Write("[{0}] ", attrs.ResolvedAddress);
            }

            if (!Configuration.Short)
            { // Only show extra detail when not in Configuration.Short mode
                if (attrs.ArtificalMessageSize != -1)
                {
                    // If custom packet size has been specified, show that
                    Console.Write(ProgramStrings.INTRO_MSG,
                        attrs.ArtificalMessageSize.ToString(),
                        attrs.Type,
                        attrs.Code,
                        attrs.Ttl);
                }
                else
                {
                    // Else show how big the string is in bytes
                    Console.Write(ProgramStrings.INTRO_MSG,
                        ASCIIEncoding.ASCII.GetByteCount(attrs.Message),
                        attrs.Type,
                        attrs.Code,
                        attrs.Ttl);
                }
            }

            // Print string
            Console.WriteLine(":");
        }

        /// <summary>
        /// Display initial listening message
        /// </summary>
        public static void ListenIntroMsg(string address)
        {
            Console.WriteLine(ProgramStrings.LISTEN_INTRO_MSG, address);
        }

        /// <summary>
        /// Display ICMP packet that have been sent
        /// </summary>
        public static void RequestPacket(ICMP packet, string address, int index)
        {
            if (!Configuration.ShowOutput || !Configuration.ShowRequests)
            {
                return;
            }

            // Show shortened info
            if (Configuration.Short)
            {
                Console.Write(ProgramStrings.REQUEST_MSG_SHORT, address);
            }
            else
            {
                Console.Write(ProgramStrings.REQUEST_MSG, address, index, packet.GetBytes().Length);
            }

            // Print coloured type
            PacketType(packet);
            Console.Write(ProgramStrings.REQUEST_CODE_TXT, packet.Code);

            // Display timestamp
            if (Configuration.ShowFullTimeStamp)
            {
                Console.Write(ProgramStrings.TIMESTAMP_LAYOUT, DateTime.Now.ToString(CultureInfo.CurrentCulture));
            }
            else if (Configuration.ShowFullTimeStampUTC)
            {
                Console.Write(ProgramStrings.TIMESTAMP_LAYOUT, DateTime.UtcNow.ToString(CultureInfo.CurrentCulture));
            }
            else if (Configuration.ShowTimeStamp)
            {
                Console.Write(ProgramStrings.TIMESTAMP_LAYOUT, DateTime.Now.ToString("HH:mm:ss"));
            }
            else if (Configuration.ShowTimeStampUTC)
            {
                Console.Write(ProgramStrings.TIMESTAMP_LAYOUT, DateTime.UtcNow.ToString("HH:mm:ss"));
            }

            // End line
            Console.WriteLine();
        }

        /// <summary>
        /// Display information about reply ping packet
        /// </summary>
        /// <param name="packet">Reply packet</param>
        /// <param name="address">Reply address</param>
        /// <param name="index">Sequence number</param>
        /// <param name="replyTime">Time taken before reply received in milliseconds</param>
        public static void ReplyPacket(ICMP packet, string address, int index, TimeSpan replyTime, int bytesRead)
        {
            if (!Configuration.ShowOutput)
            {
                return;
            }

            // If drawing symbols
            if (Configuration.UseSymbols)
            {
                if (packet.Type == 0x00)
                {
                    if (replyTime <= TimeSpan.FromMilliseconds(100))
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write(_replySymbols.LessThan100);
                    }
                    else if (replyTime <= TimeSpan.FromMilliseconds(250))
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write(_replySymbols.LessThan250);
                    }
                    else if (replyTime <= TimeSpan.FromMilliseconds(500))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write(_replySymbols.LessThan500);
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        Console.Write(_replySymbols.GreaterThan500);
                    }
                    ResetColor();
                }
                else
                {
                    Timeout(0);
                }
                return;
            }

            // Show shortened info
            if (Configuration.Short)
            {
                Console.Write(ProgramStrings.REPLY_MSG_SHORT, address);
            }
            else
            {
                Console.Write(ProgramStrings.REPLY_MSG, address, index, bytesRead);
            }

            // Print icmp packet type
            PacketType(packet);

            // Display ICMP message (if specified)
            if (Configuration.ShowMessages)
            {
                string messageWithoutHeader = Encoding.ASCII.GetString(packet.Message, 4, packet.Message.Length - 4);
                Console.Write(ProgramStrings.REPLY_MSG_TXT, new string(messageWithoutHeader.Where(c => !char.IsControl(c)).ToArray()));
            }

            // Print coloured time segment
            Console.Write(ProgramStrings.REPLY_TIME_TXT);
            if (!Configuration.NoColor)
            {
                if (replyTime <= TimeSpan.FromMilliseconds(100))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                }
                else if (replyTime <= TimeSpan.FromMilliseconds(500))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                }
            }
            Console.Write("{0:0." + new string('0', Configuration.DecimalPlaces) + "}ms", replyTime.TotalMilliseconds);
            ResetColor();

            // Display checksum
            if (Configuration.ShowChecksum)
            {
                Console.Write(ProgramStrings.REPLY_CHKSM_TXT, packet.Checksum);
            }

            // Display timestamp
            if (Configuration.ShowFullTimeStamp)
            {
                Console.Write(ProgramStrings.FULL_TIMESTAMP_LAYOUT,
                    DateTime.Now.ToString(CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern),
                    DateTime.Now.ToString("HH:mm:ss.fff"));
            }
            else if (Configuration.ShowFullTimeStampUTC)
            {
                Console.Write(ProgramStrings.FULL_TIMESTAMP_LAYOUT,
                    DateTime.UtcNow.ToString(CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern),
                    DateTime.UtcNow.ToString("HH:mm:ss.fff"));
            }
            else if (Configuration.ShowTimeStamp)
            {
                Console.Write(ProgramStrings.TIMESTAMP_LAYOUT, DateTime.Now.ToString("HH:mm:ss"));
            }
            else if (Configuration.ShowTimeStampUTC)
            {
                Console.Write(ProgramStrings.TIMESTAMP_LAYOUT, DateTime.UtcNow.ToString("HH:mm:ss"));
            }

            // End line
            Console.WriteLine();
        }

        /// <summary>
        /// Display information about a captured packet
        /// </summary>
        public static void CapturedPacket(string localAddress, ICMP packet, string remoteAddress, string timeReceived, int bytesRead)
        {
            // Display captured packet
            Console.BackgroundColor = packet.Type > _typeColors.Length ? ConsoleColor.Black : _typeColors[packet.Type];
            Console.ForegroundColor = ConsoleColor.Black;
            Console.WriteLine(ProgramStrings.CAPTURED_PACKET_MSG, timeReceived, bytesRead, remoteAddress, packet.Type, packet.Code, localAddress);

            // Reset console colours
            ResetColor();
        }

        /// <summary>
        /// Display results of scan
        /// </summary>
        public static void ScanProgress(int scanned, int found, int total, int pingsPerSecond, TimeSpan curTime, string range)
        {
            // Check if cursor position is already set
            if (_progBarPos.Left != 0)
            {
                // Store original cursor position
                CursorPosition originalPos = new CursorPosition(Console.CursorLeft, Console.CursorTop);
                Console.CursorVisible = false;

                // Update labels
                _scanInfoPos.SetToPosition();
                Console.WriteLine(ProgramStrings.SCAN_HOSTS_TXT, scanned, found, pingsPerSecond);
                _scanTimePos.SetToPosition();
                Console.Write("{0:hh\\:mm\\:ss}", curTime);
                _progBarPos.SetToPosition();
                double s = scanned;
                double tot = total;
                double blockPercent = (s / tot) * 30;
                Console.WriteLine(new String('=', Convert.ToInt32(blockPercent)) + ">");
                _perComplPos.SetToPosition();
                Console.WriteLine("{0}%", Math.Round((s / tot) * 100, 0));

                // Reset to original cursor position
                Console.SetCursorPosition(originalPos.Left, originalPos.Top);
            }
            else
            {
                // Setup labels
                Console.WriteLine(ProgramStrings.SCAN_RANGE_TXT, range);
                _scanInfoPos = new CursorPosition(Console.CursorLeft, Console.CursorTop);
                Console.WriteLine();
                Console.Write(" ");
                _scanTimePos = new CursorPosition(Console.CursorLeft, Console.CursorTop);
                Console.Write("00:00:00 [");
                _progBarPos = new CursorPosition(Console.CursorLeft, Console.CursorTop);
                Console.Write("                               ] ");
                _perComplPos = new CursorPosition(Console.CursorLeft, Console.CursorTop);
                Console.WriteLine();
            }
        }

        public static void ScanResults(int scanned, bool ranToEnd, List<Scan.HostInformation> hosts)
        {
            Console.CursorVisible = true;

            Console.WriteLine();
            Console.Write("Scan " + (ranToEnd ? "complete" : "aborted") + ". ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(scanned);
            Console.ResetColor();
            Console.Write(" addresses scanned. ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(hosts.Count);
            Console.ResetColor();
            Console.WriteLine(" hosts found.");
            if (hosts.Count != 0)
            {
                for (int i = 0; i < hosts.Count; i++)
                {
                    Scan.HostInformation entry = hosts[i];
                    Console.WriteLine(
                        (i == hosts.Count - 1 ? ProgramStrings.SCAN_END_CHAR : ProgramStrings.SCAN_CONNECTOR_CHAR) + ProgramStrings.SCAN_RESULT_ENTRY,
                        entry.Address,
                        entry.ResponseTime,
                        entry.HostName != "" ? entry.HostName : "UNAVAILABLE");
                }
            }
            Console.WriteLine();

            if (Configuration.RequireInput)
            {
                Helper.WaitForUserInput();
            }
        }

        /// <summary>
        /// Displays statistics for a ping object
        /// </summary>
        /// <param name="ping"> </param>
        public static void PingResults(PingAttributes attrs, PingResults results)
        {
            if (!Configuration.ShowOutput || !Configuration.ShowSummary)
            {
                return;
            }

            ResetColor();

            // Display stats
            double percent = (double)results.Lost / results.Sent;
            percent = Math.Round(percent * 100, 2);
            Console.WriteLine();
            Console.WriteLine(ProgramStrings.RESULTS_HEADER, attrs.ResolvedAddress);

            //   General: Sent [ 0 ], Received [ 0 ], Lost [ 0 ] (0% loss)
            Console.Write(ProgramStrings.RESULTS_GENERAL_TAG + ProgramStrings.RESULTS_SENT_TXT);
            if (Configuration.NoColor)
            {
                Console.Write(ProgramStrings.RESULTS_INFO_BOX, results.Sent);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write(ProgramStrings.RESULTS_INFO_BOX, results.Sent);
                ResetColor();
            }
            Console.Write(ProgramStrings.RESULTS_RECV_TXT);
            if (Configuration.NoColor)
            {
                Console.Write(ProgramStrings.RESULTS_INFO_BOX, results.Received);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(ProgramStrings.RESULTS_INFO_BOX, results.Received);
                ResetColor();
            }
            Console.Write(ProgramStrings.RESULTS_LOST_TXT);
            if (Configuration.NoColor)
            {
                Console.Write(ProgramStrings.RESULTS_INFO_BOX, results.Lost);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write(ProgramStrings.RESULTS_INFO_BOX, results.Lost);
                ResetColor();
            }
            Console.WriteLine(ProgramStrings.RESULTS_PERCENT_LOST_TXT, percent);

            //     Times: Min [ 0ms ] Max [ 0ms ] Avg [ 0ms ]
            Console.WriteLine(ProgramStrings.RESULTS_TIMES_TAG + ProgramStrings.RESULTS_TIME_TXT, results.MinTime, results.MaxTime, results.AvgTime);

            //     Types: Good [ 0 ], Errors [ 0 ], Unknown [ 0 ]
            Console.Write(ProgramStrings.RESULTS_TYPES_TAG);
            Console.Write(ProgramStrings.RESULTS_PKT_GOOD);
            if (Configuration.NoColor)
            {
                Console.Write(ProgramStrings.RESULTS_INFO_BOX, results.GoodPackets);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(ProgramStrings.RESULTS_INFO_BOX, results.GoodPackets);
                ResetColor();
            }
            Console.Write(ProgramStrings.RESULTS_PKT_ERR);
            if (Configuration.NoColor)
            {
                Console.Write(ProgramStrings.RESULTS_INFO_BOX, results.GoodPackets);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write(ProgramStrings.RESULTS_INFO_BOX, results.ErrorPackets);
                ResetColor();
            }
            Console.Write(ProgramStrings.RESULTS_PKT_UKN);
            if (Configuration.NoColor)
            {
                Console.Write(ProgramStrings.RESULTS_INFO_BOX, results.GoodPackets);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(ProgramStrings.RESULTS_INFO_BOX, results.OtherPackets);
                ResetColor();
            }

            // Started at: 0:00:00 (local time)
            Console.WriteLine(ProgramStrings.RESULTS_START_TIME_TXT, results.StartTime);

            //Runtime: hh:mm:ss.f
            Console.WriteLine(ProgramStrings.RESULTS_RUNTIME_TXT, results.TotalRunTime);
            Console.WriteLine();

            if (results.HasOverflowed)
            {
                Console.WriteLine(ProgramStrings.RESULTS_OVERFLOW_MSG);
                Console.WriteLine();
            }

            if (Configuration.RequireInput)
            {
                Helper.WaitForUserInput();
            }
        }

        /// <summary>
        /// Displays and updates results of an ICMP flood
        /// </summary>
        /// <param name="results"></param>
        public static void FloodProgress(ulong totalPings, ulong pingsPerSecond, string target)
        {
            // Check if labels have already been drawn
            if (_sentPos.Left > 0)
            {
                // Store original cursor position
                CursorPosition originalPos = new CursorPosition(Console.CursorLeft, Console.CursorTop);
                Console.CursorVisible = false;

                // Update labels
                Console.SetCursorPosition(_sentPos.Left, _sentPos.Top);
                Console.Write(totalPings);
                Console.SetCursorPosition(_ppsPos.Left, _ppsPos.Top);
                Console.Write("          "); // Blank first
                Console.SetCursorPosition(_ppsPos.Left, _ppsPos.Top);
                Console.Write(pingsPerSecond);
                // Reset to original cursor position
                Console.SetCursorPosition(originalPos.Left, originalPos.Top);
                Console.CursorVisible = true;
            }
            else
            {
                // Draw labels
                Console.WriteLine(ProgramStrings.FLOOD_INTO_TXT, target);
                Console.Write(ProgramStrings.FLOOD_SEND_TXT);
                _sentPos.Left = Console.CursorLeft;
                _sentPos.Top = Console.CursorTop;
                Console.WriteLine("0");
                Console.Write(ProgramStrings.FLOOD_PPS_TXT);
                _ppsPos.Left = Console.CursorLeft;
                _ppsPos.Top = Console.CursorTop;
                Console.WriteLine();
                Console.WriteLine(ProgramStrings.FLOOD_EXIT_TXT);
            }
        }

        public static void ListenResults(PingResults results)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Display Timeout message
        /// </summary>
        public static void Timeout(int seq)
        {
            if (!Configuration.ShowOutput || !Configuration.ShowTimeouts)
            {
                return;
            }

            // If drawing symbols
            if (Configuration.UseSymbols)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write(_replySymbols.Timeout);
                ResetColor();
                return;
            }

            if (!Configuration.NoColor)
            {
                Console.BackgroundColor = ConsoleColor.DarkRed;
                Console.ForegroundColor = ConsoleColor.Black;
            }
            Console.Write(ProgramStrings.TIMEOUT_TXT);

            // Configuration.Short hand
            if (!Configuration.Short)
            {
                Console.Write(ProgramStrings.TIMEOUT_SEQ_TXT, seq);
            }

            // Display timestamp
            if (Configuration.ShowFullTimeStamp)
            {
                Console.Write(ProgramStrings.TIMESTAMP_LAYOUT, DateTime.Now.ToString(CultureInfo.CurrentCulture));
            }
            else if (Configuration.ShowFullTimeStampUTC)
            {
                Console.Write(ProgramStrings.TIMESTAMP_LAYOUT, DateTime.UtcNow.ToString(CultureInfo.CurrentCulture));
            }
            else if (Configuration.ShowTimeStamp)
            {
                Console.Write(ProgramStrings.TIMESTAMP_LAYOUT, DateTime.Now.ToString("HH:mm:ss"));
            }
            else if (Configuration.ShowTimeStampUTC)
            {
                Console.Write(ProgramStrings.TIMESTAMP_LAYOUT, DateTime.UtcNow.ToString("HH:mm:ss"));
            }

            // Make double sure we dont get the red line bug
            ResetColor();
            Console.WriteLine();
        }

        /// <summary>
        /// Display error message
        /// </summary>
        /// <param name="errMsg">Error message to display</param>
        public static void Error(string errMsg, Exception e)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            string errorText = (e != null ? $"{errMsg} ({e.GetType().Name})" : errMsg);

            // Write error message
            Console.WriteLine(errorText);

            // Reset console colours
            ResetColor();
        }

        /// <summary>
        /// Display error message
        /// </summary>
        /// <param name="errMsg">Error message to display</param>
        public static void Error(string errMsg)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;

            // Write error message
            Console.WriteLine(errMsg);

            // Reset console colours
            ResetColor();
        }

        public static void Fatal(string errMsg, Exception e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            string errorText = (e != null ? $"{errMsg} ({e.GetType().Name})" : errMsg);

            Console.WriteLine(errorText);

            ResetColor();
        }

        /// <summary>
        /// Display a general message
        /// </summary>
        public static void Message(string msg, ConsoleColor color = ConsoleColor.DarkGray, bool newline = true)
        {
            if (color == ConsoleColor.DarkGray)
            {
                Console.ResetColor(); // Use default foreground color if gray is being used
            }

            Console.ForegroundColor = color;

            if (newline)
            {
                Console.WriteLine(msg);
            }
            else
            {
                Console.Write(msg);
            }

            ResetColor();
        }

        public static void PacketType(ICMP packet)
        {
            // Apply colour rules
            if (!Configuration.NoColor)
            {
                Console.BackgroundColor = packet.Type > _typeColors.Length ? ConsoleColor.White : _typeColors[packet.Type];
                Console.ForegroundColor = ConsoleColor.Black;
            }

            // Print packet type
            switch (packet.Type)
            {
                case 3:
                    Console.Write(packet.Code > ICMPStrings.DestinationUnreachableCodeValues.Length ? ICMPStrings.PacketTypes[packet.Type] : ICMPStrings.DestinationUnreachableCodeValues[packet.Code]);
                    break;

                case 5:
                    Console.Write(packet.Code > ICMPStrings.RedirectCodeValues.Length ? ICMPStrings.PacketTypes[packet.Type] : ICMPStrings.RedirectCodeValues[packet.Code]);
                    break;

                case 11:
                    Console.Write(packet.Code > ICMPStrings.TimeExceedCodeValues.Length ? ICMPStrings.PacketTypes[packet.Type] : ICMPStrings.TimeExceedCodeValues[packet.Code]);
                    break;

                case 12:
                    Console.Write(packet.Code > ICMPStrings.BadParameterCodeValues.Length ? ICMPStrings.PacketTypes[packet.Type] : ICMPStrings.BadParameterCodeValues[packet.Code]);
                    break;

                default:
                    Console.Write(packet.Type > ICMPStrings.PacketTypes.Length ? "[" + packet.Type + "] UNASSIGNED " : ICMPStrings.PacketTypes[packet.Type]);
                    break;
            }

            ResetColor();
        }

        public static void ResetColor()
        {
            Console.ResetColor();
        }
    }
}
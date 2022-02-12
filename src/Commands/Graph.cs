/**************************************************************************
 * PowerPing - Advanced command line ping tool
 * Copyright (c) 2022 Matthew Carney [matthewcarney64@gmail.com]
 * https://github.com/Killeroo/PowerPing
 *************************************************************************/

using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;

namespace PowerPing
{
    /// <summary>
    /// Graph class, sends pings using Ping.cs and displays on
    /// console based graph.
    /// </summary>
    class Graph
    {
        // Constants
        const string FULL_BAR_BLOCK_CHAR = "█";
        const string HALF_BAR_BLOCK_CHAR = "▄";
        const string BOTTOM_BAR_BLOCK_CHAR = "▀";

        // Properties
        public bool CompactGraph { get; set; } = false;
        public int EndCursorPosY { get; set; } = 0; // Position to move cursor to when graph exits

        // Local variable declaration
        private readonly CancellationToken m_CancellationToken;
        private readonly Ping m_Ping;
        private readonly PingAttributes m_PingAttributes = new PingAttributes();
        private readonly List<string[]> m_Columns = new List<string[]>();
        private readonly List<double> m_ResponseTimes = new List<double>();
        private bool m_IsGraphSetup = false;
        private int m_yAxisLength = 20;
        private int m_xAxisLength = 40;
        private int m_yAxisLeftPadding = 14;
        private int m_LegendLeftPadding = 16;
        private int m_StartScale = 5; // Stops graph from scaling in past its start scale
        private int m_Scale = 5;//50; // Our current yaxis graph scale 
        private double lastAvg = 0;
        private double lastRes = 0;

        // Limits refreshing display too quickly
        // NOTE: The actual display update rate may be limited by the ping interval
        RateLimiter displayUpdateLimiter = new RateLimiter(TimeSpan.FromMilliseconds(500));

        // Properties to use for normal and compact graph modes
        private int m_NormalLegendLeftPadding = 13;
        private int m_NormalYAxisLeftPadding = 11;
        private int m_NormalYAxisLength = 20;
        private int m_NormalXAxisLength = 70;
        private int m_CompactLegendLeftPadding = 2;
        private int m_CompactYAxisLeftPadding = 5;
        private int m_CompactYAxisLength = 10;
        private int m_CompactXAxisLength = 30;

        // Location of graph plotting space
        private int m_PlotStartX;
        private int m_PlotStartY;

        // Label locations
        private int m_SentLabelX, m_SentLabelY;
        private int m_RecvLabelX, m_RecvLabelY;
        private int m_FailLabelX, m_FailLabelY;
        private int m_RttLabelX, m_RttLabelY;
        private int m_TimeLabelX, m_TimeLabelY;
        private int m_AvgLabelX, m_AvgLabelY;
        private int m_PeakLabelX, m_PeakLabelY;
        private int m_yAxisStart;
        
        public Graph(string address, CancellationToken cancellationTkn)
        {
            // Setup ping attributes
            m_PingAttributes.InputtedAddress = address;
            m_PingAttributes.Continous = true;

            m_CancellationToken = cancellationTkn;
            m_Ping = new Ping(m_PingAttributes, cancellationTkn, OnPingResultsUpdateCallback);
            m_Scale = m_StartScale;

        }

        /// <summary>
        /// Draws and sets up graph when it is first run
        /// </summary>
        public void Start()
        {
            // Disable output
            Display.ShowOutput = false;

            // Hide cursor
            Console.CursorVisible = false;

            // Check graph is setup
            if (!m_IsGraphSetup) {
                Setup();
            }

            // Start pinging (initates update loop in OnPingResultsUpdateCallback)
            m_Ping.Send();

            // Show cursor
            Console.CursorVisible = true;
        }
        ///<summary>
        /// Setup graph
        /// </summary>
        private void Setup()
        {
            // Setup graph properties based on graph type
            if (CompactGraph) {
                m_yAxisLength = m_CompactYAxisLength;
                m_xAxisLength = m_CompactXAxisLength;
                m_LegendLeftPadding = m_CompactLegendLeftPadding;
                m_yAxisLeftPadding = m_CompactYAxisLeftPadding;
            } else {
                m_yAxisLength = m_NormalYAxisLength;
                m_xAxisLength = m_NormalXAxisLength;
                m_LegendLeftPadding = m_NormalLegendLeftPadding;
                m_yAxisLeftPadding = m_NormalYAxisLeftPadding;
            }

            CheckAndResizeGraph();
            DrawBackground();
            DrawYAxisLabels();

            m_IsGraphSetup = true;
        }
        /// <summary>
        /// Clear the plotting area of the graph
        /// </summary>
        private void Clear()
        {
            // save cursor location
            int cursorPositionX = Console.CursorLeft;
            int cursorPositionY = Console.CursorTop;

            // Set cursor position to start of plot
            Console.SetCursorPosition(m_PlotStartX, m_yAxisStart);//m_PlotStartY);

            string blankRow = new string(' ', m_xAxisLength);
            string bottomRow = new string('─', m_xAxisLength);

            for (int x = 0; x <= m_yAxisLength; x++) { //21; x++) {
                // Draw black spaces
                Console.Write(blankRow);
                Console.CursorLeft = m_PlotStartX;
                Console.CursorTop = m_PlotStartY - x;
            }

            // Draw bottom row
            Console.CursorTop = m_PlotStartY;
            Console.Write(bottomRow);

            // Reset cursor to starting position
            Console.SetCursorPosition(cursorPositionX, cursorPositionY);
        }

        // This callback will run after each ping iteration
        // This is technically our main update loop for the graph
        void OnPingResultsUpdateCallback(PingResults r)
        {
            // Make sure we're not updating the display too frequently
            if (!displayUpdateLimiter.RequestRun()) {
                return;
            }

            int scalePrevious = m_Scale;

            // Reset position
            Console.CursorTop = m_PlotStartY;
            Console.CursorLeft = m_PlotStartX;

            // Update labels
            UpdateLegend(r);

            // Get results from ping and add to graph
            AddResponseToGraph(r.CurrTime);

            // Draw graph columns
            DrawColumns();

            // Only draw the y axis labels if the scale has changed
            if (scalePrevious != m_Scale) {
                DrawYAxisLabels();
            }

            Console.CursorTop = EndCursorPosY;

        }

        /// <summary>
        /// Checks if the graph Y axis need to be scaled down or up
        /// Scaling is linear, so scale doubles or halves each time.
        /// I couldn't be asked to try and hack exponential scaling here 
        /// althrough it would probably work better
        /// </summary>
        private void CheckGraphScale(double newResponseTime)
        {
            int newTime = Convert.ToInt32(newResponseTime);
            int maxTime = m_Scale * m_yAxisLength;

            // Did we exceed our current scale?
            if (newTime > maxTime) {
               m_Scale *= 2; // Expand!

                // Recurse back into ourself to check the scale again
                // Just in case we have to increase the scale all at once
                // we want to do it now, instead we will have a jumpy 
                // rescaling look over the new few bars 
               CheckGraphScale(newResponseTime);
            }

            // Check if any value on the graph is larger than half our current
            // max y axis value
            bool scaleDown = true;
            foreach (double responseTime in m_ResponseTimes) {
                int time = Convert.ToInt32(responseTime);

                if (time > maxTime / 2) {
                    scaleDown = false;
                }
            }

            // If so scale down
            if (scaleDown && m_Scale != m_StartScale) {
                m_Scale /= 2;
            }
        }
        /// <summary>
        /// Checks the current console window size and adjusts the graph if
        /// required.
        /// </summary>
        private void CheckAndResizeGraph()
        {
            if (Console.WindowWidth < m_xAxisLength + m_yAxisLeftPadding) {
                m_xAxisLength = Math.Max(Console.WindowWidth - m_yAxisLeftPadding - 5, 35);
            }
            if (Console.WindowHeight < m_yAxisLength) {
                m_yAxisLength = Math.Max(Console.WindowHeight - 5, 10);
            }
        }

        /// <summary>
        /// Add a column to the graph list
        /// </summary>
        private void AddResponseToGraph(double responseTime)
        {
            m_ResponseTimes.Add(responseTime);

            // If number of columns exceeds x Axis length
            if (m_ResponseTimes.Count >= m_xAxisLength) {
                // Remove first element
                m_ResponseTimes.RemoveAt(0);
            }
        }

        /// <summary>
        /// Draw all graph coloums/bars
        /// </summary>
        private void DrawColumns()
        {
            // Clear columns space before drawing
            Clear();
            
            for (int x = 0; x < m_ResponseTimes.Count; x++) {
                
                // This causes us to draw a continous lower line of red when we are continously timing out
                // Instead of always drawing big red lines, we draw them at either end of the continous zone
                // I think it will just look nicer, it will cause slightly hackier code but oh well
                bool drawTimeoutSegment = false;

                // Column type
                bool timeout = false;
                bool current = false;
                
                if (x == m_ResponseTimes.Count - 1) {
                    current = true;
                }
                if (m_ResponseTimes[x] == 0) {
                    timeout = true;
                }

                // So to get a timeout segment we peak at the elements ahead and behind to check they are timeouts
                // if not we will just draw a normal line at the end of the timeout
                // Horrible hacky inline logic to make sure we don't outofbounds while checking behind and head in the array
                if (m_ResponseTimes[x] == 0 
                    && (x != 0 ? m_ResponseTimes[x - 1] == 0 : false)
                    && ((x < m_ResponseTimes.Count -1 ? m_ResponseTimes[x + 1] == 0 : false) || x == m_ResponseTimes.Count - 1)) 
                {
                    drawTimeoutSegment = true;
                }

                DrawSingleColumn(CreateColumn(m_ResponseTimes[x]), current, timeout, drawTimeoutSegment);

                Console.CursorLeft++; 
            }

            // Reset colour after
            Console.ForegroundColor = ConsoleColor.Gray;
        }
        /// <summary>
        /// Generate bar for graph
        /// </summary>
        /// <param name="time">Reply time of packet to plot</param>
        private string[] CreateColumn(double replyTime)
        {
            string[] bar;
            int count = 0;
            int time = Convert.ToInt32(replyTime);

            // Work out bar length
            for (int x = 0; x < time; x = x + m_Scale) {
                count++;
            }

            // Scale up or down graph as needed
            CheckGraphScale(replyTime);

            if (time > m_Scale * m_yAxisLength) {
                // If reply time over graph Y range draw max size column
                count = 10;
            }
            else if (time == 0) {
                // If no reply dont draw column
                string[] timeoutBar = new string[m_yAxisLength + 1];
                for (int x = 0; x < timeoutBar.Length; x++) {
                    timeoutBar[x] = "|";
                }
                timeoutBar[0] = "┴";
                return timeoutBar;
            }

            // Create array to store bar
            bar = new string[count + 1];

            // Fill bar
            for (int x = 0; x < count; x = x + 1) { // count + 1
                bar[x] = FULL_BAR_BLOCK_CHAR;
            }

            // Replace lowest bar segment
            bar[0] = "▀";

            // Replace the last segment 
            // https://stackoverflow.com/a/2705553
            int nearestMultiple = (int)Math.Round((time / (double)m_Scale), MidpointRounding.AwayFromZero) * m_Scale;

            if (nearestMultiple - time < 0) {
                bar[bar.Length - 1] = " ";
            }
            else {
                bar[bar.Length - 1] = HALF_BAR_BLOCK_CHAR;
            }

            return bar;

        }
        /// <summary>
        /// Draw graph background
        /// </summary>
        private void DrawBackground()
        {
            // Draw title
            Console.WriteLine();

            // Save position for later
            m_yAxisStart = Console.CursorTop;

            // Draw graph y axis 
            // Hack: this is code that I copied from the y axis generation function
            int maxLines = m_yAxisLength;
            int maxYValue = maxLines * m_Scale;
            int currValue = maxYValue;
            for (int x = maxLines; x != 0; x--)
            {
                // write current value with m_LegendLeftPadding (slightly less every 2 lines)
                if (x % 2 == 0) {
                    Console.Write(currValue.ToString().PadLeft(m_yAxisLeftPadding) + " ");
                } else {
                    Console.Write(new string(' ', m_yAxisLeftPadding + 1));
                }

                // Add indentation every 2 lines 
                if (x % 2 == 0) {
                    Console.Write("─");
                } else {
                    Console.Write(" ");
                }

                if (x == maxLines) {
                    Console.WriteLine("┐");
                } else {
                    Console.WriteLine("┤");
                }

                currValue -= m_Scale;
            }

            // Draw X axis of graph
            Console.Write(new string(' ', m_yAxisLeftPadding) + "0 └");

            // Save start of graph plotting area
            m_PlotStartX = Console.CursorLeft;
            m_PlotStartY = Console.CursorTop;
            Console.WriteLine(new string('─', m_xAxisLength));
            Console.WriteLine();

            string leftPadding = new string(' ', m_LegendLeftPadding);

            // Draw info (and get location info for each label)
            Console.WriteLine(leftPadding + " Ping Statistics:");
            Console.WriteLine(leftPadding + "-----------------------------------");
            Console.WriteLine(leftPadding + " Destination [ {0} ]", m_PingAttributes.InputtedAddress);

            Console.Write(leftPadding + "    Sent: ");
            m_SentLabelX = Console.CursorLeft;
            m_SentLabelY = Console.CursorTop;

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("           Received: ");
            m_RecvLabelX = Console.CursorLeft;
            m_RecvLabelY = Console.CursorTop;
            Console.ForegroundColor = ConsoleColor.Gray;

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("           Average: ");
            m_AvgLabelX = Console.CursorLeft;
            m_AvgLabelY = Console.CursorTop;
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Gray;

            Console.Write(leftPadding + " Current: ");
            m_RttLabelX = Console.CursorLeft;
            m_RttLabelY = Console.CursorTop;

            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("               Lost: ");
            m_FailLabelX = Console.CursorLeft;
            m_FailLabelY = Console.CursorTop;
            Console.ForegroundColor = ConsoleColor.Gray;

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("              Peak: ");
            m_PeakLabelX = Console.CursorLeft;
            m_PeakLabelY = Console.CursorTop;
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine();

            Console.Write(leftPadding + " Time Elapsed: ");
            m_TimeLabelX = Console.CursorLeft;
            m_TimeLabelY = Console.CursorTop;
            Console.WriteLine();

            EndCursorPosY = Console.CursorTop;
        }
        /// <summary>
        /// Draw graph bar
        /// </summary>
        private void DrawSingleColumn(string[] column, bool current, bool timeout, bool timeoutSegment)
        {
            // save cursor location
            int startingCursorPositionX = Console.CursorLeft;
            int startingCursorPositionY = Console.CursorTop;

            if (timeoutSegment) {
                Console.Write("─");

                Console.CursorLeft--;
                return;
            }

            bool inverting = false;
            foreach (string segment in column)
            {
                // Determine colour of segment
                if (timeout) {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                } else if (current) {
                    Console.ForegroundColor = ConsoleColor.Green;
                } else if (inverting) {
                    Console.ForegroundColor = ConsoleColor.Gray;
                    inverting = false;
                } else {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    inverting = true;
                }

                Console.Write(segment);

                // in an attempt to save time by not always accessing 
                // Console.Cursor positions too many times (doesn't really
                // make that much of a difference)
                int cursorPositionLeft = Console.CursorLeft;
                int cursorPositionTop = Console.CursorTop;

                // Stop over drawing at the top of the graph
                if (cursorPositionTop == m_yAxisStart) {
                    break;
                }

                if (cursorPositionTop != 0) {
                    cursorPositionTop--;
                    cursorPositionLeft--;

                    Console.SetCursorPosition(cursorPositionLeft, cursorPositionTop);
                }
            }

            // Reset cursor to starting position
            Console.SetCursorPosition(startingCursorPositionX, startingCursorPositionY);
        }
        /// <summary>
        /// Draws the labels for the y axis based on our current m_Scale
        /// </summary>
        public void DrawYAxisLabels()
        {
            int maxLines = m_yAxisLength;
            int maxYValue = maxLines * m_Scale;

            int topStart = Console.CursorTop;
            int leftStart = Console.CursorLeft;

            // Setup cursor position for drawing labels
            Console.CursorTop = m_yAxisStart;
            Console.CursorLeft = 0;

            int currValue = maxYValue;
            for (int x = maxLines; x != 0; x--)
            {
                // write current value with padding (slightly less every 2 lines)
                if (x % 2 == 0) {
                    Console.Write(currValue.ToString().PadLeft(m_yAxisLeftPadding) + " ");
                } else {
                    Console.Write(new string(' ', m_yAxisLeftPadding + 1));
                }

                // Add indentation every 2 lines 
                if (x % 2 == 0) { 
                    Console.Write("─");
                } else {
                    Console.Write(" ");
                }
                
                if (x == maxLines) {
                    Console.WriteLine("┐");
                } else {
                    Console.WriteLine("┤");
                }

                currValue -= m_Scale;
            }

            // Draw name of y axis
            // (Don't bother in compact mode, to save space)
            if (!CompactGraph) {
                Console.CursorTop = m_yAxisStart + maxLines / 2;
                Console.CursorLeft = 1;
                Console.WriteLine("Round");
                Console.CursorLeft = 2;
                Console.WriteLine("Trip");
                Console.CursorLeft = 2;
                Console.WriteLine("Time");
                Console.CursorLeft = 2;
                Console.WriteLine("(MS)");
            }

            // Reset cursor position
            Console.CursorLeft = leftStart;
            Console.CursorTop = topStart;
        }
        /// <summary>
        /// Update graph legend text labels
        /// </summary>
        /// <param name="results"></param>
        private void UpdateLegend(PingResults results)
        {
            // save cursor location
            int cursorPositionX = Console.CursorLeft;
            int cursorPositionY = Console.CursorTop;

            string blankLabel = new string(' ', 8);

            // Update sent label
            Console.SetCursorPosition(m_SentLabelX, m_SentLabelY);
            // Clear label first
            Console.Write(blankLabel);
            // Move cursor back
            Console.CursorLeft = Console.CursorLeft - 8;
            // Write label value
            Console.Write(results.Sent);

            // Update recieve label
            Console.SetCursorPosition(m_RecvLabelX, m_RecvLabelY);
            Console.Write(blankLabel);
            Console.CursorLeft = Console.CursorLeft - 8;
            Console.Write(results.Received);

            // Update average label
            Console.SetCursorPosition(m_AvgLabelX, m_AvgLabelY);
            Console.Write(new string(' ', 15));
            Console.CursorLeft = Console.CursorLeft - 15;
            double r = Math.Round(results.AvgTime, 1);
            if (lastAvg < r) {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.Write("+");
                if (results.CurrTime - lastRes > 20)
                    Console.Write("+");
            }
            else if (lastAvg > r) {
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.Write("-");
                if (lastRes - results.CurrTime > 20)
                    Console.Write("-");
            }
            else {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write("~");
            }
            lastAvg = r;
            lastRes = results.CurrTime;
            Console.ResetColor();
            Console.Write("{0:0.0}ms", results.AvgTime);

            // Update fail label
            Console.SetCursorPosition(m_FailLabelX, m_FailLabelY);
            Console.Write(blankLabel);
            Console.CursorLeft = Console.CursorLeft - 8;
            Console.Write(results.Lost);

            // Update peak label
            Console.SetCursorPosition(m_PeakLabelX, m_PeakLabelY);
            Console.Write(new string(' ', 15));
            Console.CursorLeft = Console.CursorLeft - 15;
            List<double> noTimeoutResponses = new List<double>();
            noTimeoutResponses.AddRange(m_ResponseTimes);
            noTimeoutResponses.RemoveAll(x => x == 0d);
            Console.Write("{0}ms",
                m_ResponseTimes.Count > 0 && noTimeoutResponses.Count > 0 ? Math.Round(noTimeoutResponses.Max(), 1) : 0);


            // Update RTT label
            Console.SetCursorPosition(m_RttLabelX, m_RttLabelY);
            Console.Write(blankLabel);
            Console.CursorLeft = Console.CursorLeft - 8;
            Console.Write("{0:0.0}ms", results.CurrTime);

            // Update time label
            Console.SetCursorPosition(m_TimeLabelX, m_TimeLabelY);
            Console.Write(blankLabel + "        ");
            Console.CursorLeft = Console.CursorLeft - 16;
            Console.Write("{0:hh\\:mm\\:ss}", results.TotalRunTime);

            // Reset cursor to starting position
            Console.SetCursorPosition(cursorPositionX, cursorPositionY);
        }

    }
}

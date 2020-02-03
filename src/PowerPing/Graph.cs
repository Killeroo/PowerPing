/*
MIT License - PowerPing 

Copyright (c) 2020 Matthew Carney

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

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
        public bool CompactGraph = false;
        public int EndCursorPosY = 0; // Position to move cursor to when graph exits

        // Local variable declaration
        private readonly CancellationToken m_CancellationToken;
        private readonly Ping m_Ping;
        private readonly PingAttributes m_PingAttributes = new PingAttributes();
        private readonly List<string[]> m_Columns = new List<string[]>();
        private readonly List<double> m_ResponseTimes = new List<double>();
        private bool m_IsGraphSetup = false;
        private int m_yAxisLength = 20;
        private int m_xAxisLength = 40;
        private int m_StartScale = 5;
        private int m_Scale = 5;//50;

        // Location of graph plotting space
        private int m_PlotStartX;
        private int m_PlotStartY;

        // Label locations
        private int m_SentLabelX, m_SentLabelY;
        private int m_RecvLabelX, m_RecvLabelY;
        private int m_FailLabelX, m_FailLabelY;
        private int m_RttLabelX, m_RttLabelY;
        private int m_TimeLabelX, m_TimeLabelY;
        private int m_yAxisStart;
        
        public Graph(string address, CancellationToken cancellationTkn)
        {
            m_CancellationToken = cancellationTkn;
            m_Ping = new Ping(cancellationTkn);
            m_Scale = m_StartScale;

            // Setup ping attributes
            m_PingAttributes.InputtedAddress = Lookup.QueryDNS(address, System.Net.Sockets.AddressFamily.InterNetwork);
            m_PingAttributes.Continous = true;
        }

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

            // Start drawing graph
            Draw();

            // Show cursor
            Console.CursorVisible = true;
        }

        /// <summary>
        /// Stores graph drawing loop
        /// </summary>
        private void Draw()
        {
            // The actual display update rate may be limited by the ping interval
            RateLimiter displayUpdateLimiter = new RateLimiter(TimeSpan.FromMilliseconds(500));

            // This callback will run after each ping iteration
            void ResultsUpdateCallback(PingResults r) {
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
                if (scalePrevious != m_Scale)
                {
                    DrawYAxisLabels();
                }

                Console.CursorTop = EndCursorPosY;
                
            }

            // Start pinging
            PingResults results = m_Ping.Send(m_PingAttributes, ResultsUpdateCallback);
        }
        ///<summary>
        /// Setup graph
        /// </summary>
        private void Setup() 
        {
            // Determine Xaxis size
            if (!CompactGraph) {
                m_xAxisLength = Console.WindowWidth - 50;
            }

            DrawBackground();

            DrawYAxisLabels();

            m_IsGraphSetup = true;
        }
        /// <summary>
        /// Checks if the graph Y axis need to be scaled down or up
        /// Scaling is linear, so scale doubles or halves each time.
        /// I couldn't be asked to try and hack exponential scaling here 
        /// althrough it would probably work better
        /// </summary>
        void CheckGraphScale(double newResponseTime)
        {
            int newTime = Convert.ToInt32(newResponseTime);
            int maxTime = m_Scale * m_yAxisLength;

            // Did we exceed our current scale?
            if (newTime > maxTime)
            {
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
            foreach (double responseTime in m_ResponseTimes)
            {
                int time = Convert.ToInt32(responseTime);

                if (time > maxTime / 2)
                {
                    scaleDown = false;
                }
            }

            // If so scale down
            if (scaleDown && m_Scale != m_StartScale)
            {
                m_Scale /= 2;
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

                ConsoleColor color = ConsoleColor.Gray;

                // This causes us to draw a continous lower line of red when we are continously timing out
                // Instead of always drawing big red lines, we draw them at either end of the continous zone
                // I think it will just look nicer, it will cause slightly hackier code but oh well
                bool drawTimeoutSegment = false; 

                // Alternate colour between columns for clarity
                if (x % 2 == 0) {
                    color = ConsoleColor.Gray;
                } else {
                    color = ConsoleColor.DarkGray;
                }
                if (x == m_ResponseTimes.Count - 1) {
                    color = ConsoleColor.Green;
                }
                if (m_ResponseTimes[x] == 0)
                {
                    color = ConsoleColor.DarkRed;
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

                DrawSingleColumn(CreateColumn(m_ResponseTimes[x]), color, drawTimeoutSegment);

                Console.CursorLeft++; 
            }

            // Reset colour after
            Console.ForegroundColor = ConsoleColor.Gray;
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

            // Draw Y axis of graph
            Console.WriteLine("                ┐");
            Console.WriteLine("                ┤");
            Console.WriteLine("               ─┤");
            Console.WriteLine("                ┤");
            Console.WriteLine("               ─┤");
            Console.WriteLine("                ┤");
            Console.WriteLine("               ─┤");
            Console.WriteLine("                ┤");
            Console.WriteLine(" Response      ─┤");
            Console.WriteLine("   Time         ┤");
            Console.WriteLine("   (ms)        ─┤");
            Console.WriteLine("                ┤");
            Console.WriteLine("               ─┤");
            Console.WriteLine("                ┤");
            Console.WriteLine("               ─┤");
            Console.WriteLine("                ┤");
            Console.WriteLine("               ─┤");
            Console.WriteLine("                ┤");
            Console.WriteLine("               ─┤");
            Console.WriteLine("                ┤");


            // Draw X axis of graph
            Console.Write("              0 └");

            // Save start of graph plotting area
            m_PlotStartX = Console.CursorLeft;
            m_PlotStartY = Console.CursorTop;
            Console.WriteLine(new string('─', m_xAxisLength));
            Console.WriteLine();

            // Draw info (and get location info for each label)
            Console.WriteLine("                 Ping Statistics:");
            Console.WriteLine("                -----------------------------------");
            Console.WriteLine("                 Destination [ {0} ]", m_PingAttributes.InputtedAddress);

            Console.Write("                     Sent: ");
            m_SentLabelX = Console.CursorLeft;
            m_SentLabelY = Console.CursorTop;

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("           Received: ");
            m_RecvLabelX = Console.CursorLeft;
            m_RecvLabelY = Console.CursorTop;
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Gray;

            Console.Write("                      RTT: ");
            m_RttLabelX = Console.CursorLeft;
            m_RttLabelY = Console.CursorTop;

            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("               Lost: ");
            m_FailLabelX = Console.CursorLeft;
            m_FailLabelY = Console.CursorTop;
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Gray;
            
            Console.Write("                 Time Elapsed: ");
            m_TimeLabelX = Console.CursorLeft;
            m_TimeLabelY = Console.CursorTop;
            Console.WriteLine();

            EndCursorPosY = Console.CursorTop;
        }
        /// <summary>
        /// Draw graph bar
        /// </summary>
        /// <param name="column">Column segments in array</param>
        private void DrawSingleColumn(string[] column, ConsoleColor color, bool timeoutSegment)
        {
            // save cursor location
            int cursorPositionX = Console.CursorLeft;
            int cursorPositionY = Console.CursorTop;

            if (timeoutSegment)
            {
                Console.Write("─");

                Console.CursorLeft--;
                return;
            }

            bool inverting = false;
            foreach (string segment in column)
            {
                if (color != ConsoleColor.DarkGray && color != ConsoleColor.Gray)
                {

                    Console.ForegroundColor = color;
                }
                else
                {
                    if (inverting)
                    {
                        Console.ForegroundColor = ConsoleColor.Gray;
                        inverting = false;
                    }
                    else
                    {
                        inverting = true;
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                    }
                }

                Console.Write(segment);

                // Stop over drawing at the top of the graph
                if (Console.CursorTop == m_yAxisStart)
                {
                    break;
                }

                if (Console.CursorTop != 0)
                {
                    Console.CursorTop--;
                    Console.CursorLeft--;
                }
            }

            // Reset cursor to starting position
            Console.SetCursorPosition(cursorPositionX, cursorPositionY);
        }
        public void DrawYAxisLabels()
        {
            // TODO: Only redraw if they have changed
            
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
                    Console.Write(currValue.ToString().PadLeft(14) + " ");
                } else {
                    Console.Write(new string(' ', 15));
                }

                // Add indentation every 2 lines 
                if (x % 2 == 0) { 
                    Console.Write("─");
                } else {
                    Console.Write(" ");
                }
                
                if (x == maxLines)
                    Console.WriteLine("┐");
                else 
                    Console.WriteLine("┤");

                currValue -= m_Scale;
            }

            // Draw name of axis
            Console.CursorTop = m_yAxisStart + maxLines / 2;
            Console.CursorLeft = 1;
            Console.WriteLine("Response");
            Console.CursorLeft = 3;
            Console.WriteLine("time");
            Console.CursorLeft = 3;
            Console.WriteLine("(MS)");

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

            // Update fail label
            Console.SetCursorPosition(m_FailLabelX, m_FailLabelY);
            Console.Write(blankLabel);
            Console.CursorLeft = Console.CursorLeft - 8;
            Console.Write(results.Lost);

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
            for (int x = 0; x < time; x = x +  m_Scale) {
                count++;
            }


            CheckGraphScale(replyTime);

            if (time > m_Scale * m_yAxisLength) {
                // If reply time over graph Y range draw max size column

                count = 10;
            } else if (time == 0) {
                // If no reply dont draw column
                string[] timeoutBar = new string[m_yAxisLength];
                timeoutBar[0] = "┴";
                for (int x = 1; x < m_yAxisLength; x++)
                {
                    timeoutBar[x] = "|";
                }
                return timeoutBar;//new string[] { "─" };
            }

            // Add special character at top and below

            // Remove all the stuff below

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
            //Console.WriteLine(nearestMultiple - time);
            //if (0 < time - (count * m_Scale))
            //{

            //    //bar[bar.Length - 1] = HALF_BAR_BLOCK_CHAR;
            //    if (time % m_Scale > m_Scale / 2)
            //    {
            //        bar[bar.Length - 1] = HALF_BAR_BLOCK_CHAR;
            //    }
            //    else
            //    {
            //        bar[bar.Length - 1] = FULL_BAR_BLOCK_CHAR;
            //    }
            //} 
            //else
            //{
            //    bar[bar.Length - 1] = " ";
            //}

            if (nearestMultiple - time < 0)
            {
                bar[bar.Length - 1] = " ";
            }
            else
            {
                bar[bar.Length - 1] = HALF_BAR_BLOCK_CHAR;
            }

            //Console.WriteLine(time - (count * m_Scale));
            
            // m_Scale/2 < Math.Abs(time - (count * m_Scale)))
            //    bar[bar.Length - 1] = " ";
            //} 
            //    else
            //    {
            //        bar[bar.Length - 1] = HALF_BAR_BLOCK_CHAR;
            //    }

            // Work out top character
            //if (time % m_Scale >= 0) {
            //    bar[count] = FULL_BAR_BLOCK_CHAR;
            //} else {
            //    bar[count] = HALF_BAR_BLOCK_CHAR;
            //}

            return bar;

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
        /// Clear the plotting area of the graph
        /// </summary>
        private void Clear()
        {
            // save cursor location
            int cursorPositionX = Console.CursorLeft;
            int cursorPositionY = Console.CursorTop;

            // Set cursor position to start of plot
            Console.SetCursorPosition(m_PlotStartX, m_PlotStartY);

            string blankRow = new string(' ', m_xAxisLength);
            string bottomRow = new string('─', m_xAxisLength);
            
            for (int x = 0; x <= 21; x++) {
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
    }
}

/*
MIT License - PowerPing 

Copyright (c) 2019 Matthew Carney

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

        // Properties
        public bool CompactGraph = false;
        public int EndCursorPosY = 0; // Position to move cursor to when graph exits

        // Local variable declaration
        private readonly CancellationToken m_CancellationToken;
        private readonly Ping m_Ping;
        private readonly PingAttributes m_PingAttributes = new PingAttributes();
        private readonly List<String[]> m_Columns = new List<string[]>();
        private bool m_IsGraphSetup = false;
        private int m_xAxisLength = 40;
        private int m_Scale = 1;

        // Location of graph plotting space
        private int m_PlotStartX;
        private int m_PlotStartY;

        // Label locations
        private int m_SentLabelX, m_SentLabelY;
        private int m_RecvLabelX, m_RecvLabelY;
        private int m_FailLabelX, m_FailLabelY;
        private int m_RttLabelX, m_RttLabelY;
        private int m_TimeLabelX, m_TimeLabelY;
        private int m_YAxisStart;
        
        public Graph(string address, CancellationToken cancellationTkn)
        {
            m_CancellationToken = cancellationTkn;
            m_Ping = new Ping(cancellationTkn);

            // Setup ping attributes
            m_PingAttributes.InputtedAddress = PowerPing.Lookup.QueryDNS(address, System.Net.Sockets.AddressFamily.InterNetwork);
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

                // Reset position
                Console.CursorTop = m_PlotStartY;
                Console.CursorLeft = m_PlotStartX;

                // Draw graph columns
                DrawGraphColumns();

                // Update labels
                UpdateLabels(r);

                // Get results from ping and add to graph
                AddColumnToGraph(CreateColumn(r.CurTime));

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
                m_xAxisLength = Console.WindowWidth - 30;
            }

            DrawBackground();

            m_IsGraphSetup = true;
        }
        /// <summary>
        /// Draw all graph coloums/bars
        /// </summary>
        private void DrawGraphColumns()
        {
            // Clear columns space before drawing
            Clear();

            for (int x = 0; x < m_Columns.Count; x++) {
                // Change colour for most recent column 
                if (x == m_Columns.Count - 1) {
                    Console.ForegroundColor = ConsoleColor.Green;
                }
                DrawBar(m_Columns[x]);
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

            // Draw Y axis of graph
            if (CompactGraph) {
                Console.WriteLine("         >1000 ┐");
                Console.WriteLine("           900 ┤");
                Console.WriteLine("           800 ┤");
                Console.WriteLine("           700 ┤");
                Console.WriteLine(" Response  600 ┤");
                Console.WriteLine("   Time    500 ┤");
                Console.WriteLine("   (ms)    400 ┤");
                Console.WriteLine("           300 ┤");
                Console.WriteLine("           200 ┤");
                Console.WriteLine("           100 ┤");
            } else {
                Console.WriteLine("          >1000 ┐");
                Console.WriteLine("                ┤");
                Console.WriteLine("           900 ─┤");
                Console.WriteLine("                ┤");
                Console.WriteLine("           800 ─┤");
                Console.WriteLine("                ┤");
                Console.WriteLine("           700 ─┤");
                Console.WriteLine("                ┤");
                Console.WriteLine(" Response  600 ─┤");
                Console.WriteLine("   Time         ┤");
                Console.WriteLine("   (ms)    500 ─┤");
                Console.WriteLine("                ┤");
                Console.WriteLine("           400 ─┤");
                Console.WriteLine("                ┤");
                Console.WriteLine("           300 ─┤");
                Console.WriteLine("                ┤");
                Console.WriteLine("           200 ─┤");
                Console.WriteLine("                ┤");
                Console.WriteLine("           100 ─┤");
                Console.WriteLine("                ┤");
            }


            // Draw X axis of graph
            Console.Write(CompactGraph ? "             0 └" : "              0 └");
            // Save start of graph plotting area
            m_PlotStartX = Console.CursorLeft;
            m_PlotStartY = Console.CursorTop;
            Console.WriteLine(new String('─', m_xAxisLength));
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
        /// <param name="bar"></param>
        private void DrawBar(String[] bar)
        {
            // save cursor location
            int cursorPositionX = Console.CursorLeft;
            int cursorPositionY = Console.CursorTop;

            foreach(String segment in bar)
            {
                Console.Write(segment);
                Console.CursorTop--;
                Console.CursorLeft--;
            }

            // Reset cursor to starting position
            Console.SetCursorPosition(cursorPositionX, cursorPositionY);
        }
        /// <summary>
        /// Update graph text labels
        /// </summary>
        /// <param name="results"></param>
        private void UpdateLabels(PingResults results)
        {
            // save cursor location
            int cursorPositionX = Console.CursorLeft;
            int cursorPositionY = Console.CursorTop;

            String blankLabel = new String(' ', 6);

            // Update sent label
            Console.SetCursorPosition(m_SentLabelX, m_SentLabelY);
            // Clear label first
            Console.Write(blankLabel);
            // Move cursor back
            Console.CursorLeft = Console.CursorLeft - 6;
            // Write label value
            Console.Write(results.Sent);

            // Update recieve label
            Console.SetCursorPosition(m_RecvLabelX, m_RecvLabelY);
            Console.Write(blankLabel);
            Console.CursorLeft = Console.CursorLeft - 6;
            Console.Write(results.Received);

            // Update fail label
            Console.SetCursorPosition(m_FailLabelX, m_FailLabelY);
            Console.Write(blankLabel);
            Console.CursorLeft = Console.CursorLeft - 6;
            Console.Write(results.Lost);

            // Update RTT label
            Console.SetCursorPosition(m_RttLabelX, m_RttLabelY);
            Console.Write(blankLabel);
            Console.CursorLeft = Console.CursorLeft - 6;
            Console.Write("{0:0.0}ms", results.CurTime);

            // Update time label
            Console.SetCursorPosition(m_TimeLabelX, m_TimeLabelY);
            Console.Write(blankLabel + "        ");
            Console.CursorLeft = Console.CursorLeft - 14;
            Console.Write("{0:hh\\:mm\\:ss}", results.TotalRunTime);

            // Reset cursor to starting position
            Console.SetCursorPosition(cursorPositionX, cursorPositionY);
        }
        /// <summary>
        /// Generate bar for graph
        /// </summary>
        /// <param name="time">Reply time of packet to plot</param>
        private String[] CreateColumn(double replyTime)
        {
            String[] bar;
            int count = 0;
            int time = Convert.ToInt32(replyTime);

            // Work out bar length
            for (int x = 0; x < time; x = x + (CompactGraph ? 50 : m_Scale)) {
                count++;
            }

            if (time > 1000) {
                // If reply time over graph Y range draw max size column
                count = CompactGraph ? 20 : 10;
            } else if (time == 0) {
                // If no reply dont draw column
                return new String[] { "─" };
            }

            count = count / 2;

            // Create array to store bar
            bar = new String[count + 1];

            // Fill bar
            for (int x = 0; x < count + 1; x = x + 1) {
                bar[x] = FULL_BAR_BLOCK_CHAR;
            }

            // Replace lowest bar segment
            bar[0] = "▀";

            // Work out top segment based on length
            if (CompactGraph) { // Work out for compact graph
                if (count + 1 % 2 == 0) {
                    bar[count] = FULL_BAR_BLOCK_CHAR;
                } else if (time <= 100) {
                    if (time <= 50) {
                        bar[count] = "▀";
                    } else {
                        bar[count] = HALF_BAR_BLOCK_CHAR;
                    }
                } else {
                    bar[count] = FULL_BAR_BLOCK_CHAR;
                }
            } else { // Work out for full graph
                if (count + 1 % 2 == 0) {
                    bar[count] = FULL_BAR_BLOCK_CHAR;
                } else if (time <= 100) {
                    if (time <= 25) {
                        bar[count] = "▀";
                    } else if (time <= 50) {
                        bar[count] = HALF_BAR_BLOCK_CHAR;
                    } else if (time <= 75) {
                        bar[count] = FULL_BAR_BLOCK_CHAR;
                    } else {
                        bar[count] = HALF_BAR_BLOCK_CHAR;
                    }
                } else {
                    bar[count] = FULL_BAR_BLOCK_CHAR;
                }
            }

            return bar;

        }
        /// <summary>
        /// Add a column to the graph list
        /// </summary>
        private void AddColumnToGraph(String[] col)
        {
            m_Columns.Add(col);

            // If number of columns exceeds x Axis length
            if (m_Columns.Count >= m_xAxisLength) {
                // Remove first element
                m_Columns.RemoveAt(0);
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

            String blankRow = new String(' ', m_xAxisLength);
            String bottomRow = new String('─', m_xAxisLength);

            for (int x = 0; x <= (CompactGraph ? 11 : 21); x++) {
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

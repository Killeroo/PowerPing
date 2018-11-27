/*
MIT License - PowerPing 

Copyright (c) 2018 Matthew Carney

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
        private readonly CancellationToken cancellationToken;
        private readonly Ping graphPing;
        private readonly PingAttributes graphPingAttrs = new PingAttributes();
        private readonly List<String[]> graphColumns = new List<string[]>();
        private bool isGraphSetup = false;
        private int xAxisLength = 40;

        // Location of graph plotting space
        private int plotStartX;
        private int plotStartY;

        // Label locations
        private int sentLabelX, sentLabelY;
        private int recLabelX, recLabelY;
        private int failLabelX, failLabelY;
        private int rttLabelX, rttLabelY;
        private int timeLabelX, timeLabelY;
        
        public Graph(string address, CancellationToken cancellationTkn)
        {
            cancellationToken = cancellationTkn;
            graphPing = new Ping(cancellationTkn);

            // Setup ping attributes
            graphPingAttrs.Host = PowerPing.Lookup.QueryDNS(address, System.Net.Sockets.AddressFamily.InterNetwork);
            graphPingAttrs.Continous = true;
        }

        public void Start()
        {
            // Disable output
            Display.ShowOutput = false;

            // Hide cursor
            Console.CursorVisible = false;

            // Check graph is setup
            if (!isGraphSetup) {
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
            var limiter = new DisplayUpdateLimiter(TimeSpan.FromMilliseconds(500));
            void OnResultsUpdate(PingResults r) {
                // Make sure we're not updating the display too frequently
                if (!limiter.RequestUpdate()) {
                    return;
                }

                // Reset position
                Console.CursorTop = plotStartY;
                Console.CursorLeft = plotStartX;

                // Draw graph columns
                DrawGraphColumns();

                // Update labels
                UpdateLabels(r);

                // Get results from ping and add to graph
                AddColumnToGraph(CreateColumn(r.CurTime));

                Console.CursorTop = EndCursorPosY;
            }

            // Start pinging
            PingResults results = graphPing.Send(graphPingAttrs, OnResultsUpdate);
        }
        ///<summary>
        /// Setup graph
        /// </summary>
        private void Setup() 
        {
            // Determine Xaxis size
            if (!CompactGraph) {
                xAxisLength = Console.WindowWidth - 30;
            }

            DrawBackground();

            isGraphSetup = true;
        }
        /// <summary>
        /// Draw all graph coloums/bars
        /// </summary>
        private void DrawGraphColumns()
        {
            // Clear columns space before drawing
            Clear();

            for (int x = 0; x < graphColumns.Count; x++) {
                // Change colour for most recent column 
                if (x == graphColumns.Count - 1) {
                    Console.ForegroundColor = ConsoleColor.Green;
                }
                DrawBar(graphColumns[x]);
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
            plotStartX = Console.CursorLeft;
            plotStartY = Console.CursorTop;
            Console.WriteLine(new String('─', xAxisLength));
            Console.WriteLine();

            // Draw info (and get location info for each label)
            Console.WriteLine("                 Ping Statistics:");
            Console.WriteLine("                -----------------------------------");
            Console.WriteLine("                 Destination [ {0} ]", graphPingAttrs.Host);

            Console.Write("                     Sent: ");
            sentLabelX = Console.CursorLeft;
            sentLabelY = Console.CursorTop;

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("           Received: ");
            recLabelX = Console.CursorLeft;
            recLabelY = Console.CursorTop;
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Gray;

            Console.Write("                      RTT: ");
            rttLabelX = Console.CursorLeft;
            rttLabelY = Console.CursorTop;

            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("               Lost: ");
            failLabelX = Console.CursorLeft;
            failLabelY = Console.CursorTop;
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Gray;
            
            Console.Write("                 Time Elapsed: ");
            timeLabelX = Console.CursorLeft;
            timeLabelY = Console.CursorTop;
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
            Console.SetCursorPosition(sentLabelX, sentLabelY);
            // Clear label first
            Console.Write(blankLabel);
            // Move cursor back
            Console.CursorLeft = Console.CursorLeft - 6;
            // Write label value
            Console.Write(results.Sent);

            // Update recieve label
            Console.SetCursorPosition(recLabelX, recLabelY);
            Console.Write(blankLabel);
            Console.CursorLeft = Console.CursorLeft - 6;
            Console.Write(results.Received);

            // Update fail label
            Console.SetCursorPosition(failLabelX, failLabelY);
            Console.Write(blankLabel);
            Console.CursorLeft = Console.CursorLeft - 6;
            Console.Write(results.Lost);

            // Update RTT label
            Console.SetCursorPosition(rttLabelX, rttLabelY);
            Console.Write(blankLabel);
            Console.CursorLeft = Console.CursorLeft - 6;
            Console.Write("{0:0.0}ms", results.CurTime);

            // Update time label
            Console.SetCursorPosition(timeLabelX, timeLabelY);
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
            for (int x = 0; x < time; x = x + (CompactGraph ? 50 : 25)) {
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
            graphColumns.Add(col);

            // If number of columns exceeds x Axis length
            if (graphColumns.Count >= xAxisLength) {
                // Remove first element
                graphColumns.RemoveAt(0);
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
            Console.SetCursorPosition(plotStartX, plotStartY);

            String blankRow = new String(' ', xAxisLength);
            String bottomRow = new String('─', xAxisLength);

            for (int x = 0; x <= (CompactGraph ? 11 : 21); x++) {
                // Draw black spaces
                Console.Write(blankRow);
                Console.CursorLeft = plotStartX;
                Console.CursorTop = plotStartY - x;
            }

            // Draw bottom row
            Console.CursorTop = plotStartY;
            Console.Write(bottomRow);

            // Reset cursor to starting position
            Console.SetCursorPosition(cursorPositionX, cursorPositionY);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading;

namespace PowerPing
{
    class Graph
    {
        const string FULL_BAR_BLOCK_CHAR = "█";
        const string HALF_BAR_BLOCK_CHAR = "▄";

        public bool compactGraph = false;

        // Local variable declaration
        private Ping graphPing = new Ping();
        private List<String[]> graphColumns = new List<string[]>();
        private bool isGraphSetup = false;
        private int xAxisLength = 40;
        private String address;

        // Location of graph plotting space
        private int plotStartX;
        private int plotStartY;

        // Label locations
        private int sentLabelX, sentLabelY;
        private int recLabelX, recLabelY;
        private int failLabelX, failLabelY;
        private int rttLabelX, rttLabelY;
        
        public Graph(string address)
        {
            // Setup ping
            graphPing.address = address;
            graphPing.message = "R U Alive?";
            graphPing.timeout = 3000;
            graphPing.continous = true;
            graphPing.interval = 1000;
            graphPing.ttl = 255;
        }

        public void start()
        {
            // Hide cursor
            Console.CursorVisible = false;

            // Check graph is setup
            if (!isGraphSetup)
                setup();

            // Start drawing graph
            draw();
        }

        private void draw()
        {
            // Drawing loop

            // Save start position
            plotStartY = Console.CursorTop - 5;
            plotStartX = 7;

            // Start sending ping
            var pinger = new Thread(new ThreadStart(graphPing.send));
            //graphPing.send();

            // Drawing loop
            while (true)
            {
                // Reset position
                Console.CursorTop = plotStartY;
                Console.CursorLeft = plotStartX;

                // Draw graph columns
                drawGraphColumns();

                // Update labels
                updateLabels(graphPing);

                // Get results from ping and add to graph
                addColumnToGraph(createColumn(graphPing.getCurrentResponseTime));

                // Wait one second
                Thread.Sleep(1000);
            }
            
        }

        /// <summary>
        /// Setup graph
        /// </summary>
        private void setup() 
        {
            drawBackground();

            isGraphSetup = true;
        }

        private void drawGraphColumns()
        {
            // Clear columns space before drawing
            clear();

            // Draw each graph column
            foreach (String[] bar in graphColumns)
            {
                drawBar(bar);
                Console.CursorLeft++;
            }
        }

        /// <summary>
        /// Draw graph background
        /// </summary>
        private void drawBackground()
        {
            // Draw Y axis of graph
            if (compactGraph)
            {
                Console.WriteLine(">1000 ┐");
                Console.WriteLine("  900 ┤");
                Console.WriteLine("  800 ┤");
                Console.WriteLine("  700 ┤");
                Console.WriteLine("  600 ┤");
                Console.WriteLine("  500 ┤");
                Console.WriteLine("  400 ┤");
                Console.WriteLine("  300 ┤");
                Console.WriteLine("  200 ┤");
                Console.WriteLine("  100 ┤");
            }
            else
            {
                Console.WriteLine(">1000 ┐");
                Console.WriteLine("      ┤");
                Console.WriteLine(" 900 ─┤");
                Console.WriteLine("      ┤");
                Console.WriteLine(" 800 ─┤");
                Console.WriteLine("      ┤");
                Console.WriteLine(" 700 ─┤");
                Console.WriteLine("      ┤");
                Console.WriteLine(" 600 ─┤");
                Console.WriteLine("      ┤");
                Console.WriteLine(" 500 ─┤");
                Console.WriteLine("      ┤");
                Console.WriteLine(" 400 ─┤");
                Console.WriteLine("      ┤");
                Console.WriteLine(" 300 ─┤");
                Console.WriteLine("      ┤");
                Console.WriteLine(" 200 ─┤");
                Console.WriteLine("      ┤");
                Console.WriteLine(" 100 ─┤");
                Console.WriteLine("      ┤");
            }
            

            // Draw X axis of graph
            drawXAxis();

            // Draw info (and get location info for each label)
            Console.Write("       Sent: ");
            sentLabelX = Console.CursorLeft;
            sentLabelY = Console.CursorTop;

            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.Write("       Recieved: ");
            recLabelX = Console.CursorLeft;
            recLabelY = Console.CursorTop;
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Gray;

            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.Write("                     Failed: ");
            failLabelX = Console.CursorLeft;
            failLabelY = Console.CursorTop;
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Gray;

            Console.WriteLine();
            Console.Write("Current round trip time (RTT): ");
            rttLabelX = Console.CursorLeft;
            rttLabelY = Console.CursorTop;
            Console.WriteLine();
        }

        /// <summary>
        /// Draw X axis of graph
        /// </summary>
        private void drawXAxis()
        {
            Console.Write("    0 └");
            for (int x = 0; x <= xAxisLength; x++)
            {
                Console.Write("─");
            }
            Console.WriteLine();
        }

        private void drawBar(String[] bar)
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

        private void updateLabels(Ping ping)
        {
            // save cursor location
            int cursorPositionX = Console.CursorLeft;
            int cursorPositionY = Console.CursorTop;

            String blankLabel = new String(' ', 4);

            // Update sent label
            Console.SetCursorPosition(sentLabelX, sentLabelY);
            // Clear label first
            Console.Write(blankLabel);
            // Move cursor back
            Console.CursorLeft = Console.CursorLeft - 4;
            // Write label value
            Console.Write(ping.getPacketsSent);

            // Update recieve label
            Console.SetCursorPosition(recLabelX, recLabelY);
            Console.Write(blankLabel);
            Console.CursorLeft = Console.CursorLeft - 4;
            Console.Write(ping.getPacketsRecieved);

            // Update fail label
            Console.SetCursorPosition(failLabelX, failLabelY);
            Console.Write(blankLabel);
            Console.CursorLeft = Console.CursorLeft - 4;
            Console.Write(ping.getPacketsLost);

            // Update RTT label
            Console.SetCursorPosition(rttLabelX, rttLabelY);
            Console.Write(blankLabel);
            Console.CursorLeft = Console.CursorLeft - 4;
            Console.Write(ping.getCurrentResponseTime);

            // Reset cursor to starting position
            Console.SetCursorPosition(cursorPositionX, cursorPositionY);
        }

        /// <summary>
        /// Clear the plotting area of the graph
        /// </summary>
        private void clear()
        {
            // save cursor location
            int cursorPositionX = Console.CursorLeft;
            int cursorPositionY = Console.CursorTop;

            // Set cursor position to start of plot
            Console.SetCursorPosition(plotStartX, plotStartY);

            String blankRow = new String(' ', xAxisLength);
            String bottomRow = new String('─', xAxisLength);

            for(int x = 0; x < (compactGraph ? 10 : 20); x++)
            {
                // Draw black spaces
                Console.Write(blankRow);
                Console.CursorLeft = plotStartX;
                Console.CursorTop--;

                // Draw bottom row
                if (x == 9 || x == 19)
                {
                    Console.CursorTop = plotStartY;
                    Console.Write(bottomRow);
                }
            }

            // Reset cursor to starting position
            Console.SetCursorPosition(cursorPositionX, cursorPositionY);
        }

        /// <summary>
        /// Generate bar for graphbased
        /// </summary>
        /// <param name="replyTime">Reply time of packet to plot</param>
        private String[] createColumn(long replyTime)
        {
            String[] bar;
            int count = 0;

            // Work out bar length
            for (int x = 0; x < replyTime; x = x + (compactGraph ? 50 : 25))
                count++;

            count = count / 2;

            // Create array to store bar
            bar = new String[count + 1];

            // Fill bar
            for (int x = 0; x < count + 1; x = x + 1)// + 2)
                bar[x] = FULL_BAR_BLOCK_CHAR;

            // Replace lowest bar segment
            bar[0] = "▀";

            // Work out top segment based on length
            if (compactGraph) // Work out for compact graph
            {
                if (count + 1 % 2 == 0)
                    bar[count] = FULL_BAR_BLOCK_CHAR;
                else if (replyTime <= 100)
                    if (replyTime <= 50)
                        bar[count] = "▀";
                    else
                        bar[count] = HALF_BAR_BLOCK_CHAR;
                else
                    bar[count] = FULL_BAR_BLOCK_CHAR;
            }
            else // Work out for full graph
            {
                if (count + 1 % 2 == 0)
                    bar[count] = FULL_BAR_BLOCK_CHAR;
                else if (replyTime <= 100)
                    if (replyTime <= 25)
                        bar[count] = "▀";//HALF_BAR_BLOCK_CHAR;
                    else if (replyTime <= 50)
                        bar[count] = HALF_BAR_BLOCK_CHAR;
                    else if (replyTime <= 75)
                        bar[count] = FULL_BAR_BLOCK_CHAR;
                    else
                        bar[count] = HALF_BAR_BLOCK_CHAR;//FULL_BAR_BLOCK_CHAR;//"▀";
                else
                    bar[count] = FULL_BAR_BLOCK_CHAR;//HALF_BAR_BLOCK_CHAR;
            }

            return bar;

        }

        /// <summary>
        /// Add a column to the graph list
        /// </summary>
        private void addColumnToGraph(String[] col)
        {
            graphColumns.Add(col);

            // If number of columns exceeds x Axis length
            if (graphColumns.Count >= xAxisLength)
                // Remove first element
                graphColumns.RemoveAt(0);
        }
    }
}

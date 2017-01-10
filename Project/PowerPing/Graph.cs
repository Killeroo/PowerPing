using System;
using System.Collections.Generic;
using System.Threading;

namespace PowerPing
{
    class Graph
    {
        const string FULL_BAR_BLOCK_CHAR = "█";
        const string HALF_BAR_BLOCK_CHAR = "▄";

        int plotStartX;
        int plotStartY;

        List<String[]> graphColumns = new List<string[]>();

        bool compactGraph = true;
        bool isGraphSetup = false;
        int xAxisLength = 40;

        public void start()
        {
            Console.CursorVisible = false;

            // Check graph is setup
            if (!isGraphSetup)
                setup();

            addColumnToGraph(createBar(100));

            addColumnToGraph(createBar(95));

            addColumnToGraph(createBar(110));

            addColumnToGraph(createBar(20));

            addColumnToGraph(createBar(50));

            addColumnToGraph(createBar(75));

            addColumnToGraph(createBar(22));

            addColumnToGraph(createBar(120));

            addColumnToGraph(createBar(220));

            // Start drawing graph
            draw();
            
        }

        private void draw()
        {
            // Drawing loop

            // Save start position
            plotStartY = Console.CursorTop - 5;
            plotStartX = 7;

            // Drawing loop
            while (true)
            {
                // Reset position
                Console.CursorTop = plotStartY;
                Console.CursorLeft = plotStartX;


                //// Go through each column of graph X axis
                //for (int x = 0; x <= xAxisLength; x++)
                //{
                //    // TODO:

                //    Thread.Sleep(1000);
                //}

                drawGraphColumns();

                Random rand = new Random();

                Console.Read();

                addColumnToGraph(createBar(rand.Next(1,900)));

                //Thread.Sleep(1000);
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
            for (int x = 0; x <= xAxisLength; x++)
            {

            }

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

            // Draw info
            Console.WriteLine("       Sent:       Recieved:");
            Console.WriteLine("                     Failed:");
            Console.WriteLine();
            Console.WriteLine("Current round trip time (RTT): ");
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

        public void clearGraph()
        {
            // Save orginal cursor

            for(int x = 0; x <= xAxisLength; x++)
            {

            }
        }

        /// <summary>
        /// Generate bar for graphbased
        /// </summary>
        /// <param name="replyTime">Reply time of packet to plot</param>
        private String[] createBar(long replyTime)
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

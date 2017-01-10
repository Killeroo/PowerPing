using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPing
{
    class Graph
    {
        const string FULL_BAR_BLOCK_CHAR = "█";
        const string HALF_BAR_BLOCK_CHAR = "▄";

        int xAxisStartPoint;

        List<String[]> graphColumns;

        bool compactGraph = false;
        bool isGraphSetup = false;
        int xAxisLength = 40;

        public void start()
        {
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
            int graphStartY = Console.CursorTop - 5;
            int graphStartX = 7;

            // Drawing loop
            while (true)
            {
                // Reset position
                Console.CursorTop = graphStartY;
                Console.CursorLeft = graphStartX;

                // Go through each column of graph X axis
                for (int x = 0; x <= 40; x++)
                {
                    // TODO:
                }
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
            foreach(String segment in bar)
            {
                Console.Write(segment);
                Console.CursorTop--;
                Console.CursorLeft--;
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
                else if (count == 1)
                    if (replyTime <= 50)
                        bar[count] = HALF_BAR_BLOCK_CHAR;
                    else
                        bar[count] = FULL_BAR_BLOCK_CHAR;//"▀";
                else
                    bar[count] = HALF_BAR_BLOCK_CHAR;
            }

            return bar;

        }

    }
}

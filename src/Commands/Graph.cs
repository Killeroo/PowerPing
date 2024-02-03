/**************************************************************************
 * PowerPing - Advanced command line ping tool
 * Copyright (c) 2024 Matthew Carney [matthewcarney64@gmail.com]
 * https://github.com/Killeroo/PowerPing
 *************************************************************************/

namespace PowerPing
{
    /// <summary>
    /// Graph class, sends pings using Ping.cs and displays on
    /// console based graph.
    /// </summary>
    internal class Graph
    {
        // Constants
        private const string FULL_BAR_BLOCK_CHAR = "█";

        private const string HALF_BAR_BLOCK_CHAR = "▄";
        private const string BOTTOM_BAR_BLOCK_CHAR = "▀";

        // Properties
        public int EndCursorPosY { get; set; } = 0; // Position to move cursor to when graph exits

        // Local variable declaration
        private readonly CancellationToken _cancellationToken;
        private readonly Ping _ping;
        private readonly PingAttributes _pingAttributes = new PingAttributes();
        private readonly List<string[]> _columns = new List<string[]>();
        private readonly List<double> _responseTimes = new List<double>();

        private bool _isGraphSetup = false;
        private int _yAxisLength = 20;
        private int _xAxisLength = 40;
        private int _yAxisLeftPadding = 14;
        private int _legendLeftPadding = 16;
        private int _startScale = 5; // Stops graph from scaling in past its start scale
        private int _scale = 5;//50; // Our current yaxis graph scale
        private double _lastAvg = 0;
        private double _lastRes = 0;

        // Limits refreshing display too quickly
        // NOTE: The actual display update rate may be limited by the ping interval
        private readonly RateLimiter _displayUpdateLimiter = new RateLimiter(TimeSpan.FromMilliseconds(500));

        // Graph positioning and properties
        private readonly int _normalLegendLeftPadding = 13;
        private readonly int _normalYAxisLeftPadding = 11;
        private readonly int _normalYAxisLength = 20;
        private readonly int _normalXAxisLength = 70;

        // Location of graph plotting space
        private int _plotStartX;
        private int _plotStartY;

        // Label locations
        private int _sentLabelX, _sentLabelY;

        private int _recvLabelX, _recvLabelY;
        private int _failLabelX, _failLabelY;
        private int _rttLabelX, _rttLabelY;
        private int _timeLabelX, _timeLabelY;
        private int _avgLabelX, _avgLabelY;
        private int _peakLabelX, _peakLabelY;
        private int _yAxisStart;

        public Graph(string address, CancellationToken cancellationTkn)
        {
            // Setup ping attributes
            _pingAttributes.InputtedAddress = address;
            _pingAttributes.Continous = true;

            _cancellationToken = cancellationTkn;
            _ping = new Ping(_pingAttributes, cancellationTkn);
            _ping.OnResultsUpdate += OnPingResultsUpdateCallback;
            _scale = _startScale;
        }

        /// <summary>
        /// Draws and sets up graph when it is first run
        /// </summary>
        public void Start()
        {
            // Hide cursor
            Console.CursorVisible = false;

            // Check graph is setup
            if (!_isGraphSetup)
            {
                Setup();
            }

            // Start pinging (initates update loop in OnPingResultsUpdateCallback)
            _ping.Send();

            // Show cursor
            Console.CursorVisible = true;
        }

        ///<summary>
        /// Setup graph
        /// </summary>
        private void Setup()
        {
            // Setup graph properties 
            _yAxisLength = _normalYAxisLength;
            _xAxisLength = _normalXAxisLength;
            _legendLeftPadding = _normalLegendLeftPadding;
            _yAxisLeftPadding = _normalYAxisLeftPadding;

            CheckAndResizeGraph();
            DrawBackground();
            DrawYAxisLabels();

            _isGraphSetup = true;
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
            Console.SetCursorPosition(_plotStartX, _yAxisStart);//m_PlotStartY);

            string blankRow = new string(' ', _xAxisLength);
            string bottomRow = new string('─', _xAxisLength);

            for (int x = 0; x <= _yAxisLength; x++)
            { //21; x++) {
                // Draw black spaces
                Console.Write(blankRow);
                Console.CursorLeft = _plotStartX;
                Console.CursorTop = _plotStartY - x;
            }

            // Draw bottom row
            Console.CursorTop = _plotStartY;
            Console.Write(bottomRow);

            // Reset cursor to starting position
            Console.SetCursorPosition(cursorPositionX, cursorPositionY);
        }

        // This callback will run after each ping iteration
        // This is technically our main update loop for the graph
        private void OnPingResultsUpdateCallback(PingResults r)
        {
            // Make sure we're not updating the display too frequently
            if (!_displayUpdateLimiter.RequestRun())
            {
                return;
            }

            int scalePrevious = _scale;

            // Reset position
            Console.CursorTop = _plotStartY;
            Console.CursorLeft = _plotStartX;

            // Update labels
            UpdateLegend(r);

            // Get results from ping and add to graph
            AddResponseToGraph(r.CurrTime);

            // Draw graph columns
            DrawColumns();

            // Only draw the y axis labels if the scale has changed
            if (scalePrevious != _scale)
            {
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
            int maxTime = _scale * _yAxisLength;

            // Did we exceed our current scale?
            if (newTime > maxTime)
            {
                _scale *= 2; // Expand!

                // Recurse back into ourself to check the scale again
                // Just in case we have to increase the scale all at once
                // we want to do it now, instead we will have a jumpy
                // rescaling look over the new few bars
                CheckGraphScale(newResponseTime);
            }

            // Check if any value on the graph is larger than half our current
            // max y axis value
            bool scaleDown = true;
            foreach (double responseTime in _responseTimes)
            {
                int time = Convert.ToInt32(responseTime);

                if (time > maxTime / 2)
                {
                    scaleDown = false;
                }
            }

            // If so scale down
            if (scaleDown && _scale != _startScale)
            {
                _scale /= 2;
            }
        }

        /// <summary>
        /// Checks the current console window size and adjusts the graph if
        /// required.
        /// </summary>
        private void CheckAndResizeGraph()
        {
            if (Console.WindowWidth < _xAxisLength + _yAxisLeftPadding)
            {
                _xAxisLength = Math.Max(Console.WindowWidth - _yAxisLeftPadding - 5, 35);
            }
            if (Console.WindowHeight < _yAxisLength)
            {
                _yAxisLength = Math.Max(Console.WindowHeight - 5, 10);
            }
        }

        /// <summary>
        /// Add a column to the graph list
        /// </summary>
        private void AddResponseToGraph(double responseTime)
        {
            _responseTimes.Add(responseTime);

            // If number of columns exceeds x Axis length
            if (_responseTimes.Count >= _xAxisLength)
            {
                // Remove first element
                _responseTimes.RemoveAt(0);
            }
        }

        /// <summary>
        /// Draw all graph coloums/bars
        /// </summary>
        private void DrawColumns()
        {
            // Clear columns space before drawing
            Clear();

            for (int x = 0; x < _responseTimes.Count; x++)
            {
                // This causes us to draw a continous lower line of red when we are continously timing out
                // Instead of always drawing big red lines, we draw them at either end of the continous zone
                // I think it will just look nicer, it will cause slightly hackier code but oh well
                bool drawTimeoutSegment = false;

                // Column type
                bool timeout = false;
                bool current = false;

                if (x == _responseTimes.Count - 1)
                {
                    current = true;
                }
                if (_responseTimes[x] == 0)
                {
                    timeout = true;
                }

                // So to get a timeout segment we peak at the elements ahead and behind to check they are timeouts
                // if not we will just draw a normal line at the end of the timeout
                // Horrible hacky inline logic to make sure we don't outofbounds while checking behind and head in the array
                if (_responseTimes[x] == 0
                    && (x != 0 ? _responseTimes[x - 1] == 0 : false)
                    && ((x < _responseTimes.Count - 1 ? _responseTimes[x + 1] == 0 : false) || x == _responseTimes.Count - 1))
                {
                    drawTimeoutSegment = true;
                }

                DrawSingleColumn(CreateColumn(_responseTimes[x]), current, timeout, drawTimeoutSegment);

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
            for (int x = 0; x < time; x = x + _scale)
            {
                count++;
            }

            // Scale up or down graph as needed
            CheckGraphScale(replyTime);

            if (time > _scale * _yAxisLength)
            {
                // If reply time over graph Y range draw max size column
                count = 10;
            }
            else if (time == 0)
            {
                // If no reply dont draw column
                string[] timeoutBar = new string[_yAxisLength + 1];
                for (int x = 0; x < timeoutBar.Length; x++)
                {
                    timeoutBar[x] = "|";
                }
                timeoutBar[0] = "┴";
                return timeoutBar;
            }

            // Create array to store bar
            bar = new string[count + 1];

            // Fill bar
            for (int x = 0; x < count; x = x + 1)
            { // count + 1
                bar[x] = FULL_BAR_BLOCK_CHAR;
            }

            // Replace lowest bar segment
            bar[0] = "▀";

            // Replace the last segment
            // https://stackoverflow.com/a/2705553
            int nearestMultiple = (int)Math.Round((time / (double)_scale), MidpointRounding.AwayFromZero) * _scale;

            if (nearestMultiple - time < 0)
            {
                bar[bar.Length - 1] = " ";
            }
            else
            {
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
            _yAxisStart = Console.CursorTop;

            // Draw graph y axis
            // Hack: this is code that I copied from the y axis generation function
            int maxLines = _yAxisLength;
            int maxYValue = maxLines * _scale;
            int currValue = maxYValue;
            for (int x = maxLines; x != 0; x--)
            {
                // write current value with m_LegendLeftPadding (slightly less every 2 lines)
                if (x % 2 == 0)
                {
                    Console.Write(currValue.ToString().PadLeft(_yAxisLeftPadding) + " ");
                }
                else
                {
                    Console.Write(new string(' ', _yAxisLeftPadding + 1));
                }

                // Add indentation every 2 lines
                if (x % 2 == 0)
                {
                    Console.Write("─");
                }
                else
                {
                    Console.Write(" ");
                }

                if (x == maxLines)
                {
                    Console.WriteLine("┐");
                }
                else
                {
                    Console.WriteLine("┤");
                }

                currValue -= _scale;
            }

            // Draw X axis of graph
            Console.Write(new string(' ', _yAxisLeftPadding) + "0 └");

            // Save start of graph plotting area
            _plotStartX = Console.CursorLeft;
            _plotStartY = Console.CursorTop;
            Console.WriteLine(new string('─', _xAxisLength));
            Console.WriteLine();

            string leftPadding = new string(' ', _legendLeftPadding);

            // Draw info (and get location info for each label)
            Console.WriteLine(leftPadding + " Ping Statistics:");
            Console.WriteLine(leftPadding + "-----------------------------------");
            Console.WriteLine(leftPadding + " Destination [ {0} ]", _pingAttributes.InputtedAddress);

            Console.Write(leftPadding + "    Sent: ");
            _sentLabelX = Console.CursorLeft;
            _sentLabelY = Console.CursorTop;

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("           Received: ");
            _recvLabelX = Console.CursorLeft;
            _recvLabelY = Console.CursorTop;
            Console.ForegroundColor = ConsoleColor.Gray;

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("           Average: ");
            _avgLabelX = Console.CursorLeft;
            _avgLabelY = Console.CursorTop;
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Gray;

            Console.Write(leftPadding + " Current: ");
            _rttLabelX = Console.CursorLeft;
            _rttLabelY = Console.CursorTop;

            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("               Lost: ");
            _failLabelX = Console.CursorLeft;
            _failLabelY = Console.CursorTop;
            Console.ForegroundColor = ConsoleColor.Gray;

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("              Peak: ");
            _peakLabelX = Console.CursorLeft;
            _peakLabelY = Console.CursorTop;
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine();

            Console.Write(leftPadding + " Time Elapsed: ");
            _timeLabelX = Console.CursorLeft;
            _timeLabelY = Console.CursorTop;
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

            if (timeoutSegment)
            {
                Console.Write("─");

                Console.CursorLeft--;
                return;
            }

            bool inverting = false;
            foreach (string segment in column)
            {
                // Determine colour of segment
                if (timeout)
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                }
                else if (current)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                }
                else if (inverting)
                {
                    Console.ForegroundColor = ConsoleColor.Gray;
                    inverting = false;
                }
                else
                {
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
                if (cursorPositionTop == _yAxisStart)
                {
                    break;
                }

                if (cursorPositionTop != 0)
                {
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
            int maxLines = _yAxisLength;
            int maxYValue = maxLines * _scale;

            int topStart = Console.CursorTop;
            int leftStart = Console.CursorLeft;

            // Setup cursor position for drawing labels
            Console.CursorTop = _yAxisStart;
            Console.CursorLeft = 0;

            int currValue = maxYValue;
            for (int x = maxLines; x != 0; x--)
            {
                // write current value with padding (slightly less every 2 lines)
                if (x % 2 == 0)
                {
                    Console.Write(currValue.ToString().PadLeft(_yAxisLeftPadding) + " ");
                }
                else
                {
                    Console.Write(new string(' ', _yAxisLeftPadding + 1));
                }

                // Add indentation every 2 lines
                if (x % 2 == 0)
                {
                    Console.Write("─");
                }
                else
                {
                    Console.Write(" ");
                }

                if (x == maxLines)
                {
                    Console.WriteLine("┐");
                }
                else
                {
                    Console.WriteLine("┤");
                }

                currValue -= _scale;
            }

            // Draw name of y axis
            Console.CursorTop = _yAxisStart + maxLines / 2;
            Console.CursorLeft = 1;
            Console.WriteLine("Round");
            Console.CursorLeft = 2;
            Console.WriteLine("Trip");
            Console.CursorLeft = 2;
            Console.WriteLine("Time");
            Console.CursorLeft = 2;
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
            Console.SetCursorPosition(_sentLabelX, _sentLabelY);
            // Clear label first
            Console.Write(blankLabel);
            // Move cursor back
            Console.CursorLeft = Console.CursorLeft - 8;
            // Write label value
            Console.Write(results.Sent);

            // Update recieve label
            Console.SetCursorPosition(_recvLabelX, _recvLabelY);
            Console.Write(blankLabel);
            Console.CursorLeft = Console.CursorLeft - 8;
            Console.Write(results.Received);

            // Update average label
            Console.SetCursorPosition(_avgLabelX, _avgLabelY);
            Console.Write(new string(' ', 15));
            Console.CursorLeft = Console.CursorLeft - 15;
            double r = Math.Round(results.AvgTime, 1);
            if (_lastAvg < r)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.Write("+");
                if (results.CurrTime - _lastRes > 20)
                    Console.Write("+");
            }
            else if (_lastAvg > r)
            {
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.Write("-");
                if (_lastRes - results.CurrTime > 20)
                    Console.Write("-");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write("~");
            }
            _lastAvg = r;
            _lastRes = results.CurrTime;
            Console.ResetColor();
            Console.Write("{0:0.0}ms", results.AvgTime);

            // Update fail label
            Console.SetCursorPosition(_failLabelX, _failLabelY);
            Console.Write(blankLabel);
            Console.CursorLeft = Console.CursorLeft - 8;
            Console.Write(results.Lost);

            // Update peak label
            Console.SetCursorPosition(_peakLabelX, _peakLabelY);
            Console.Write(new string(' ', 15));
            Console.CursorLeft = Console.CursorLeft - 15;
            List<double> noTimeoutResponses = new List<double>();
            noTimeoutResponses.AddRange(_responseTimes);
            noTimeoutResponses.RemoveAll(x => x == 0d);
            Console.Write("{0}ms",
                _responseTimes.Count > 0 && noTimeoutResponses.Count > 0 ? Math.Round(noTimeoutResponses.Max(), 1) : 0);

            // Update RTT label
            Console.SetCursorPosition(_rttLabelX, _rttLabelY);
            Console.Write(blankLabel);
            Console.CursorLeft = Console.CursorLeft - 8;
            Console.Write("{0:0.0}ms", results.CurrTime);

            // Update time label
            Console.SetCursorPosition(_timeLabelX, _timeLabelY);
            Console.Write(blankLabel + "        ");
            Console.CursorLeft = Console.CursorLeft - 16;
            Console.Write("{0:hh\\:mm\\:ss}", results.TotalRunTime);

            // Reset cursor to starting position
            Console.SetCursorPosition(cursorPositionX, cursorPositionY);
        }
    }
}
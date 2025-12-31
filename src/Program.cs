/**************************************************************************
 * PowerPing - Advanced command line ping tool
 * Copyright (c) 2026 Matthew Carney [matthewcarney64@gmail.com]
 * https://github.com/Killeroo/PowerPing
 *************************************************************************/

namespace PowerPing
{
    internal static class Program
    {
        private static readonly CancellationTokenSource _cancellationTokenSource = new ();
        private static DisplayConfiguration _displayConfiguration = new ();
        private static LogMessageHandler? _logMessageHandler = null;
        private static ConsoleMessageHandler? _consoleMessageHandler = null;

        public const bool BETA = false;

        /// <summary>
        /// Main entry point of PowerPing
        /// Parses arguments and runs operations
        /// </summary>
        /// <param name="args">Program arguments</param>
        private static void Main(string[] args)
        {
            PingAttributes parsedAttributes = new PingAttributes();

            // Show current version info
            //Display.Version();

            // Check if no arguments
            if (args.Length == 0)
            {
                ConsoleDisplay.Help();
                return;
            }

            // Parse command line arguments
            if (!CommandLine.Parse(args, ref parsedAttributes, ref _displayConfiguration))
            {
                Helper.ErrorAndExit("Problem parsing arguments, use \"PowerPing /help\" or \"PowerPing /?\" for help.");
            }

            Helper.RequireInput = _displayConfiguration.RequireInput;

            // Find address/host in arguments
            List<string> parsedAddresses = new List<string>();
            if (parsedAttributes.Operation != PingOperation.Whoami &&
                parsedAttributes.Operation != PingOperation.Listen)
            {
                if (!CommandLine.FindAddresses(args, ref parsedAddresses))
                {
                    Helper.ErrorAndExit("Could not find correctly formatted address, please check and try again");
                }
            }

            // Perform DNS lookup on inputted address
            // inputtedAttributes.ResolvedAddress = Lookup.QueryDNS(inputtedAttributes.InputtedAddress, inputtedAttributes.UseICMPv4 ? AddressFamily.InterNetwork : AddressFamily.InterNetworkV6);

            // Add Control C event handler
            if (parsedAttributes.Operation != PingOperation.Whoami &&
                parsedAttributes.Operation != PingOperation.Location &&
                parsedAttributes.Operation != PingOperation.Whois)
            {
                Console.CancelKeyPress += new ConsoleCancelEventHandler(ExitHandler);
            }

            // Set configuration
            ConsoleDisplay.Configuration = _displayConfiguration;
            ConsoleDisplay.CancellationToken = _cancellationTokenSource.Token;


            // Select correct function using opMode
            switch (parsedAttributes.Operation)
            {
#pragma warning disable CS8604 // InputtedAddress has to be equal to something so disable null reference warning for this.
                case PingOperation.Listen: RunListenOperation(args, parsedAttributes); break;
                case PingOperation.Location: RunLocationOperation(parsedAttributes.InputtedAddress); break;
                case PingOperation.Whoami: RunWhoAmIOperation(); break;
                case PingOperation.Whois: RunWhoisOperation(parsedAttributes.InputtedAddress); break;
                case PingOperation.Graph: RunGraphOperation(parsedAttributes.InputtedAddress, _cancellationTokenSource.Token); break;
                case PingOperation.Flood: RunFloodOperation(parsedAttributes.InputtedAddress, _cancellationTokenSource.Token); break;
                case PingOperation.Scan: RunScanOperation(parsedAttributes, _cancellationTokenSource.Token); break;
                case PingOperation.Normal: RunNormalPingOperation(parsedAttributes, parsedAddresses, _cancellationTokenSource.Token); break;
#pragma warning restore CS8604 // Possible null reference argument.

                default:
                    Helper.ErrorAndExit("Could not determine ping operation");
                    break;
            }

            // Reset console colour
            ConsoleDisplay.ResetColor();
            try { Console.CursorVisible = true; } catch (Exception) { }
        }

        /// <summary>
        /// Runs when Exit or Cancel event fires (normally when Ctrl-C)
        /// is pressed. Used to clean up and stop operations when exiting
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private static void ExitHandler(object? sender, ConsoleCancelEventArgs args)
        {
            // Cancel termination
            args.Cancel = true;

            // Request currently running job to finish up
            _cancellationTokenSource.Cancel();

            // Reset colour on exit
            Console.ResetColor();
        }

        private static void RunNormalPingOperation(
            PingAttributes baseAttributes,
            List<string> parsedAddresses,
            CancellationToken cancellationToken)
        {
            if (baseAttributes == null)
            {
                return;
            }

            var sendPing = (PingAttributes attributes) =>
            {
                Ping p = new Ping(attributes, cancellationToken);
                
                if (attributes.EnableFileLogging)
                {
                    // Setup the path we are going to save the log to
                    // (generate the name if needed)
                    attributes.LogFilePath = LogFile.SetupPath(attributes.LogFilePath, attributes.InputtedAddress);
                    _logMessageHandler = new LogMessageHandler(attributes.LogFilePath, _displayConfiguration);

                    // Add callbacks for logging to a file
                    // These need to be first.. so they get run first
                    p.OnStart += _logMessageHandler.OnStart;
                    p.OnFinish += _logMessageHandler.OnFinish;
                    p.OnTimeout += _logMessageHandler.OnTimeout;
                    p.OnRequest += _logMessageHandler.OnRequest;
                    p.OnReply += _logMessageHandler.OnReply;
                    p.OnError += _logMessageHandler.OnError;
                }

                // Add handler to display ping events
                _consoleMessageHandler =
                    new ConsoleMessageHandler(_displayConfiguration, _cancellationTokenSource.Token);
                _consoleMessageHandler.Register(p);

                // Send the pings!
                p.Send();
            };
            
            
            if (parsedAddresses.Count > 1)
            {
                Thread?[] threads = new Thread[parsedAddresses.Count];
                for (int index = 0; index < threads.Length; index++)
                {
                    threads[index] = new (() =>
                    {
                        PingAttributes modifiedAttrs = new PingAttributes(baseAttributes);
                        modifiedAttrs.InputtedAddress = parsedAddresses[index];
                        sendPing(modifiedAttrs);
                    });
                    threads[index].IsBackground = true;
                    threads[index].Start();
                    Thread.Sleep(100);
                }

                do
                {
                    
                    Thread.Sleep(1);

                } while (threads.Any(x => x != null && x.IsAlive));
            }
            else
            {
                baseAttributes.InputtedAddress = parsedAddresses.First();
                sendPing(baseAttributes);
            }
        }

        private static void RunFloodOperation(string address, CancellationToken cancellationToken)
        {
            Flood f = new Flood();

            f.Start(address, cancellationToken);
        }

        private static void RunGraphOperation(string address, CancellationToken cancellationToken)
        {
            Graph g = new Graph(address, cancellationToken);

            g.Start();
        }

        private static void RunWhoisOperation(string address)
        {
            Lookup.QueryWhoIs(address);

            if (_displayConfiguration.RequireInput)
            {
                Helper.WaitForUserInput();
            }
        }

        private static void RunWhoAmIOperation()
        {
            Console.WriteLine(Lookup.GetAddressLocationInfo("", true));
            if (_displayConfiguration.RequireInput)
            {
                Helper.WaitForUserInput();
            }
        }

        private static void RunListenOperation(string[] args, PingAttributes attributes)
        {
            // If we find an address then pass it to listen, otherwise start it without one
            if (CommandLine.FindAddress(args, ref attributes))
            {
                Listen.Start(_cancellationTokenSource.Token, attributes.InputtedAddress);
            }
            else
            {
                Listen.Start(_cancellationTokenSource.Token);
            }
        }

        private static void RunLocationOperation(string address)
        {
            Console.WriteLine(Lookup.GetAddressLocationInfo(address, true));
            if (_displayConfiguration.RequireInput)
            {
                Helper.WaitForUserInput();
            }
        }

        private static void RunScanOperation(PingAttributes attributes, CancellationToken cancellationToken)
        {
            if (attributes.EnableFileLogging)
            {
                attributes.LogFilePath = LogFile.SetupPath(attributes.LogFilePath, attributes.InputtedAddress);
                _logMessageHandler = new LogMessageHandler(attributes.LogFilePath, _displayConfiguration);

                Scan.OnScanFinished += _logMessageHandler.OnScanFinished;
            }

            // Add callbacks for console display
            _consoleMessageHandler = new ConsoleMessageHandler(_displayConfiguration, _cancellationTokenSource.Token);
            Scan.OnScanProgress += _consoleMessageHandler.OnScanProgress;
            Scan.OnScanFinished += _consoleMessageHandler.OnScanFinished;

            Scan.Start(attributes.InputtedAddress, cancellationToken);
        }
    }
}
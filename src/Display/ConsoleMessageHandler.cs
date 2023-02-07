namespace PowerPing
{
    public class ConsoleMessageHandler
    {
        public DisplayConfiguration DisplayConfig = new DisplayConfiguration();
        public PingAttributes Attributes = new PingAttributes();
        public CancellationToken Token = new CancellationToken();

        public ConsoleMessageHandler(DisplayConfiguration config, CancellationToken token)
        {
            DisplayConfig = config;
            Token = token;
        }

        public void OnError(PingError error)
        {
            if (error.Fatal)
            {
                ConsoleDisplay.Fatal(error.Message, error.Exception);

                // Exit on fatal error
                Helper.ExitWithError();
            }
            else
            {
                ConsoleDisplay.Error(error.Message, error.Exception);
            }
        }

        public void OnFinish(PingResults results)
        {
            ConsoleDisplay.PingResults(Attributes, results);
        }

        public void OnReply(PingReply response)
        {
            // Determine what form the response address is going to be displayed in
            // TODO: Move this when lookup refactor is done
            string responseAddress = response.Endpoint.ToString();
            if (DisplayConfig.UseResolvedAddress)
            {
                // Returned address normally have port at the end (eg 8.8.8.8:0) so we need to remove that before trying to query the DNS
                string responseIP = responseAddress.ToString().Split(':')[0];

                // Resolve the ip and store as the response address
                responseAddress = Helper.RunWithCancellationToken(() => Lookup.QueryHost(responseIP), Token);
            }
            else if (DisplayConfig.UseInputtedAddress)
            {
                responseAddress = Attributes.InputtedAddress;
            }

            ConsoleDisplay.ReplyPacket(
                response.Packet,
                responseAddress,
                response.SequenceNumber,
                response.RoundTripTime,
                response.BytesRead);

            if (Attributes.BeepMode == 2)
            {
                try { Console.Beep(); }
                catch (Exception) { } // Silently continue if Console.Beep errors
            }
        }

        public void OnRequest(PingRequest request)
        {
            ConsoleDisplay.RequestPacket(
                request.Packet,
                (DisplayConfig.UseInputtedAddress | DisplayConfig.UseResolvedAddress ? Attributes.InputtedAddress : Attributes.ResolvedAddress),
                request.SequenceNumber);
        }

        public void OnStart(PingAttributes attributes)
        {
            // Cache the PingAttributes at this point as Ping has modified the inputted and resolved addresses
            Attributes = attributes;

            ConsoleDisplay.PingIntroMsg(attributes);
        }

        public void OnTimeout(PingTimeout timeout)
        {
            ConsoleDisplay.Timeout(timeout.SequenceNumber);

            if (Attributes.BeepMode == 1)
            {
                try { Console.Beep(); }
                catch (Exception) { }
            }
        }
    }
}
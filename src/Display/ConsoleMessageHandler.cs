using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPing
{
    public class ConsoleMessageHandler : IPingMessageHandler
    {
        public DisplayConfiguration DisplayConfig = new DisplayConfiguration();
        public PingAttributes Attributes = new PingAttributes();
        public CancellationToken Token = new CancellationToken();

        public ConsoleMessageHandler(DisplayConfiguration config, CancellationToken token)
        {
            DisplayConfig = config;
            Token = token;
        }

        public void OnError(string message, Exception e = null, bool fatal = false)
        {
            string errorText = (e != null ? $"{message} ({e.GetType().Name})" : message);

            if (fatal)
            {
                ConsoleDisplay.Fatal(errorText);
                
                // Exit on fatal error
                Helper.ExitWithError();    
            }
            else
            {
                ConsoleDisplay.Error(errorText);
            }
        }

        public void OnFinish(PingResults results)
        {
            ConsoleDisplay.PingResults(Attributes, results);
        }

        public void OnReply(PingReply response)
        {
            // Determine what form the response address is going to be displayed in
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
            if (DisplayConfig.ShowRequests)
            {
                ConsoleDisplay.RequestPacket(
                    request.Packet, 
                    (DisplayConfig.UseInputtedAddress | DisplayConfig.UseResolvedAddress ? Attributes.InputtedAddress : Attributes.ResolvedAddress), 
                    request.SequenceNumber);
            }
        }

        public void OnResultsUpdate(PingResults results)
        {
            
        }

        public void OnStart(PingAttributes attributes)
        {

            // TODO: I think this part is bullshit, check later
            if (DisplayConfig.UseResolvedAddress)
            {
                try
                {
                    Attributes.InputtedAddress = Helper.RunWithCancellationToken(() => Lookup.QueryHost(Attributes.ResolvedAddress), Token);
                }
                catch (OperationCanceledException) { }

                if (Attributes.InputtedAddress == "")
                {
                    // If reverse lookup fails just display whatever is in the address field
                    Attributes.InputtedAddress = Attributes.ResolvedAddress;
                }
            }

            ConsoleDisplay.PingIntroMsg(Attributes);
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

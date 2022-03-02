/**************************************************************************
 * PowerPing - Advanced command line ping tool
 * Copyright (c) 2022 Matthew Carney [matthewcarney64@gmail.com]
 * https://github.com/Killeroo/PowerPing
 *************************************************************************/

namespace PowerPing
{
    public interface IPingMessageHandler
    {
        public void OnRequest(PingRequest request);
        public void OnReply(PingReply response);
        public void OnTimeout(PingTimeout timeout);
        public void OnResultsUpdate(PingResults results);
        public void OnStart(PingAttributes attributes);
        public void OnFinish(PingResults results);
        public void OnError(string message, Exception e = null, bool fatal = false);

    }
}

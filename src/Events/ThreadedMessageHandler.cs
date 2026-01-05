
namespace PowerPing
{
    public class ThreadedMessageHandler : PingEventHandlerBase
    {
        public DisplayConfiguration DisplayConfig = new();
        public PingAttributes Attributes = new();
        public CancellationToken Token = new();
        
        private object _lock = new();
        private Stack<PingEventData> _eventData = new();
        private List<PingEventHandlerBase> _handlers = new();

        public void RegisterEventHandler(PingEventHandlerBase eventHandler)
        {
            _handlers.Add(eventHandler);
        }
        
        public void FlushMessages()
        {
            lock (_lock)
            {
                while (_eventData.Count > 0)
                {
                    var currentEventData = _eventData.Pop();
                    foreach (PingEventHandlerBase handler in _handlers)
                    {
                        switch (currentEventData)
                        {
                            case PingAttributes attributes: handler.OnStart(attributes); break;
                            case PingResults results: handler.OnFinish(results); break;
                            case PingTimeout timeout: handler.OnTimeout(timeout); break;
                            case PingRequest request: handler.OnRequest(request); break;
                            case PingReply reply: handler.OnReply(reply); break;
                        }
                    }
                }
            }
        }

        public override void OnError(PingError error)
        {
            lock (_lock)
            {
                _eventData.Push(error);
            }
        }
        
        public override void OnFinish(PingResults results)
        {
            lock (_lock)
            {
                _eventData.Push(results);
            }
        }

        public override void OnReply(PingReply response)
        {
            lock (_lock)
            {
                _eventData.Push(response);
            }
        }

        public override void OnRequest(PingRequest request)
        {
            lock (_lock)
            {
                _eventData.Push(request);
            }
        }

        public override void OnStart(PingAttributes attributes)
        {
            lock (_lock)
            {
                _eventData.Push(attributes);
            }
        }

        public override void OnTimeout(PingTimeout timeout)
        {
            lock (_lock)
            {
                _eventData.Push(timeout);
            }
        }
    }
}
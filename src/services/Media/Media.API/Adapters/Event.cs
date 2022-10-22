namespace Media.API.Adapters
{
    using System;
    using Microsoft.VisualBasic;

    public static class EventType
    {
        // domain event type
        public static readonly string EventDomain = "EVENT_DOMAIN";
        // integration event type
        public static readonly string EventIntegration = "EVENT_INTEGRATION";
    }

    public class MediaEventMetadata
    {
        public Guid EventId { get; }
        public string DispatchTime { get; }
        // event source
        public string ServiceName { get; }
        public string EventType { get; }

        public MediaEventMetadata(string eventType, string serviceName = "media")
        {
            this.EventId = new Guid();
            this.DispatchTime = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString();
            this.ServiceName = Strings.UCase(serviceName);
            this.EventType = eventType;
        }
    }

    public record MediaEvent(
        MediaEventMetadata Metadata,
        string Content
    );
}

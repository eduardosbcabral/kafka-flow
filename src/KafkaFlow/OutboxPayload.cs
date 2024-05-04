namespace KafkaFlow;

public class OutboxPayload<T>
{
    public T Data { get; set; } = default!;
    public string Domain { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public Guid Key { get; set; }
    public string Version { get; set; } = string.Empty;
    public DateTime RaisedAt { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string AggregateId { get; set; } = string.Empty;
    public Guid TraceKey { get; set; }

    public OutboxPayload() { }
}

namespace KafkaFlow.Sample.Messages;

public class KafkaMessage<TPayload> 
{
    public KafkaSchema Schema { get; set; } = new();
    public OutboxPayload<TPayload> Payload { get; set; } = new();
    
    public KafkaMessage() { }
}

public class KafkaSchema
{
    public string Type { get; set; } = string.Empty;
    public bool Optional { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Version { get; set; }

    public KafkaSchema() { }
}
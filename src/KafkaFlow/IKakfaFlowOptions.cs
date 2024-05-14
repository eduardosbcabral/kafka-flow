namespace KafkaFlow;

public interface IKakfaFlowOptions
{
    public bool Validate(out string errorMessage);
}

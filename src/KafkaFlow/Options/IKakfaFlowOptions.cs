namespace KafkaFlow.Options;

public interface IKakfaFlowOptions
{
    public bool Validate(out string errorMessage);
}

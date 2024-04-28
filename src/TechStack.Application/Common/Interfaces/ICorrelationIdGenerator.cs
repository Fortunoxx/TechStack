namespace TechStack.Application.Common.Interfaces;

public interface ICorrelationIdGenerator
{
    string Get();
    
    void Set(string correlationId);
}
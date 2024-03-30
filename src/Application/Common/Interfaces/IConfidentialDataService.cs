using MassTransit.Initializers.PropertyProviders;
using Microsoft.Extensions.Compliance.Classification;
using Microsoft.Extensions.Compliance.Redaction;
using Microsoft.Extensions.Logging;

namespace TechStack.Application.Common.Interfaces;

public interface IConfidentialDataService
{
    ConfidentialData CreateEntry();
}

public class ConfidentialDataService : IConfidentialDataService
{
    public ConfidentialData CreateEntry() => new(
        "X999999006", 
        new DateTime(2000, 1, 1), 
        "test@mail.org", 
        "DE1200001111222233", 
        "PBNKDEFF"
    );
}

public record ConfidentialData(
    [SensitiveData] string ProtectedId, // strange, in serilog this does not exist
    [PiiData] DateTime DateOfBirth, // strange, in serilog this does not exist
    string Email,
    string Iban,
    string Bic);

public static partial class Logging
{
    [LoggerMessage(LogLevel.Information, "Created confidential data")]
    public static partial void LogConfidentialDataCreated(this ILogger logger,
        [LogProperties] ConfidentialData data);
}

public static class DataTaxonomy
{
    public static string TaxonomyName { get; } = typeof(DataTaxonomy).FullName!;

    public static DataClassification SensitiveData { get; } = new(TaxonomyName, nameof(SensitiveData));

    public static DataClassification PiiData { get; } = new(TaxonomyName, nameof(PiiData));
}

public class SensitiveDataAttribute : DataClassificationAttribute
{
    public SensitiveDataAttribute() : base(DataTaxonomy.SensitiveData) { }
}

public class PiiDataAttribute : DataClassificationAttribute
{
    public PiiDataAttribute() : base(DataTaxonomy.PiiData) { }
}

public class StarRedactor : Redactor
{
    public override int GetRedactedLength(ReadOnlySpan<char> input)
        => input.Length;

    public override int Redact(ReadOnlySpan<char> source, Span<char> destination)
    {
        destination.Fill('*');
        return destination.Length;
    }
}

public class RedactedRedactor : Redactor
{
    public override int GetRedactedLength(ReadOnlySpan<char> input)
        => input.Length;

    public override int Redact(ReadOnlySpan<char> source, Span<char> destination)
    {
        destination = "[redacted]".ToCharArray();
        return destination.Length;
    }
}
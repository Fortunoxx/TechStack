namespace TechStack.Application.UnitTests.Common;

using System.Linq;

public class UnitTestAutoMapperException(string message, IEnumerable<(Type TSource, Type TDestination)> untestedMappings) : Exception(message)
{
    public override string Message
    {
        get
        {
            if (untestedMappings != null)
            {
                var message = base.Message;
                message += $"{Environment.NewLine}==================";
                message += $"{Environment.NewLine}Found untested mappings. Please add unit tests for these mappings and make sure to use {nameof(UnitTestAutoMapper)}";
                message += $"{Environment.NewLine}==================";
                message += $"{Environment.NewLine}Untested Mappings:";
                message += $"{Environment.NewLine}==================";
                message += $"{Environment.NewLine}{string.Join(Environment.NewLine, untestedMappings.Select(x => $"{x.TSource} -> {x.TDestination}"))}";
                message += $"{Environment.NewLine}==================";
                return message;
            }

            return Message;
        }
    }
}

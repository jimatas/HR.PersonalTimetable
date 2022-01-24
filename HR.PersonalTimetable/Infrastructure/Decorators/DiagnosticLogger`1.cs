using Developist.Core.Cqrs;
using Developist.Core.Cqrs.Commands;
using Developist.Core.Utilities;

using Microsoft.Extensions.Logging;

using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace HR.PersonalTimetable.Infrastructure.Decorators
{
    public class DiagnosticLogger<TCommand> : ICommandHandlerWrapper<TCommand>
        where TCommand : ICommand
    {
        private static JsonSerializerOptions serializerOptions;
        private readonly ILogger logger;

        public DiagnosticLogger(ILogger<DiagnosticLogger<TCommand>> logger)
        {
            this.logger = Ensure.Argument.NotNull(() => logger);
        }

        public async Task HandleAsync(TCommand command, HandlerDelegate next, CancellationToken cancellationToken)
        {
            logger.LogDebug("Executing {CommandType} command {CommandDetails}", command.GetType().Name, JsonSerialize(command));

            var stopwatch = Stopwatch.StartNew();
            await next().ConfigureAwait(false);
            stopwatch.Stop();

            logger.LogDebug("Executed {CommandType} command in {Milliseconds} ms.", command.GetType().Name, stopwatch.ElapsedMilliseconds);
        }

        private static object JsonSerialize(object value)
        {
            return new DeferredToString(() => JsonSerializer.Serialize(value, CreateDefaultSerializerOptions()));

            static JsonSerializerOptions CreateDefaultSerializerOptions() => serializerOptions ??= new()
            {
                WriteIndented = false,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
        }
    }
}

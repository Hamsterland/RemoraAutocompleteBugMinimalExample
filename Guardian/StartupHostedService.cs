using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Remora.Discord.Gateway;
using Remora.Discord.Gateway.Results;
using Remora.Results;
using Serilog;

namespace Guardian.Services
{
    public class StartupHostedService : IHostedService
    {
        private readonly ILogger _logger;
        private readonly DiscordGatewayClient _client;

        public StartupHostedService(
            ILogger logger,
            DiscordGatewayClient client)
        {
            _logger = logger;
            _client = client;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var result = await _client.RunAsync(cancellationToken);

            if (result.IsSuccess)
            {
                return;
            }

            switch (result.Error)
            {
                case ExceptionError exe:
                {
                    _logger.Error(exe.Exception, "Exception During Gateway Connection: {ExceptionMessage}", exe.Message);
                    break;
                }
                case GatewayWebSocketError:
                case GatewayDiscordError:
                {
                    _logger.Error("Gateway Error: {Message}", result.Error.Message);
                    break;
                }
                default:
                {
                    _logger.Error("Unknown Error: {Message}", result.Error.Message);
                    break;
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
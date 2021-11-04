// See https://aka.ms/new-console-template for more information

using System.Threading.Tasks;
using Serilog;
using Guardian.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Remora.Commands.Extensions;
using Remora.Discord.Commands.Extensions;
using Remora.Discord.Commands.Services;
using Remora.Discord.Gateway.Extensions;
using Guardian.Services;
using Remora.Discord.Core;
using Serilog.Sinks.SystemConsole.Themes;

namespace Guardian
{
    public class Program
    {
        private const string _token = "";
        private const string _debugSeverID = "";
    
        public static async Task Main(string[] args)
        {
            var host = CreateHost();
            await host.RunConsoleAsync();
        }

        private static IHostBuilder CreateHost()
        {
            return Host.CreateDefaultBuilder()
                .UseSerilog(InitialiseSerilog)
                .ConfigureServices(async (context, collection) => await InitialiseSerivces(context, collection));
        }

        private static void InitialiseSerilog(HostBuilderContext context, LoggerConfiguration configuration)
        {
            configuration
                .Enrich.FromLogContext()
                .MinimumLevel.Information()
                .WriteTo.Console(theme: SystemConsoleTheme.Literate);
        }
    
        private static async Task InitialiseSerivces(HostBuilderContext context, IServiceCollection collection)
        {
            collection
                .AddHostedService<StartupHostedService>()
                .AddCommandGroup<NoteCommands>()
                .AddInteractionResponder(x => x.SuppressAutomaticResponses = true)
                .AddDiscordGateway(_ => _token)
                .AddDiscordCommands(true)
                .AddAutocompleteProvider<ReasonAutocompleteProvider>();
        
            await SupportSlashCommands(collection);
        }
    
        private static async Task SupportSlashCommands(IServiceCollection collection)
        {
            var services = collection.BuildServiceProvider(true);
        
            var logger = services.GetRequiredService<ILogger>();
            var slashService = services.GetRequiredService<SlashService>();

            var slashSupportResult = slashService.SupportsSlashCommands();
            if (!slashSupportResult.IsSuccess)
            {
                logger.Warning("The registered commands do not support Slash Commands: {Reason}", slashSupportResult.Error.Message);
                return;
            }

            Snowflake.TryParse(_debugSeverID, out var snowflake);
        
            var updateSlashResult = await slashService.UpdateSlashCommandsAsync(snowflake);
            if (!updateSlashResult.IsSuccess)
            {
                logger.Warning("Failed to update Slash Commands: {Reason}", updateSlashResult.Error.Message);
                return;
            }

            logger.Information("Initialised Slash Commands");
        }
    }
}
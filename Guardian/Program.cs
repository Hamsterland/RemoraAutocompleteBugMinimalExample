// See https://aka.ms/new-console-template for more information

global using System.Threading;
global using System.Threading.Tasks;
global using Serilog;
using Guardian.Commands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Remora.Commands.Extensions;
using Remora.Commands.Groups;
using Remora.Discord.Commands.Extensions;
using Remora.Discord.Commands.Services;
using Remora.Discord.Gateway.Extensions;
using Guardian.Services;
using Serilog.Sinks.SystemConsole.Themes;

namespace Guardian;

public class Program
{
    public static async Task Main(string[] args)
    {
        var host = CreateHost();
        await host.RunConsoleAsync();
    }

    private static IHostBuilder CreateHost()
    {
        return Host.CreateDefaultBuilder()
            .UseSerilog(InitialiseSerilog)
            .ConfigureAppConfiguration(InitialiseConfiguration)
            .ConfigureServices(async (context, collection) => await InitialiseSerivces(context, collection));
    }

    private static void InitialiseSerilog(HostBuilderContext context, LoggerConfiguration configuration)
    {
        configuration
            .Enrich.FromLogContext()
            .MinimumLevel.Information()
            .WriteTo.Console(theme: SystemConsoleTheme.Literate);
    }

    private static void InitialiseConfiguration(IConfigurationBuilder builder)
    {
        builder
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", false, true);
    }

    private static async Task InitialiseSerivces(HostBuilderContext context, IServiceCollection collection)
    {
        AddCoreServices(context, collection);
        AddFeatureServices(collection);
        AddCommandGroups(collection);
        await SupportSlashCommands(collection);
    }

    private static void AddCoreServices(HostBuilderContext context, IServiceCollection collection)
    {
        collection
            .AddAutoMapper(typeof(Program))
            .AddInteractionResponder(x => x.SuppressAutomaticResponses = true)
            .AddDiscordGateway(_ => context.Configuration["Discord:Token"])
            .AddDiscordCommands(true)
            .AddAutocompleteProvider<ReasonAutocompleteProvider>();
    }

    private static void AddFeatureServices(IServiceCollection collection)
    {
        collection.AddHostedService<StartupHostedService>();
    }

    private static void AddCommandGroups(IServiceCollection collection)
    {
        var commandGroupTypeInfos = typeof(Program)
            .Assembly
            .DefinedTypes
            .Where(x => typeof(CommandGroup).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract);

        foreach (var commandGroupTypeInfo in commandGroupTypeInfos)
        {
            collection.AddCommandGroup(commandGroupTypeInfo);
        }
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

        var updateSlashResult = await slashService.UpdateSlashCommandsAsync(Constants.GuildId);
        if (!updateSlashResult.IsSuccess)
        {
            logger.Warning("Failed to update Slash Commands: {Reason}", updateSlashResult.Error.Message);
            return;
        }

        logger.Information("Initialised Slash Commands");
    }
}
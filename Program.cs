using BettingBot;
using BettingBot.Database;
using BettingBot.Services;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Services;
using CliWrap;

const string ServiceName = "Telegram betting bot service";

if (args is { Length: 1 })
{
    try
    {
        string executablePath =
            Path.Combine(AppContext.BaseDirectory, "BettingBot.exe");

        if (args[0] is "/Install")
        {
            await Cli.Wrap("sc")
                .WithArguments(new[] { "create", ServiceName, $"binPath={executablePath}", "start=auto" })
                .ExecuteAsync();
        }
        else if (args[0] is "/Uninstall")
        {
            await Cli.Wrap("sc")
                .WithArguments(new[] { "stop", ServiceName })
                .ExecuteAsync();

            await Cli.Wrap("sc")
                .WithArguments(new[] { "delete", ServiceName })
                .ExecuteAsync();
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex);
    }

    return;
}



IHost host = Host.CreateDefaultBuilder(args)
    .UseWindowsService(options =>
    {
        options.ServiceName = ServiceName;
    })
    .ConfigureServices((context, services) =>
    {
        services.Configure<BotConfiguration>(
            context.Configuration.GetSection(BotConfiguration.Configuration));

        services.AddHttpClient("telegram_bot_client")
                .AddTypedClient<ITelegramBotClient>((httpClient, sp) =>
                {
                    BotConfiguration? botConfig = sp.GetConfiguration<BotConfiguration>();
                    TelegramBotClientOptions options = new(botConfig.BotToken);
                    return new TelegramBotClient(options, httpClient);
                });

        services.AddScoped<UpdateHandler>();
        services.AddScoped<ReceiverService>();
        services.AddHostedService<PollingService>();

        services.AddDbContext<BotContext>(o =>
        {
            o.UseNpgsql();
        },
        ServiceLifetime.Transient);

    })
    .Build();

using var serviceScope = host.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
var userContext = serviceScope.ServiceProvider.GetRequiredService<BotContext>();
userContext.Database.Migrate();

await host.RunAsync();
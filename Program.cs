using BettingBot;
using BettingBot.Database;
using BettingBot.Services;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Services;

IHost host = Host.CreateDefaultBuilder(args)
    .UseWindowsService(options =>
    {
        options.ServiceName = "Telegram betting bot service";
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
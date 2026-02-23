/* PSEUDOCODE / PLAN
   - Problem: Calling builder.Services.BuildServiceProvider() creates duplicate singleton instances.
   - Goal: Validate the registered ITelegramBotClient early without calling BuildServiceProvider on the IServiceCollection.
   - Approach:
     1. Remove any call to builder.Services.BuildServiceProvider().
     2. Build the application host once with builder.Build().
     3. Use the built application's IServiceProvider (app.Services) to resolve ITelegramBotClient.
        - Resolving from app.Services uses the correct root provider and avoids duplicate singleton instances.
     4. Perform the diagnostic validation immediately after Build and before app.Run (hosted services start at Run).
     5. Preserve original diagnostic behavior and throw if validation fails.
*/

using CommandBot.Interfaces;
using CommandBot.Models;
using CommandBot.Services;
using CommandBot.Workers;
using CbTsSa_Shared.DBModels;
using CbTsSa_Shared.Interfaces;
using CbTsSa_Shared.Models;
using CbTsSa_Shared.CbTsSaConstants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
using WhatsappBusiness.CloudApi.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Ensure console logging and raise log level for diagnostic categories so
// our command parsing/handler logs appear in production (Heroku) without
// requiring environment changes.
builder.Logging.AddConsole();
// Reduce CommandRegistry trace noise now that we confirmed pattern matching works
builder.Logging.AddFilter("CommandBot.Services.CommandRegistry", Microsoft.Extensions.Logging.LogLevel.Information);
builder.Logging.AddFilter("CommandBot.Commands.UpdateUserFieldCommand", Microsoft.Extensions.Logging.LogLevel.Information);
builder.Logging.AddFilter("CommandBot.Commands.FieldHandlers", Microsoft.Extensions.Logging.LogLevel.Information);

// Configuration sources
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

var configuration = builder.Configuration;

// Bootstrap logger for logging during host building/configuration
var bootstrapLogger = LoggerFactory.Create(lb => lb.AddConsole()).CreateLogger("Bootstrap");

// Register default IHttpClientFactory early so services/commands that require it can be constructed.
// Also register the named WhatsApp client later after whatsappConfig is validated.
builder.Services.AddHttpClient();

// ---- PayFast named HttpClient (disable auto-redirect to capture 302 Location header) ----
builder.Services.AddHttpClient("PayFast", client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
})
.ConfigurePrimaryHttpMessageHandler(() =>
{
    return new HttpClientHandler
    {
        AllowAutoRedirect = false
    };
});

// ---- HostBusiness binding (Heroku env first) ----
builder.Services.Configure<HostBusiness>(options =>
{
    var envCell = Environment.GetEnvironmentVariable("HOSTBUSINESS_CELLNUMBER");
    var envBaseUrl = Environment.GetEnvironmentVariable("HOSTBUSINESS_BASEURL");

    // Use environment variable if set and not empty
    if (!string.IsNullOrWhiteSpace(envCell))
        options.Cellnumber = envCell;
    else
        options.Cellnumber = configuration["HostBusiness:Cellnumber"] ?? "-1";

    if (!string.IsNullOrWhiteSpace(envBaseUrl))
        options.BaseUrl = envBaseUrl;
    else
        options.BaseUrl = configuration["HostBusiness:BaseUrl"] ?? "http://localhost:5001";
});

// Validate HostBusiness after binding
builder.Services.AddOptions<HostBusiness>()
    .Validate(o =>
        long.TryParse(o.Cellnumber, out _) &&
        Uri.TryCreate(o.BaseUrl, UriKind.Absolute, out _),
        "HostBusiness: Cellnumber must be numeric and BaseUrl must be a valid absolute URL.")
    .PostConfigure(o =>
    {
        bootstrapLogger.LogInformation("HostBusiness resolved: Cellnumber='{Cellnumber}', BaseUrl='{BaseUrl}'", o.Cellnumber, o.BaseUrl);
    });

// ---- JWT config ----
// Existing binding already supports environment variables because AddEnvironmentVariables() is enabled.
// Keep the binding but add diagnostic logging showing whether env vars are present.
var jwtSection = configuration.GetSection("JwtIssueConfig");
builder.Services.AddOptions<JwtIssueConfig>()
    .Bind(jwtSection)
    .Validate(opt =>
        !string.IsNullOrWhiteSpace(opt.Key) &&
        !string.IsNullOrWhiteSpace(opt.Issuer) &&
        opt.Audiences?.Count > 0,
        "Invalid JWT config");

// Diagnostic: show if env override present
bootstrapLogger.LogInformation("=== JWT Config Debug ===");
bootstrapLogger.LogInformation("JwtIssueConfig:Key present: {Present}", !string.IsNullOrWhiteSpace(configuration["JwtIssueConfig:Key"]));
bootstrapLogger.LogInformation("JwtIssueConfig:Issuer present: {Present}", !string.IsNullOrWhiteSpace(configuration["JwtIssueConfig:Issuer"]));
bootstrapLogger.LogInformation("========================");

// ---- RabbitMQ settings ----
// Prefer environment variables (Heroku) but fallback to appsettings
var rabbitSection = configuration.GetSection("RabbitMQ");
var rabbitSettings = rabbitSection.Get<RabbitMqSettings>() ?? new RabbitMqSettings();

// Env-first overrides
var envRabbitConnection = Environment.GetEnvironmentVariable("RabbitMQ__ConnectionString")
                          ?? Environment.GetEnvironmentVariable("RABBITMQ__ConnectionString");
if (!string.IsNullOrWhiteSpace(envRabbitConnection))
{
    rabbitSettings.ConnectionString = envRabbitConnection;
}
else
{
    var envHost = Environment.GetEnvironmentVariable("RabbitMQ__Host");
    if (!string.IsNullOrWhiteSpace(envHost))
        rabbitSettings.Host = envHost;

    var envUser = Environment.GetEnvironmentVariable("RabbitMQ__Username");
    if (!string.IsNullOrWhiteSpace(envUser))
        rabbitSettings.Username = envUser;

    var envPass = Environment.GetEnvironmentVariable("RabbitMQ__Password");
    if (!string.IsNullOrWhiteSpace(envPass))
        rabbitSettings.Password = envPass;

    var envPort = Environment.GetEnvironmentVariable("RabbitMQ__Port");
    if (int.TryParse(envPort, out var parsedPort))
        rabbitSettings.Port = parsedPort;
}

if (!string.IsNullOrWhiteSpace(rabbitSettings.ConnectionString))
{
    bootstrapLogger.LogInformation("RabbitMQ: Using CloudAMQP connection string (from env or config)");
}
else
{
    bootstrapLogger.LogInformation("RabbitMQ Host: {Host}, User: {User}", rabbitSettings.Host, rabbitSettings.Username);
}

builder.Services
    .AddOptions<RabbitMqSettings>()
    .Bind(rabbitSection)
    .Validate(opt =>
    {
        if (!string.IsNullOrWhiteSpace(opt.ConnectionString))
            return true;

        return
            !string.IsNullOrWhiteSpace(opt.Host) &&
            !string.IsNullOrWhiteSpace(opt.Username) &&
            !string.IsNullOrWhiteSpace(opt.Password) &&
            opt.Port > 0;
    }, "Invalid RabbitMQ configuration");

// ---- DbContext ----
builder.Services.AddDbContext<AppDbContext>(options =>
{
    var herokuUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
    string connectionString;

    if (!string.IsNullOrWhiteSpace(herokuUrl))
    {
        // Heroku Postgres → convert URL to Npgsql connection string
        var databaseUri = new Uri(herokuUrl);
        var userInfo = databaseUri.UserInfo.Split(':');

        connectionString =
            $"Host={databaseUri.Host};" +
            $"Port={databaseUri.Port};" +
            $"Username={userInfo[0]};" +
            $"Password={userInfo[1]};" +
            $"Database={databaseUri.AbsolutePath.TrimStart('/')};" +
            $"SSL Mode=Require;Trust Server Certificate=true";

        bootstrapLogger.LogInformation("Database: Using Heroku PostgreSQL");
    }
    else
    {
        // Local dev → use PostgreSQL connection string from appsettings
        connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection string is missing in appsettings.json");

        bootstrapLogger.LogInformation("Database: Using local PostgreSQL");
    }

    options.UseNpgsql(connectionString, b => b.MigrationsAssembly("CbTsSa_Shared"));
});

builder.Services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());

// Domain services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ConversationContextFactory>();
builder.Services.AddScoped<BusinessContextFactory>();
builder.Services.AddScoped<ICommandRunner, CommandRunner>();

// ---- Form Handler Registry ----
// Register all form handlers as scoped
builder.Services.AddScoped<CommandBot.Commands.FormHandlers.UserSignupFormHandler>();

// Register the registry as singleton and configure it via a factory
builder.Services.AddSingleton<CommandBot.Commands.FormHandlerRegistry>(sp =>
{
    var registry = new CommandBot.Commands.FormHandlerRegistry();

    // Create a temporary scope to resolve scoped handlers for registration
    using var scope = sp.CreateScope();
    var userSignupHandler = scope.ServiceProvider.GetRequiredService<CommandBot.Commands.FormHandlers.UserSignupFormHandler>();

    registry.Register(userSignupHandler);

    bootstrapLogger.LogInformation("FormHandlerRegistry configured with {Count} handlers: {Types}",
        2,
        string.Join(", ", registry.GetRegisteredFormTypes()));

    return registry;
});

// ---- Field Update Handler Registry ----
// Register all field handlers as scoped
builder.Services.AddScoped<CommandBot.Commands.FieldHandlers.NameFieldHandler>();
builder.Services.AddScoped<CommandBot.Commands.FieldHandlers.EmailFieldHandler>();
builder.Services.AddScoped<CommandBot.Commands.FieldHandlers.SocialFieldHandler>();
builder.Services.AddScoped<CommandBot.Commands.FieldHandlers.CellFieldHandler>();
builder.Services.AddScoped<CommandBot.Commands.FieldHandlers.ConsentFieldHandler>();

// Register the registry as singleton with a factory that configures it once
builder.Services.AddSingleton<CommandBot.Commands.FieldHandlers.FieldUpdateHandlerRegistry>(sp =>
{
    var registry = new CommandBot.Commands.FieldHandlers.FieldUpdateHandlerRegistry();

    // Create a temporary scope to resolve scoped handlers for registration
    using var scope = sp.CreateScope();
    registry.Register(scope.ServiceProvider.GetRequiredService<CommandBot.Commands.FieldHandlers.NameFieldHandler>());
    registry.Register(scope.ServiceProvider.GetRequiredService<CommandBot.Commands.FieldHandlers.EmailFieldHandler>());
    registry.Register(scope.ServiceProvider.GetRequiredService<CommandBot.Commands.FieldHandlers.SocialFieldHandler>());
    registry.Register(scope.ServiceProvider.GetRequiredService<CommandBot.Commands.FieldHandlers.CellFieldHandler>());
    registry.Register(scope.ServiceProvider.GetRequiredService<CommandBot.Commands.FieldHandlers.ConsentFieldHandler>());

    bootstrapLogger.LogInformation("FieldUpdateHandlerRegistry configured with {Count} handlers: {Types}",
        5,
        string.Join(", ", registry.GetRegisteredFieldNames()));

    return registry;
});

// Register CommandRegistry and CommandParser (single registration, after http factory is available)
builder.Services.AddSingleton<CommandRegistry>();
builder.Services.AddScoped<CommandParser>();

// ---- RabbitMQ services ----
// Register the concrete service once and map the interface to the same instance
builder.Services.AddSingleton<RabbitMQService>();
builder.Services.AddSingleton<IRabbitMQInterface>(sp => sp.GetRequiredService<RabbitMQService>());

// These are the correct shared interfaces
builder.Services.AddSingleton<ITelegramDispatchService, TelegramDispatchService>();
builder.Services.AddSingleton<IWhatsAppDispatchService, WhatsAppDispatchService>();

// ---- Background workers ----
builder.Services.AddHostedService<RabbitMQInitializer>();
builder.Services.AddHostedService<CommandWorker>();
builder.Services.AddHostedService<TelegramConsumerService>();
builder.Services.AddHostedService<WhatsAppConsumerService>();
builder.Services.AddHostedService<BroadcastCleanupWorker>();
builder.Services.AddSingleton<IBackgroundTaskRunner, DefaultBackgroundTaskRunner>();

// ---- BusinessContext scoped binding ----
builder.Services.AddScoped<BusinessContext>(sp =>
{
    var db = sp.GetRequiredService<IAppDbContext>();
    var opts = sp.GetRequiredService<IOptions<HostBusiness>>().Value;
    var factory = sp.GetRequiredService<BusinessContextFactory>();

    return factory.CreateBusinessContext(
        db,
        opts.BaseUrl,
        opts.Cellnumber,
        ChatChannelType.WhatsApp);
});

// ---- WhatsApp Business Cloud API (prefer environment variables) ----
// Build whatsappConfig by preferring Heroku / environment variables (double-underscore matches IConfiguration keys)
// Do NOT fall back between WhatsAppBusinessId and WhatsAppBusinessAccountId. If either is missing, fail startup with a clear error.
var whatsappConfig = new WhatsappBusiness.CloudApi.Configurations.WhatsAppBusinessCloudApiConfig
{
    AccessToken = (Environment.GetEnvironmentVariable("WhatsAppBusiness__AccessToken")
                  ?? configuration["WhatsAppBusiness:AccessToken"])!,
    WhatsAppBusinessPhoneNumberId = (Environment.GetEnvironmentVariable("WhatsAppBusiness__WhatsAppBusinessPhoneNumberId")
                                     ?? configuration["WhatsAppBusiness:WhatsAppBusinessPhoneNumberId"])!,
    // canonical explicit reads — do NOT treat one as fallback for the other
    WhatsAppBusinessId = (Environment.GetEnvironmentVariable("WhatsAppBusiness__WhatsAppBusinessId")
                         ?? configuration["WhatsAppBusiness:WhatsAppBusinessId"])!,
    WhatsAppBusinessAccountId = (Environment.GetEnvironmentVariable("WhatsAppBusiness__WhatsAppBusinessAccountId")
                                 ?? configuration["WhatsAppBusiness:WhatsAppBusinessAccountId"])!,
    WebhookVerifyToken = (Environment.GetEnvironmentVariable("WhatsAppBusiness__WebhookVerifyToken")
                         ?? configuration["WhatsAppBusiness:WebhookVerifyToken"])!,
    AppName = (Environment.GetEnvironmentVariable("WhatsAppBusiness__AppName")
              ?? configuration["WhatsAppBusiness:AppName"])!,
    Version = (Environment.GetEnvironmentVariable("WhatsAppBusiness__Version")
              ?? configuration["WhatsAppBusiness:Version"])!
};

// Enforce explicit presence — do not attempt to resolve one from the other.
if (string.IsNullOrWhiteSpace(whatsappConfig.AccessToken))
    throw new InvalidOperationException("WhatsApp configuration is missing AccessToken. Set environment variable 'WhatsAppBusiness__AccessToken'.");

if (string.IsNullOrWhiteSpace(whatsappConfig.WhatsAppBusinessId))
    throw new InvalidOperationException("WhatsApp configuration is missing WhatsAppBusinessId. Set environment variable 'WhatsAppBusiness__WhatsAppBusinessId'.");

if (string.IsNullOrWhiteSpace(whatsappConfig.WhatsAppBusinessAccountId))
    throw new InvalidOperationException("WhatsApp configuration is missing WhatsAppBusinessAccountId. Set environment variable 'WhatsAppBusiness__WhatsAppBusinessAccountId'.");

// Diagnostic — do not print secrets
bootstrapLogger.LogInformation("=== WhatsApp Cloud API Config Debug ===");
bootstrapLogger.LogInformation("AccessToken: {State}", string.IsNullOrWhiteSpace(whatsappConfig.AccessToken) ? "NOT SET" : "SET");
bootstrapLogger.LogInformation("WhatsAppBusinessPhoneNumberId: {State}", string.IsNullOrWhiteSpace(whatsappConfig.WhatsAppBusinessPhoneNumberId) ? "NOT SET" : "SET");
bootstrapLogger.LogInformation("WhatsAppBusinessId: {State}", string.IsNullOrWhiteSpace(whatsappConfig.WhatsAppBusinessId) ? "NOT SET" : "SET");
bootstrapLogger.LogInformation("WhatsAppBusinessAccountId: {State}", string.IsNullOrWhiteSpace(whatsappConfig.WhatsAppBusinessAccountId) ? "NOT SET" : "SET");
bootstrapLogger.LogInformation("WebhookVerifyToken: {State}", string.IsNullOrWhiteSpace(whatsappConfig.WebhookVerifyToken) ? "NOT SET" : "SET");
bootstrapLogger.LogInformation("AppName: {State}", string.IsNullOrWhiteSpace(whatsappConfig.AppName) ? "NOT SET" : "SET");
bootstrapLogger.LogInformation("Version: {State}", string.IsNullOrWhiteSpace(whatsappConfig.Version) ? "NOT SET" : "SET");
bootstrapLogger.LogInformation("=============================");

// Register service using explicit WABA id (do not override it)
builder.Services.AddWhatsAppBusinessCloudApiService(whatsappConfig);

// Register named WhatsApp HttpClient (kept - commands use "WhatsAppApi" named client)
builder.Services.AddHttpClient("WhatsAppApi", client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("User-Agent", "CommandBot/1.0");
});

// Register services
builder.Services.AddScoped<WhatsAppMediaService>();

// Bind Telegram client options (token) from config/env
builder.Services.Configure<CommandBot.Clients.TelegramClientOptions>(options =>
{
    options.BotToken = Environment.GetEnvironmentVariable("TelegramBusiness__BotToken")
                       ?? configuration["TelegramBusiness:BotToken"];
});

// Register typed TelegramHttpClient using IHttpClientFactory with resilience policies (Polly)
builder.Services.AddHttpClient<CommandBot.Clients.ITelegramClient, CommandBot.Clients.TelegramHttpClient>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("User-Agent", "CommandBot/1.0");
})
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler())
    .AddPolicyHandler((sp, request) =>
        HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryAttempt, context) =>
                {
                    var log = sp.GetRequiredService<ILogger<CommandBot.Clients.TelegramHttpClient>>();
                    if (outcome.Exception != null)
                        log.LogWarning(outcome.Exception, "Telegram HTTP retry {Retry} for {Url} after {Delay}s due to exception", retryAttempt, request.RequestUri, timespan.TotalSeconds);
                    else
                        log.LogWarning("Telegram HTTP retry {Retry} for {Url} after {Delay}s due to status {Status}", retryAttempt, request.RequestUri, timespan.TotalSeconds, outcome.Result?.StatusCode);
                }))
    .AddPolicyHandler((sp, request) =>
        HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30),
                onBreak: (outcome, timespan) =>
                {
                    var log = sp.GetRequiredService<ILogger<CommandBot.Clients.TelegramHttpClient>>();
                    if (outcome.Exception != null)
                        log.LogError(outcome.Exception, "Telegram circuit breaker opened for {Url} due to exception", request.RequestUri);
                    else
                        log.LogError("Telegram circuit breaker opened for {Url} due to status {Status}", request.RequestUri, outcome.Result?.StatusCode);
                },
                onReset: () =>
                {
                    var log = sp.GetRequiredService<ILogger<CommandBot.Clients.TelegramHttpClient>>();
                    log.LogInformation("Telegram circuit breaker reset");
                })) ;

// ---- MVC + Swagger ----
builder.Services.AddControllers();
builder.Services.AddRazorPages();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// ---- Build app ----
// ---- Build app ----
var app = builder.Build();

// Acquire a logger for startup diagnostics
var logger = app.Services.GetRequiredService<ILogger<Program>>();

// ⚠️ VALIDATE TELEGRAM BOT CLIENT USING THE BUILT APPLICATION SERVICE PROVIDER
// This uses app.Services instead of calling builder.Services.BuildServiceProvider() which
// would create a second ServiceProvider and duplicate singleton instances.
try
{
    var tgClient = app.Services.GetRequiredService<CommandBot.Clients.ITelegramClient>(); // Ensure telemetry validation uses ITelegramClient retrieval
    logger.LogInformation("✅ Telegram client validated successfully during startup");
}
catch (Exception ex)
{
    logger.LogError(ex, "❌ TELEGRAM BOT CLIENT VALIDATION FAILED: {Message}", ex.Message);
    throw;
}

// Heroku port binding
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Urls.Clear();
app.Urls.Add($"http://*:{port}");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseRouting();
app.UseStaticFiles(); // If you have wwwroot folder

app.UseAuthorization();
app.MapControllers();
app.MapRazorPages();

// Add diagnostic logging
var endpoints = app.Services.GetRequiredService<Microsoft.AspNetCore.Routing.EndpointDataSource>();
logger.LogInformation("=== Registered Endpoints ===");
foreach (var endpoint in endpoints.Endpoints)
{
    if (endpoint is Microsoft.AspNetCore.Routing.RouteEndpoint routeEndpoint)
    {
        logger.LogInformation("  {Route}", routeEndpoint.RoutePattern.RawText);
    }
}
logger.LogInformation("============================");

// ✅ PRE-FLIGHT MIGRATION CHECK
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    try
    {
        // ✅ Check if EF thinks migrations are pending
        var pendingMigrations = db.Database.GetPendingMigrations().ToList();
        var appliedMigrations = db.Database.GetAppliedMigrations().ToList();

        logger.LogInformation("📊 Migration Status:");
        logger.LogInformation("   Applied: {AppliedCount}", appliedMigrations.Count);
        logger.LogInformation("   Pending: {PendingCount}", pendingMigrations.Count);

        if (pendingMigrations.Any())
        {
            logger.LogWarning("⚠️ Found {Count} pending migration(s):", pendingMigrations.Count);
            foreach (var migration in pendingMigrations)
            {
                logger.LogWarning("   - {Migration}", migration);
            }

            if (!appliedMigrations.Any())
            {
                logger.LogInformation("✅ Fresh database detected - applying all migrations...");
            }
            else
            {
                logger.LogInformation("⚠️ Incomplete or new migrations detected - applying updates...");
            }

            db.Database.Migrate();
            logger.LogInformation("✅ Migrations applied successfully.");
        }
        else
        {
            logger.LogInformation("✅ Database schema is up-to-date (no pending migrations).");
        }
    }
    catch (Npgsql.PostgresException pgEx) when (pgEx.SqlState == "42P07")
    {
        // 42P07 = relation already exists
        logger.LogError("❌ MIGRATION CONFLICT DETECTED:");
        logger.LogError("❌ Tables already exist but migration history is incomplete.");
        logger.LogError("❌ This indicates a previous migration was interrupted.");
        logger.LogError("\n🔧 RECOVERY OPTIONS:");
        logger.LogError("   1. DROP SCHEMA public CASCADE; CREATE SCHEMA public; (clean slate)");
        logger.LogError("   2. Manually insert migration record into __EFMigrationsHistory");
        logger.LogError("\n❌ Error: {Message}", pgEx.MessageText);
        
        var crashOnMigrationFailure = Environment.GetEnvironmentVariable("CRASH_ON_MIGRATION_FAILURE") == "true";
        if (crashOnMigrationFailure)
        {
            throw;
        }
        
        logger.LogWarning("⚠️ Application will start but database may be in inconsistent state.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "❌ Migration pre-flight check failed:");
        
        var crashOnMigrationFailure = Environment.GetEnvironmentVariable("CRASH_ON_MIGRATION_FAILURE") == "true";
        if (crashOnMigrationFailure)
        {
            throw;
        }
        
        logger.LogWarning("⚠️ Application will start but database operations may fail.");
    }
}

// Add this right before app.Run() for diagnostics
app.MapGet("/", () => "ShakeApp API is running!");
app.MapGet("/test", () => "Test endpoint works!");

app.Run();
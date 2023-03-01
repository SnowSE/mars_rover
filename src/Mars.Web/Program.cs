using Hellang.Middleware.ProblemDetails;
using Mars.Web;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Exceptions;
using System.Diagnostics.Metrics;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;

// This is required if the collector doesn't expose an https endpoint. By default, .NET
// only allows http2 (required for gRPC) to secure endpoints.
AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

var builder = WebApplication.CreateBuilder(args);

var appResourceBuilder = ResourceBuilder
    .CreateDefault()
    .AddService("Mars.Web");

builder.Logging.AddOpenTelemetry(o =>
{
    o.IncludeFormattedMessage = true;
    o.IncludeScopes = true;
    o.SetResourceBuilder(appResourceBuilder);
    o.ParseStateValues = true;
    o.AddOtlpExporter(o => o.Endpoint = new Uri("http://otel-collector:4317"));
});

builder.Services.AddOpenTelemetry()
    .WithTracing(builder =>
    {
        builder.SetResourceBuilder(appResourceBuilder);
        builder.AddSource(ActivitySources.MarsWeb.Name);
        builder.AddSource(ActivitySources.Demo.Name);
        builder.AddHttpClientInstrumentation();
        builder.AddAspNetCoreInstrumentation(o =>
        {
            o.EnrichWithException = (activity, exception) =>
            {
                activity.SetTag("exceptionType", exception.GetType().ToString());
            };
        });
        builder.AddOtlpExporter(o => o.Endpoint = new Uri("http://otel-collector:4317"));
    });

var joinMeter = new Meter("Mars.Web.Meters");
var counter = joinMeter.CreateCounter<long>("Game Joins");
builder.Services.AddSingleton<MarsCounters>(new MarsCounters
{
    GameJoins = counter,
});

builder.Services.AddOpenTelemetry()
    .WithMetrics(builder =>
    {
        builder.AddMeter(joinMeter.Name);
        builder.SetResourceBuilder(appResourceBuilder);
        builder.AddAspNetCoreInstrumentation();
        builder.AddHttpClientInstrumentation();
        builder.AddRuntimeInstrumentation();
        builder.AddProcessInstrumentation();
        builder.AddOtlpExporter(o => o.Endpoint = new Uri("http://otel-collector:4317"));
    });

builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpClient<ExtraApiClient>();

builder.Host.UseSerilog((context, loggerConfig) =>
{
    loggerConfig.WriteTo.Console()
    .Enrich.WithExceptionDetails()
    .WriteTo.Seq(builder.Configuration["SeqServer"] ?? throw new ApplicationException("Unable to locate key SeqServer in configuration"));
});

builder.Services.AddProblemDetails(opts =>
{
    opts.IncludeExceptionDetails = (ctx, ex) => false;
});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Mars Rover", Version = "v1" });
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

builder.Services.AddSingleton<MultiGameHoster>();
builder.Services.AddSingleton<IMapProvider, FileSystemMapProvider>();

builder.Services.AddHostedService<CleanupGameService>();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = 429;
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Request.Query["token"].FirstOrDefault() ?? httpContext.Request.Headers.Host.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = int.Parse(builder.Configuration["ApiLimitPerSecond"] ?? throw new Exception("Unable to find ApiLimitPerSecond in config")),
                QueueLimit = 0,
                Window = TimeSpan.FromSeconds(1)
            }));
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStaticFiles();

app.UseRouting();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Mars Rover v1");
});

app.MapBlazorHub().DisableRateLimiting();

#if !DEBUG
app.UseRateLimiter();
#endif

app.MapControllers();

app.MapFallbackToPage("/_Host").DisableRateLimiting();

app.Run();

public partial class Program { }

public class ExtraApiClient
{
    private readonly HttpClient client;

    public ExtraApiClient(HttpClient client)
    {
        this.client = client;
        client.BaseAddress = new Uri("http://mars.extraapi");
    }

    public async Task<string> GetWeatherAsync()
    {
        return await client.GetStringAsync("/weatherforecast");
    }
}
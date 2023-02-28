using Hellang.Middleware.ProblemDetails;
using Mars.Web;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Exceptions;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

//using var traceProvider = Sdk.CreateTracerProviderBuilder()
//    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("Mars.Web"))
//    .AddSource(GameActivitySource.Instance.Name)
//    .AddJaegerExporter(o =>
//    {
//        o.Protocol = OpenTelemetry.Exporter.JaegerExportProtocol.HttpBinaryThrift;
//        o.Endpoint = new Uri("http://jaeger:14268/api/traces");
//    })
//    .AddHttpClientInstrumentation()
//    .AddAspNetCoreInstrumentation()
//    .Build();

builder.Services.AddOpenTelemetry()
    .WithTracing(builder =>
    {
        builder.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("Mars.Web"));
        builder.AddSource(ActivitySources.MarsWeb.Name);
        builder.AddSource(ActivitySources.Demo.Name);
        builder.AddJaegerExporter(o =>
        {
            o.Protocol = OpenTelemetry.Exporter.JaegerExportProtocol.HttpBinaryThrift;
            o.Endpoint = new Uri("http://jaeger:14268/api/traces");
        });
        builder.AddHttpClientInstrumentation();
        builder.AddAspNetCoreInstrumentation(o =>
        {
            o.EnrichWithException = (activity, exception) =>
            {
                activity.SetTag("exceptionType", exception.GetType().ToString());
            };
        });
    })
    .WithMetrics(builder =>
    {
        builder.AddAspNetCoreInstrumentation();
        builder.AddHttpClientInstrumentation();
        builder.AddProcessInstrumentation();
        //builder.AddPrometheusExporter(config =>
        //{
        //    config.StartHttpListener = true;
        //    config.HttpListenerPrefixes = new[]
        //    {
        //        "http://prometheus:9464"
        //    };
        //});
    });

//using var meterProvider = Sdk.CreateMeterProviderBuilder()
//    .AddRuntimeInstrumentation()
//    .AddProcessInstrumentation()
//    .AddPrometheusExporter(o =>
//    {
//        o.StartHttpListener = true;
//        o.HttpListenerPrefixes = new[]
//        {
//            "http://prometheus:9184"
//        };
//    })
//    .Build();

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
}
);

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
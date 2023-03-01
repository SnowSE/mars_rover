using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics;

// This is required if the collector doesn't expose an https endpoint. By default, .NET
// only allows http2 (required for gRPC) to secure endpoints.
AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

var builder = WebApplication.CreateBuilder(args);

var appResourceBuilder = ResourceBuilder
    .CreateDefault()
    .AddService("Mars.ExtraApi");

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
        builder.AddSource(ExtraApiActivitySources.Default.Name);
        builder.AddSource(ExtraApiActivitySources.Weather.Name);
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

builder.Services.AddOpenTelemetry()
    .WithMetrics(builder =>
    {
        builder.SetResourceBuilder(appResourceBuilder);
        builder.AddAspNetCoreInstrumentation();
        builder.AddHttpClientInstrumentation();
        builder.AddRuntimeInstrumentation();
        builder.AddProcessInstrumentation();
        builder.AddOtlpExporter(o => o.Endpoint = new Uri("http://otel-collector:4317"));
    });

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

public static class ExtraApiActivitySources
{
    public static ActivitySource Default { get; } = new ActivitySource("Mars.ExtraApi", "1.0");
    public static ActivitySource Weather { get; } = new ActivitySource("Mars.ExtraApi Weather", "1.0");
}
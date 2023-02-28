using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);


using var traceProvider = Sdk.CreateTracerProviderBuilder()
    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("Mars.ExtraApi"))
    .AddSource(ExtraApiActivitySources.Default.Name)
    .AddJaegerExporter(o =>
    {
        o.Protocol = OpenTelemetry.Exporter.JaegerExportProtocol.HttpBinaryThrift;
        o.Endpoint = new Uri("http://jaeger:14268/api/traces");
    })
    .AddHttpClientInstrumentation()
    .AddAspNetCoreInstrumentation()
    .Build();

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
}
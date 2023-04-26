using Hellang.Middleware.ProblemDetails;
using Mars.Web;
using Mars.Web.Controllers;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Exceptions;
using System.Configuration;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

using var traceProvider = Sdk.CreateTracerProviderBuilder()
	.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("Mars.Web"))
	.AddSource(GameActivitySource.Instance.Name)
	.AddJaegerExporter(o =>
	{
		o.Protocol = OpenTelemetry.Exporter.JaegerExportProtocol.HttpBinaryThrift;
		o.Endpoint = new Uri("http://jaeger:14268/api/traces");
	})
	.AddHttpClientInstrumentation()
	.AddAspNetCoreInstrumentation()
	.Build();

builder.Services.AddOptions<GameConfig>()
	.BindConfiguration(nameof(GameConfig))
	.ValidateDataAnnotations()
	.ValidateOnStart();
builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddControllers().AddJsonOptions(options =>
{
	options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddEndpointsApiExplorer();

builder.Host.UseSerilog((context, loggerConfig) =>
{
	loggerConfig.WriteTo.Console()
	.Enrich.WithExceptionDetails()
	.WriteTo.Seq(builder.Configuration["SeqServer"] ?? throw new ConfigurationErrorsException("Unable to locate key SeqServer in configuration"));
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
	{
		var token = httpContext.Request.Query["token"].FirstOrDefault();
		var ipAddress = httpContext.Request.Headers.Host.ToString();
		var apiLimit = httpContext.RequestServices.GetRequiredService<IOptions<GameConfig>>().Value.ApiLimitPerSecond;

		return RateLimitPartition.GetFixedWindowLimiter(
			partitionKey: token ?? ipAddress,
			factory: partition => new FixedWindowRateLimiterOptions
			{
				AutoReplenishment = true,
				PermitLimit = apiLimit,
				QueueLimit = 0,
				Window = TimeSpan.FromSeconds(1)
			});
	});
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

app.MapBlazorHub()
   .DisableRateLimiting();

app.UseRateLimiter();

app.MapControllers();

app.MapFallbackToPage("/_Host")
   .DisableRateLimiting();

app.Run();

public partial class Program { }

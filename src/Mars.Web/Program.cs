using Mars.Web;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Exceptions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Hellang.Middleware.ProblemDetails;
using Serilog;
using Serilog.Exceptions;

var builder = WebApplication.CreateBuilder(args);
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

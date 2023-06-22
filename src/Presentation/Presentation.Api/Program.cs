using DataCollectors.OPCUA.Core.Application;
using DataCollectors.OPCUA.Core.Application.Shared.Options;
using DataCollectors.OPCUA.Presentation.Api.Configuration.Options;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Server.HttpSys;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder
    .Host
    .ConfigureSerilog();

builder
    .WebHost
    .UseHttpSys(options =>
    {
        options.AllowSynchronousIO = false;
        options.Authentication.Schemes = AuthenticationSchemes.None;
        options.Authentication.AllowAnonymous = true;
        options.MaxConnections = null;
        options.MaxRequestBodySize = 30000000;
    });

builder
    .Services
    .Configure<OPCUASettings>(builder.Configuration.GetSection("OPCUA"))
    .Configure<HostOptions>(hostOptions =>
    {
        hostOptions.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
    });

builder
    .Services
    .AddCoreServices();

builder
    .Services
    .AddHttpContextAccessor()
    .AddMemoryCache()
    .AddHttpLogging(logging =>
    {
        logging.LoggingFields = HttpLoggingFields.RequestBody;

        logging.MediaTypeOptions.AddText("application/javascript");
        logging.RequestBodyLogLimit = 4096;
        logging.ResponseBodyLogLimit = 4096;
    });

builder.Services
    .AddControllers()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder
    .Services
    .AddHealthChecks();

var app = builder.Build();

app
    .UseHttpLogging();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app
    .UseHttpsRedirection()
    .UseRouting();

app.MapControllers();

app
    .MapHealthChecks("/health");

app.Run();
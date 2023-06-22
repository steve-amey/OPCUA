using Serilog;

namespace DataCollectors.OPCUA.Presentation.Api.Configuration.Options;

public static class SerilogConfiguration
{
    public static void ConfigureSerilog(this IHostBuilder host)
    {
        host.UseSerilog((ctx, provider, logger) =>
        {
            logger
                .ReadFrom.Configuration(ctx.Configuration);
        });
    }
}
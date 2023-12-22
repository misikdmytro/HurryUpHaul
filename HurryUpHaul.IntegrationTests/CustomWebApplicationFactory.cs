using Microsoft.AspNetCore.Mvc.Testing;

using Serilog;

namespace HurryUpHaul.IntegrationTests
{
    public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
    {
        protected override IHost CreateHost(IHostBuilder builder)
        {
            builder.UseSerilog((ctx, cfg) =>
            {
                cfg.ReadFrom.Configuration(ctx.Configuration);
                cfg.MinimumLevel.Is(Serilog.Events.LogEventLevel.Warning);
            });

            return base.CreateHost(builder);
        }
    }
}
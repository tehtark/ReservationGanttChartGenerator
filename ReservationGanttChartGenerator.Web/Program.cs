using MudBlazor.Services;
using ReservationGanttChartGenerator.Web.Components;
using ReservationGanttChartGenerator.Application;
using Serilog;
using Serilog.Events;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Initialise logger.
        InitialiseLogger(builder.Host);

        InitialiseServices(builder.Services);

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment()) {
            app.UseExceptionHandler("/Error", createScopeForErrors: true);
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.UseStaticFiles();
        app.UseAntiforgery();

        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        app.Run();
    }

    private static void InitialiseLogger(IHostBuilder host)
    {
        host.UseSerilog((context, logger) => {
            logger.MinimumLevel.Is(LogEventLevel.Debug)
            .WriteTo.File(AppDomain.CurrentDomain.BaseDirectory + "/logs/log.json", rollingInterval: RollingInterval.Day, shared: true)
            .WriteTo.Console();
        });

        Log.Debug("Logger: Initialised.");
    }

    private static void InitialiseServices(IServiceCollection services)
    {
        // Add MudBlazor services
        services.AddMudServices();

        // Add services to the container.
        services.AddRazorComponents().AddInteractiveServerComponents();

        services.AddApplication();
    }
}
using Microsoft.Extensions.DependencyInjection;

namespace ReservationGanttChartGenerator.Application;

public static class ApplicationDependencyInjection
{
    public static void AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(config => {
            config.RegisterServicesFromAssembly(typeof(ApplicationDependencyInjection).Assembly);
        });

        services.AddTransient<Abstractions.ImageService>();
    }
}
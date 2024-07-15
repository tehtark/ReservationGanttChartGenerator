using Microsoft.Extensions.DependencyInjection;
using ReservationGanttChartGenerator.Application.Abstractions;
using ReservationGanttChartGenerator.Application.Abstractions.Interfaces;

namespace ReservationGanttChartGenerator.Application;

public static class ApplicationDependencyInjection
{
    public static void AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(config => {
            config.RegisterServicesFromAssembly(typeof(ApplicationDependencyInjection).Assembly);
        });

        services.AddTransient<IFileService, FileService>();
        services.AddTransient<IImageGenerationService, ImageGenerationService>();
    }
}
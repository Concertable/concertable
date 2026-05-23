using Concertable.Shared.Pdf.Application;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Shared.Pdf.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSharedPdf(this IServiceCollection services)
    {
        services.AddScoped<IPdfService, PdfService>();
        return services;
    }
}

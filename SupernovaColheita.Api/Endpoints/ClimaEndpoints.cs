using SupernovaColheita.Api.Services;

namespace SupernovaColheita.Api.Endpoints;

public static class ClimaEndpoints
{
    public static void MapClimaEndpoints(this WebApplication app)
    {
        app.MapGet("/api/clima", async (
                double latitude,
                double longitude,
                ClimaService climaService,
                CancellationToken cancellationToken) =>
            {
                if (latitude < -90 || latitude > 90 ||
                    longitude < -180 || longitude > 180 ||
                    double.IsNaN(latitude) ||
                    double.IsNaN(longitude))
                {
                    return Results.BadRequest(
                        "Latitude ou longitude inválida.");
                }

                try
                {
                    using var dados = await climaService.ConsultarAsync(
                        latitude, longitude, cancellationToken);

                    return Results.Ok(dados.RootElement.Clone());
                }
                catch (HttpRequestException)
                {
                    return Results.Problem(
                        "Serviço meteorológico indisponível.",
                        statusCode: 503);
                }
                catch (TaskCanceledException)
                    when (!cancellationToken.IsCancellationRequested)
                {
                    return Results.Problem(
                        "Tempo limite da consulta meteorológica excedido.",
                        statusCode: 503);
                }
            })
            .WithTags("Clima")
            .WithSummary("Consultar condições meteorológicas")
            .WithDescription("Consulta dados meteorológicos da Open-Meteo por latitude e longitude.")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status429TooManyRequests)
            .Produces(StatusCodes.Status503ServiceUnavailable)
            .RequireAuthorization()
            .RequireRateLimiting("limite-api");
    }
}
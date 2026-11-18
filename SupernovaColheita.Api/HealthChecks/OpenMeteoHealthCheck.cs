using Microsoft.Extensions.Diagnostics.HealthChecks;
using SupernovaColheita.Api.Services;

namespace SupernovaColheita.Api.HealthChecks;

public class OpenMeteoHealthCheck : IHealthCheck
{
    private readonly ClimaService _climaService;

    public OpenMeteoHealthCheck(ClimaService climaService)
    {
        _climaService = climaService;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var dados = await _climaService.ConsultarAsync(
                -22.9056, -47.0608, cancellationToken);

            if (!dados.RootElement.TryGetProperty(
                    "current", out _))
            {
                return HealthCheckResult.Unhealthy(
                    "Open-Meteo respondeu sem dados meteorológicos.");
            }

            return HealthCheckResult.Healthy(
                "Open-Meteo respondeu corretamente.");
        }
        catch (Exception ex)
            when (ex is HttpRequestException
                      or TaskCanceledException
                      or System.Text.Json.JsonException)
        {
            return HealthCheckResult.Unhealthy(
                "Falha ao consultar Open-Meteo.", ex);
        }
    }
}
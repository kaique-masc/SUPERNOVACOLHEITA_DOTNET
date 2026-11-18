using System.Globalization;
using System.Text.Json;

namespace SupernovaColheita.Api.Services;

public class ClimaService
{
    private readonly HttpClient _httpClient;

    public ClimaService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<JsonDocument> ConsultarAsync(
        double latitude,
        double longitude,
        CancellationToken cancellationToken = default)
    {
        var lat = latitude.ToString(CultureInfo.InvariantCulture);
        var lon = longitude.ToString(CultureInfo.InvariantCulture);

        var url =
            $"v1/forecast?latitude={lat}&longitude={lon}" +
            "&current=temperature_2m,relative_humidity_2m," +
            "precipitation,wind_speed_10m" +
            "&timezone=auto";

        using var resposta = await _httpClient.GetAsync(
            url, cancellationToken);

        resposta.EnsureSuccessStatusCode();

        await using var stream =
            await resposta.Content.ReadAsStreamAsync(cancellationToken);

        return await JsonDocument.ParseAsync(
            stream, cancellationToken: cancellationToken);
    }
}
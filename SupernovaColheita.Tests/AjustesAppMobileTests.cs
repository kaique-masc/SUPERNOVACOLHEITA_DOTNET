using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace SupernovaColheita.Tests;

public class TokenDaApiJavaTests
{
    [Fact]
    public async Task TokenComIssuerDaJavaESemAudience_DeveSerAceito()
    {
        using var aplicacao = new ApiTesteFactory();
        using var cliente = aplicacao.ClienteComToken(
            TokensDeTeste.GerarDaJava());

        using var resposta = await cliente.GetAsync("/api/oportunidades");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
    }

    [Fact]
    public async Task TokenComIssuerDesconhecido_DeveRetornar401()
    {
        using var aplicacao = new ApiTesteFactory();
        using var cliente = aplicacao.ClienteComToken(
            TokensDeTeste.GerarDaJava(emissor: "emissor-desconhecido"));

        using var resposta = await cliente.GetAsync("/api/oportunidades");

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task TokenAssinadoComOutraChave_DeveRetornar401()
    {
        using var aplicacao = new ApiTesteFactory();
        using var cliente = aplicacao.ClienteComToken(
            TokensDeTeste.GerarDaJava(
                chave: "OutraChaveQualquerComMaisDe32Caracteres!!"));

        using var resposta = await cliente.GetAsync("/api/oportunidades");

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }
}

public class SeedOportunidadesTests
{
    [Fact]
    public async Task BancoNovo_DeveTerCincoOportunidadesFuturasEmCidadesDiferentes()
    {
        using var aplicacao = new ApiTesteFactory();
        using var cliente = aplicacao.ClienteComToken(
            TokensDeTeste.GerarDaJava());

        var oportunidades = await cliente.GetFromJsonAsync<JsonElement>(
            "/api/oportunidades");

        Assert.Equal(5, oportunidades.GetArrayLength());

        var cidades = oportunidades.EnumerateArray()
            .Select(o => o.GetProperty("municipio").GetString())
            .Distinct()
            .Count();

        Assert.Equal(5, cidades);

        foreach (var oportunidade in oportunidades.EnumerateArray())
        {
            Assert.True(
                oportunidade.GetProperty("prazoInscricao").GetDateTime()
                    > DateTime.UtcNow);
        }
    }
}

public class CorsTests
{
    [Fact]
    public async Task Preflight_DaVersaoWebDoApp_DevePermitirQualquerOrigem()
    {
        using var aplicacao = new ApiTesteFactory();
        using var cliente = aplicacao.CreateClient();

        using var requisicao = new HttpRequestMessage(
            HttpMethod.Options, "/api/oportunidades");

        requisicao.Headers.TryAddWithoutValidation(
            "Origin", "http://localhost:8081");
        requisicao.Headers.TryAddWithoutValidation(
            "Access-Control-Request-Method", "GET");
        requisicao.Headers.TryAddWithoutValidation(
            "Access-Control-Request-Headers", "authorization");

        using var resposta = await cliente.SendAsync(requisicao);

        Assert.True(
            resposta.IsSuccessStatusCode,
            $"Preflight retornou {(int)resposta.StatusCode}.");

        Assert.True(
            resposta.Headers.TryGetValues(
                "Access-Control-Allow-Origin", out var origens));

        Assert.Contains("*", origens);
    }
}

public class RateLimitConfiguravelTests
{
    [Fact]
    public async Task LimitePadraoDoAppsettings_NaoBloqueiaUsoNormalDoApp()
    {
        using var aplicacao = new ApiTesteFactory();
        using var cliente = aplicacao.ClienteComToken(
            TokensDeTeste.GerarDaJava());

        for (var i = 0; i < 20; i++)
        {
            using var resposta = await cliente.GetAsync(
                "/api/oportunidades");

            Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        }
    }

    [Fact]
    public async Task LimiteReduzidoPorConfiguracao_DeveRetornar429AposOLimite()
    {
        using var aplicacao = new ApiTesteFactory(limitePorMinuto: 2);
        using var cliente = aplicacao.ClienteComToken(
            TokensDeTeste.GerarDaJava());

        for (var i = 0; i < 2; i++)
        {
            using var permitida = await cliente.GetAsync(
                "/api/oportunidades");

            Assert.Equal(HttpStatusCode.OK, permitida.StatusCode);
        }

        using var bloqueada = await cliente.GetAsync("/api/oportunidades");

        Assert.Equal(HttpStatusCode.TooManyRequests, bloqueada.StatusCode);
    }
}

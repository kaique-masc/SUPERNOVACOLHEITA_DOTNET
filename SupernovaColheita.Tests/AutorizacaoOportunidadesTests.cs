using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace SupernovaColheita.Tests;

public class AutorizacaoOportunidadesTests
{
    private static object NovaOportunidade() => new
    {
        titulo = "Chamada pública de teste",
        instituicao = "Escola de Teste",
        municipio = "Campinas",
        estado = "SP",
        valorEstimado = 1000,
        prazoInscricao = DateTime.UtcNow.AddDays(10),
        ativa = true
    };

    [Fact]
    public async Task Produtor_PodeListarOportunidades()
    {
        using var aplicacao = new ApiTesteFactory();
        using var cliente = aplicacao.ClienteComToken(
            TokensDeTeste.GerarDaJava(perfil: "PRODUTOR"));

        using var resposta = await cliente.GetAsync("/api/oportunidades");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
    }

    [Fact]
    public async Task Produtor_NaoPodePublicarOportunidade_DeveRetornar403()
    {
        using var aplicacao = new ApiTesteFactory();
        using var cliente = aplicacao.ClienteComToken(
            TokensDeTeste.GerarDaJava(perfil: "PRODUTOR"));

        using var resposta = await cliente.PostAsJsonAsync(
            "/api/oportunidades", NovaOportunidade());

        Assert.Equal(HttpStatusCode.Forbidden, resposta.StatusCode);
    }

    [Fact]
    public async Task Produtor_NaoPodeExcluirOportunidade_DeveRetornar403()
    {
        using var aplicacao = new ApiTesteFactory();
        using var cliente = aplicacao.ClienteComToken(
            TokensDeTeste.GerarDaJava(perfil: "PRODUTOR"));

        using var resposta = await cliente.DeleteAsync("/api/oportunidades/1");

        Assert.Equal(HttpStatusCode.Forbidden, resposta.StatusCode);
    }

    [Theory]
    [InlineData("COMPRADOR")]
    [InlineData("GESTOR")]
    [InlineData("comprador")]
    public async Task CompradorOuGestor_PodePublicarOportunidade(string perfil)
    {
        using var aplicacao = new ApiTesteFactory();
        using var cliente = aplicacao.ClienteComToken(
            TokensDeTeste.GerarDaJava(perfil: perfil));

        using var resposta = await cliente.PostAsJsonAsync(
            "/api/oportunidades", NovaOportunidade());

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
    }

    [Theory]
    [InlineData("COMPRADOR")]
    [InlineData("GESTOR")]
    public async Task CompradorOuGestor_PodeExcluirOportunidade(string perfil)
    {
        using var aplicacao = new ApiTesteFactory();
        using var cliente = aplicacao.ClienteComToken(
            TokensDeTeste.GerarDaJava(perfil: perfil));

        // O banco novo traz o seed, então a oportunidade 1 existe.
        using var resposta = await cliente.DeleteAsync("/api/oportunidades/1");

        Assert.Equal(HttpStatusCode.NoContent, resposta.StatusCode);
    }

    [Fact]
    public async Task SemToken_AoPublicarOportunidade_DeveRetornar401()
    {
        using var aplicacao = new ApiTesteFactory();
        using var cliente = aplicacao.CreateClient();

        using var resposta = await cliente.PostAsJsonAsync(
            "/api/oportunidades", NovaOportunidade());

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }
}

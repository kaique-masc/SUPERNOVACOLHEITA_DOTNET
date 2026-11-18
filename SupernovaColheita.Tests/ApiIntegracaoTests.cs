using System.Net;
using Xunit;

namespace SupernovaColheita.Tests;

public class ApiIntegracaoTests
{
    [Theory]
    [InlineData("/api/produtores")]
    [InlineData("/api/produtos")]
    [InlineData("/api/oportunidades")]
    public async Task EndpointSemToken_DeveRetornar401(
        string rota)
    {
        using var aplicacao = new ApiTesteFactory();
        using var cliente = aplicacao.CreateClient();

        var resposta = await cliente.GetAsync(rota);

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            resposta.StatusCode);
    }
}
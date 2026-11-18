using System.Net;
using Xunit;

namespace SupernovaColheita.Tests;

public class RateLimitPorUsuarioTests
{
    [Fact]
    public async Task MesmoIdComEmailsDiferentes_DevemCompartilharOLimite()
    {
        using var aplicacao = new ApiTesteFactory(limitePorMinuto: 2);

        using var primeiro = aplicacao.ClienteComToken(
            TokensDeTeste.GerarDaJava(id: 7, email: "a@supernova.com"));
        using var segundo = aplicacao.ClienteComToken(
            TokensDeTeste.GerarDaJava(id: 7, email: "b@supernova.com"));

        for (var i = 0; i < 2; i++)
        {
            using var permitida = await primeiro.GetAsync(
                "/api/oportunidades");

            Assert.Equal(HttpStatusCode.OK, permitida.StatusCode);
        }

        // A claim "id" é a mesma, então o contador também é o mesmo.
        using var bloqueada = await segundo.GetAsync("/api/oportunidades");

        Assert.Equal(HttpStatusCode.TooManyRequests, bloqueada.StatusCode);
    }

    [Fact]
    public async Task IdsDiferentes_DevemTerLimitesIndependentes()
    {
        using var aplicacao = new ApiTesteFactory(limitePorMinuto: 2);

        using var primeiro = aplicacao.ClienteComToken(
            TokensDeTeste.GerarDaJava(id: 7));
        using var segundo = aplicacao.ClienteComToken(
            TokensDeTeste.GerarDaJava(id: 8));

        for (var i = 0; i < 2; i++)
        {
            using var permitida = await primeiro.GetAsync(
                "/api/oportunidades");

            Assert.Equal(HttpStatusCode.OK, permitida.StatusCode);
        }

        using var outroUsuario = await segundo.GetAsync("/api/oportunidades");

        Assert.Equal(HttpStatusCode.OK, outroUsuario.StatusCode);
    }
}

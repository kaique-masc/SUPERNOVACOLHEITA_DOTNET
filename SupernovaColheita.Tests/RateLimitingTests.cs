using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace SupernovaColheita.Tests;

public class RateLimitingTests
{
    [Fact]
    public async Task SextaRequisicaoDeveRetornar429()
    {
        using var aplicacao = new ApiTesteFactory(limitePorMinuto: 5);
        using var cliente = aplicacao.CreateClient();

        var email = $"rate-{Guid.NewGuid():N}@exemplo.com";

        var cadastro = await cliente.PostAsJsonAsync(
            "/api/auth/cadastro",
            new
            {
                nome = "Usuario Rate Limit",
                email,
                senha = "Teste12345!"
            });

        Assert.Equal(HttpStatusCode.Created, cadastro.StatusCode);

        var login = await cliente.PostAsJsonAsync(
            "/api/auth/login",
            new
            {
                email,
                senha = "Teste12345!"
            });

        Assert.Equal(HttpStatusCode.OK, login.StatusCode);

        using var dadosLogin = JsonDocument.Parse(
            await login.Content.ReadAsStringAsync());

        var token = dadosLogin.RootElement
            .GetProperty("token")
            .GetString();

        Assert.False(string.IsNullOrWhiteSpace(token));

        cliente.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        for (int i = 0; i < 5; i++)
        {
            using var resposta = await cliente.GetAsync(
                "/api/produtores");

            Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        }

        // A sexta requisição deve ser bloqueada.
        using var bloqueada = await cliente.GetAsync(
            "/api/produtores");

        Assert.Equal(
            HttpStatusCode.TooManyRequests,
            bloqueada.StatusCode);
    }
}
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace SupernovaColheita.Tests;

public class AutenticacaoTests
{
    [Fact]
    public async Task CadastroELogin_DeveGerarTokenJWT()
    {
        using var aplicacao = new ApiTesteFactory();
        using var cliente = aplicacao.CreateClient();

        var email = $"teste-{Guid.NewGuid():N}@exemplo.com";

        var cadastro = await cliente.PostAsJsonAsync(
            "/api/auth/cadastro",
            new
            {
                nome = "Produtor Teste",
                email,
                senha = "Teste12345!"
            });

        Assert.Equal(
            HttpStatusCode.Created,
            cadastro.StatusCode);

        var login = await cliente.PostAsJsonAsync(
            "/api/auth/login",
            new
            {
                email,
                senha = "Teste12345!"
            });

        Assert.Equal(
            HttpStatusCode.OK,
            login.StatusCode);

        using var resposta = JsonDocument.Parse(
            await login.Content.ReadAsStringAsync());

        Assert.True(
            resposta.RootElement.TryGetProperty(
                "token", out var token));

        Assert.False(
            string.IsNullOrWhiteSpace(token.GetString()));
    }

    [Fact]
    public async Task LoginComSenhaErrada_DeveRetornar401()
    {
        using var aplicacao = new ApiTesteFactory();
        using var cliente = aplicacao.CreateClient();

        var login = await cliente.PostAsJsonAsync(
            "/api/auth/login",
            new
            {
                email = "inexistente@exemplo.com",
                senha = "SenhaErrada"
            });

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            login.StatusCode);
    }
}
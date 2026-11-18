
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace SupernovaColheita.Tests;

public class IdempotenciaTests
{
    [Fact]
    public async Task MesmaChaveNaoDeveDuplicarProdutor()
    {
        using var aplicacao = new ApiTesteFactory();
        using var cliente = aplicacao.CreateClient();

        var email = $"teste-{Guid.NewGuid():N}@exemplo.com";

        var cadastro = await cliente.PostAsJsonAsync(
            "/api/auth/cadastro",
            new
            {
                nome = "Usuario Teste",
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
            .GetProperty("token").GetString();

        Assert.False(string.IsNullOrWhiteSpace(token));

        cliente.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        using var verificacao = await cliente.GetAsync("/api/produtores");

        Assert.True(
            verificacao.StatusCode == HttpStatusCode.OK,
            $"JWT rejeitado. Status: {(int)verificacao.StatusCode}. " +
            $"Resposta: {await verificacao.Content.ReadAsStringAsync()}");


        var produtor = new
        {
            nome = "Produtor Idempotente",
            cpfCnpj = "12345678901",
            municipio = "Campinas",
            estado = "SP"
        };

        var chave = Guid.NewGuid().ToString();

        using var requisicao1 = new HttpRequestMessage(
            HttpMethod.Post, "/api/produtores");

        requisicao1.Headers.Add("Idempotency-Key", chave);
        requisicao1.Content = JsonContent.Create(produtor);

        using var resposta1 = await cliente.SendAsync(requisicao1);

        Assert.Equal(HttpStatusCode.Created, resposta1.StatusCode);

        using var requisicao2 = new HttpRequestMessage(
            HttpMethod.Post, "/api/produtores");

        requisicao2.Headers.Add("Idempotency-Key", chave);
        requisicao2.Content = JsonContent.Create(produtor);

        using var resposta2 = await cliente.SendAsync(requisicao2);

        Assert.Equal(HttpStatusCode.Created, resposta2.StatusCode);

        using var dados1 = JsonDocument.Parse(
            await resposta1.Content.ReadAsStringAsync());

        using var dados2 = JsonDocument.Parse(
            await resposta2.Content.ReadAsStringAsync());

        
        var json1 = dados1.RootElement.GetRawText();
        var json2 = dados2.RootElement.GetRawText();

        Assert.True(
            dados1.RootElement.TryGetProperty("id", out var id1),
            $"A primeira resposta não contém 'id'. JSON: {json1}");


        Assert.True(
            dados2.RootElement.TryGetProperty("id", out var id2) ||
            dados2.RootElement.TryGetProperty("Id", out id2),
            $"A segunda resposta não contém ID. JSON: {json2}");


        Assert.Equal(id1.GetInt32(), id2.GetInt32());

       
        var produtores = await cliente.GetFromJsonAsync<JsonElement>(
            "/api/produtores");

        Assert.Equal(1, produtores.GetArrayLength());
    }
}

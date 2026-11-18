using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace SupernovaColheita.Tests;

public static class TokensDeTeste
{
    public const string EmissorJava = "supernova-colheita-api";

    /// <summary>
    /// Gera um token no formato da API Java: HS256, sem audience e com as
    /// claims sub (e-mail), id (numérico) e role.
    /// </summary>
    public static string GerarDaJava(
        string emissor = EmissorJava,
        string chave = ApiTesteFactory.ChaveJwt,
        string perfil = "PRODUTOR",
        long id = 7,
        string email = "jose@supernova.com")
    {
        var credenciais = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(chave)),
            SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, email),
            new Claim(
                "id",
                id.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ClaimValueTypes.Integer64),
            new Claim("role", perfil)
        };

        var token = new JwtSecurityToken(
            issuer: emissor,
            audience: null,
            claims: claims,
            notBefore: DateTime.UtcNow.AddMinutes(-1),
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credenciais);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public static HttpClient ClienteComToken(
        this ApiTesteFactory fabrica,
        string token)
    {
        var cliente = fabrica.CreateClient();

        cliente.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        return cliente;
    }
}

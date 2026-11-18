using System.Globalization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SupernovaColheita.Api.Data;

namespace SupernovaColheita.Tests;

public class ApiTesteFactory : WebApplicationFactory<Program>
{
    public const string ChaveJwt =
        "ChaveSupernovaExclusivaParaTestes1234567890";

    private readonly string _caminhoBanco = Path.Combine(
        Path.GetTempPath(),
        $"supernova-testes-{Guid.NewGuid():N}.db");

    private readonly int? _limitePorMinuto;

    /// <param name="limitePorMinuto">
    /// Valor de RateLimit:PermitLimit para o teste. Quando nulo, vale o padrão
    /// do appsettings.json (60 requisições por minuto).
    /// </param>
    public ApiTesteFactory(int? limitePorMinuto = null)
    {
        _limitePorMinuto = limitePorMinuto;
    }

    protected override void ConfigureWebHost(
        IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.UseSetting("Jwt:Key", ChaveJwt);

        var configuracao = new Dictionary<string, string?>
        {
            ["Jwt:Key"] = ChaveJwt,
            ["Jwt:Issuer"] = "SupernovaColheita",
            ["Jwt:Audience"] = "SupernovaColheita",
            ["ConnectionStrings:DefaultConnection"] =
                $"Data Source={_caminhoBanco}"
        };

        if (_limitePorMinuto is { } limite)
        {
            var valor = limite.ToString(CultureInfo.InvariantCulture);

            builder.UseSetting("RateLimit:PermitLimit", valor);
            configuracao["RateLimit:PermitLimit"] = valor;
        }

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(configuracao);
        });

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();

            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite(
                    $"Data Source={_caminhoBanco}"));
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (!disposing)
            return;

        // Libera conexões SQLite mantidas no pool.
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();

        foreach (var caminho in new[]
                 {
                     _caminhoBanco,
                     _caminhoBanco + "-wal",
                     _caminhoBanco + "-shm"
                 })
        {
            if (!File.Exists(caminho))
                continue;

            try
            {
                File.Delete(caminho);
            }
            catch (IOException)
            {
                // No Windows o arquivo pode continuar bloqueado por um instante.
            }
        }
    }
}

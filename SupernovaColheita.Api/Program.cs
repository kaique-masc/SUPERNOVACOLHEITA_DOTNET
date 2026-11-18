

using SupernovaColheita.Api.HealthChecks;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SupernovaColheita.Api.Data;
using SupernovaColheita.Api.Endpoints;
using SupernovaColheita.Api.Models;
using SupernovaColheita.Api.Services;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Supernova Colheita API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Digite Bearer seguido de um espaço e do token JWT.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();

var jwtKey = builder.Configuration["Jwt:Key"];

if (string.IsNullOrWhiteSpace(jwtKey) &&
    builder.Environment.IsDevelopment())
{
    jwtKey = Convert.ToBase64String(
        System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));

    builder.Configuration["Jwt:Key"] = jwtKey;

    Console.WriteLine(
        "Ambiente Development: Jwt__Key não definida; chave JWT temporária " +
        "gerada. Tokens da API Java NÃO serão aceitos: defina Jwt__Key com o " +
        "mesmo valor do JWT_SECRET da API Java.");
}

if (string.IsNullOrWhiteSpace(jwtKey) ||
    Encoding.UTF8.GetByteCount(jwtKey) < 32)
{
    throw new InvalidOperationException(
        "Configure Jwt:Key com pelo menos 32 bytes.");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Aceita o token emitido pela API Java e o emitido por esta API.
        // Se Jwt:Issuers não estiver configurado, vale só Jwt:Issuer.
        var emissoresValidos = builder.Configuration
            .GetSection("Jwt:Issuers")
            .Get<string[]>();

        if (emissoresValidos is not { Length: > 0 })
        {
            emissoresValidos = new[]
            {
                builder.Configuration["Jwt:Issuer"] ?? "SupernovaColheita"
            };
        }

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuers = emissoresValidos,

            // A API Java não coloca audience no token. A segurança continua
            // pela assinatura (chave secreta) e pelo issuer.
            ValidateAudience = false,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey)),

            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization(options =>
{
    // Só COMPRADOR e GESTOR publicam e excluem oportunidades. Aceita o perfil
    // do token da API Java (claim "role") e o do login próprio
    // (ClaimTypes.Role), sem diferenciar maiúsculas de minúsculas.
    options.AddPolicy(PoliticasAutorizacao.PublicarOportunidade, politica =>
        politica
            .RequireAuthenticatedUser()
            .RequireAssertion(contexto => contexto.User.Claims.Any(claim =>
                (claim.Type == ClaimTypes.Role || claim.Type == "role") &&
                (string.Equals(
                     claim.Value, "COMPRADOR",
                     StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(
                     claim.Value, "GESTOR",
                     StringComparison.OrdinalIgnoreCase)))));
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode =
        StatusCodes.Status429TooManyRequests;

    options.AddPolicy("limite-api", context =>
    {
        // Identificação do usuário: primeiro a claim "id" (token da API Java),
        // depois o identificador do login próprio (sub/NameIdentifier) e, sem
        // token válido, o IP. Os prefixos evitam que ids de origens diferentes
        // dividam o mesmo contador.
        var claimId = context.User.FindFirst("id")?.Value;
        var claimSub = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                       ?? context.User.FindFirst("sub")?.Value;
        var ip = context.Connection.RemoteIpAddress?.ToString()
                 ?? "desconhecido";

        string identificador;

        if (context.User.Identity?.IsAuthenticated == true &&
            !string.IsNullOrWhiteSpace(claimId))
            identificador = $"id:{claimId}";
        else if (context.User.Identity?.IsAuthenticated == true &&
                 !string.IsNullOrWhiteSpace(claimSub))
            identificador = $"sub:{claimSub}";
        else
            identificador = $"ip:{ip}";

        // Limite configurável: RateLimit:PermitLimit no appsettings.json ou
        // variável RateLimit__PermitLimit (ex.: 5 para demonstrar o 429).
        // É lido por requisição para respeitar também a configuração dos testes.
        var limitePorMinuto = context.RequestServices
            .GetRequiredService<IConfiguration>()
            .GetValue("RateLimit:PermitLimit", 60);

        if (limitePorMinuto < 1)
            limitePorMinuto = 60;

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: identificador,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = limitePorMinuto,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            });
    });
});


builder.Services.AddHttpClient("health-externo", client =>
{
    client.Timeout = TimeSpan.FromSeconds(5);
});


builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>("banco-de-dados")
    .AddCheck<OpenMeteoHealthCheck>("servico-externo");



builder.Services.AddHttpClient<ClimaService>(client =>
{
    client.BaseAddress =
        new Uri("https://api.open-meteo.com/");

    client.Timeout = TimeSpan.FromSeconds(5);
});


// CORS: necessário para o app rodando no navegador (versão web do Expo).
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(politica =>
        politica.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger(options =>
    {
        options.SerializeAsV2 = true;
    });

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
            "/swagger/v1/swagger.json",
            "Supernova Colheita API v1");
    });
}


app.UseRouting();
app.UseCors();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseRateLimiter();
app.UseAuthorization();


using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();

    // Oportunidades de exemplo para o app não abrir vazio (só se a tabela estiver vazia).
    DadosIniciais.Semear(db);
}


app.MapProdutorEndpoints();
app.MapProdutoEndpoints();
app.MapOportunidadeEndpoints();
app.MapAuthEndpoints();

app.MapHealthChecks("/health")
    .AllowAnonymous();

app.MapClimaEndpoints();

app.Run();


public partial class Program { }

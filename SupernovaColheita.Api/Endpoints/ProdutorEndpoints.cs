
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SupernovaColheita.Api.Data;
using SupernovaColheita.Api.Models;

namespace SupernovaColheita.Api.Endpoints;

public static class ProdutorEndpoints
{
    public static void MapProdutorEndpoints(this WebApplication app)
    {
        var grupo = app.MapGroup("/api/produtores")
            .WithTags("Produtores")
            .RequireAuthorization()
            .RequireRateLimiting("limite-api");

        grupo.MapGet("/", async (AppDbContext db) =>
            await db.Produtores.AsNoTracking().ToListAsync())
            .WithSummary("Listar produtores")
            .Produces<List<Produtor>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status429TooManyRequests);

        grupo.MapGet("/{id:int}", async (int id, AppDbContext db) =>
        {
            var produtor = await db.Produtores.FindAsync(id);

            return produtor is null
                ? Results.NotFound()
                : Results.Ok(produtor);
        })
        .WithSummary("Consultar produtor por ID")
        .Produces<Produtor>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status429TooManyRequests);

        grupo.MapPost("/", async (
            Produtor produtor,
            HttpContext http,
            AppDbContext db) =>
        {
            var erros = new List<ValidationResult>();
            var contexto = new ValidationContext(produtor);

            if (!Validator.TryValidateObject(produtor, contexto, erros, true))
            {
                return Results.ValidationProblem(
                    erros.GroupBy(e =>
                            e.MemberNames.FirstOrDefault() ?? "Geral")
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(e =>
                                e.ErrorMessage ?? "Inválido").ToArray()));
            }

            var chave = http.Request.Headers["Idempotency-Key"].ToString();

            if (string.IsNullOrWhiteSpace(chave) || chave.Length > 128)
            {
                return Results.BadRequest(
                    "Informe um Idempotency-Key válido de até 128 caracteres.");
            }

            const string rota = "POST:/api/produtores";

            var jsonRequisicao = JsonSerializer.Serialize(new
            {
                produtor.Nome,
                produtor.CpfCnpj,
                produtor.Municipio,
                produtor.Estado
            });

            var hash = Convert.ToHexString(
                SHA256.HashData(Encoding.UTF8.GetBytes(jsonRequisicao)));

            await using var transacao =
                await db.Database.BeginTransactionAsync(
                    System.Data.IsolationLevel.Serializable);

            var registro = await db.RegistrosIdempotencia
                .AsNoTracking()
                .FirstOrDefaultAsync(r =>
                    r.Chave == chave && r.Rota == rota);

            if (registro is not null)
            {
                if (registro.HashRequisicao != hash)
                {
                    return Results.Conflict(
                        "Esta chave já foi utilizada com outros dados.");
                }

                http.Response.Headers.Location =
                    $"/api/produtores/{JsonSerializer.Deserialize<Produtor>(
                        registro.RespostaJson,
                        new JsonSerializerOptions(JsonSerializerDefaults.Web))!.Id}";

                return Results.Content(
                    registro.RespostaJson,
                    "application/json",
                    statusCode: registro.StatusCode);
            }

            db.Produtores.Add(produtor);
            await db.SaveChangesAsync();

            var resposta = JsonSerializer.Serialize(
                produtor,
                new JsonSerializerOptions(JsonSerializerDefaults.Web));

            db.RegistrosIdempotencia.Add(
                new RegistroIdempotencia
                {
                    Chave = chave,
                    Rota = rota,
                    HashRequisicao = hash,
                    RespostaJson = resposta,
                    StatusCode = StatusCodes.Status201Created
                });

            await db.SaveChangesAsync();
            await transacao.CommitAsync();

            return Results.Created(
                $"/api/produtores/{produtor.Id}",
                produtor);
        })
        .WithSummary("Cadastrar produtor")
        .WithDescription(
            "Exige o cabeçalho Idempotency-Key para evitar cadastros duplicados.")
        .Produces<Produtor>(StatusCodes.Status201Created)
        .ProducesValidationProblem()
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status409Conflict)
        .Produces(StatusCodes.Status429TooManyRequests);

        grupo.MapDelete("/{id:int}", async (
            int id,
            AppDbContext db) =>
        {
            var produtor = await db.Produtores.FindAsync(id);

            if (produtor is null)
                return Results.NotFound();

            db.Produtores.Remove(produtor);
            await db.SaveChangesAsync();

            return Results.NoContent();
        })
        .WithSummary("Excluir produtor")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status429TooManyRequests);
    }
}



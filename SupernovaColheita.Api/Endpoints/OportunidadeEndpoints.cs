
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using SupernovaColheita.Api.Data;
using SupernovaColheita.Api.Models;

namespace SupernovaColheita.Api.Endpoints;

public static class OportunidadeEndpoints
{
    public static void MapOportunidadeEndpoints(this WebApplication app)
    {
        var grupo = app.MapGroup("/api/oportunidades")
            .WithTags("Oportunidades")
            .RequireAuthorization()
            .RequireRateLimiting("limite-api");

        grupo.MapGet("/", async (AppDbContext db) =>
            await db.Oportunidades.AsNoTracking().ToListAsync())
            .WithSummary("Listar oportunidades")
            .Produces<List<Oportunidade>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status429TooManyRequests);

        grupo.MapGet("/{id:int}", async (int id, AppDbContext db) =>
        {
            var oportunidade = await db.Oportunidades.FindAsync(id);

            return oportunidade is null
                ? Results.NotFound()
                : Results.Ok(oportunidade);
        })
        .WithSummary("Consultar oportunidade por ID")
        .Produces<Oportunidade>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status429TooManyRequests);

        grupo.MapPost("/", async (
            Oportunidade oportunidade,
            AppDbContext db) =>
        {
            var erros = new List<ValidationResult>();

            if (!Validator.TryValidateObject(
                    oportunidade,
                    new ValidationContext(oportunidade),
                    erros,
                    true))
            {
                return Results.ValidationProblem(
                    erros.GroupBy(e =>
                            e.MemberNames.FirstOrDefault() ?? "Geral")
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(e =>
                                e.ErrorMessage ?? "Inválido").ToArray()));
            }

            db.Oportunidades.Add(oportunidade);
            await db.SaveChangesAsync();

            return Results.Created(
                $"/api/oportunidades/{oportunidade.Id}",
                oportunidade);
        })
        .WithSummary("Cadastrar oportunidade")
        .WithDescription("Apenas perfis COMPRADOR e GESTOR.")
        .RequireAuthorization(PoliticasAutorizacao.PublicarOportunidade)
        .Produces<Oportunidade>(StatusCodes.Status201Created)
        .ProducesValidationProblem()
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status429TooManyRequests);

        grupo.MapDelete("/{id:int}", async (int id, AppDbContext db) =>
        {
            var oportunidade = await db.Oportunidades.FindAsync(id);

            if (oportunidade is null)
                return Results.NotFound();

            db.Oportunidades.Remove(oportunidade);
            await db.SaveChangesAsync();

            return Results.NoContent();
        })
        .WithSummary("Excluir oportunidade")
        .WithDescription("Apenas perfis COMPRADOR e GESTOR.")
        .RequireAuthorization(PoliticasAutorizacao.PublicarOportunidade)
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status429TooManyRequests);
    }
}

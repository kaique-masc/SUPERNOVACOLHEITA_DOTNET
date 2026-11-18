
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using SupernovaColheita.Api.Data;
using SupernovaColheita.Api.Models;

namespace SupernovaColheita.Api.Endpoints;

public static class ProdutoEndpoints
{
    public static void MapProdutoEndpoints(this WebApplication app)
    {
        var grupo = app.MapGroup("/api/produtos")
            .WithTags("Produtos")
            .RequireAuthorization()
            .RequireRateLimiting("limite-api");

        grupo.MapGet("/", async (AppDbContext db) =>
            await db.Produtos.AsNoTracking().ToListAsync())
            .WithSummary("Listar produtos")
            .Produces<List<Produto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status429TooManyRequests);

        grupo.MapGet("/{id:int}", async (int id, AppDbContext db) =>
        {
            var produto = await db.Produtos.FindAsync(id);

            return produto is null
                ? Results.NotFound()
                : Results.Ok(produto);
        })
        .WithSummary("Consultar produto por ID")
        .Produces<Produto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status429TooManyRequests);

        grupo.MapPost("/", async (Produto produto, AppDbContext db) =>
        {
            var erros = new List<ValidationResult>();

            if (!Validator.TryValidateObject(
                    produto, new ValidationContext(produto), erros, true))
            {
                return Results.ValidationProblem(
                    erros.GroupBy(e =>
                            e.MemberNames.FirstOrDefault() ?? "Geral")
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(e =>
                                e.ErrorMessage ?? "Inválido").ToArray()));
            }

            if (!await db.Produtores.AnyAsync(p => p.Id == produto.ProdutorId))
                return Results.BadRequest("Produtor não encontrado.");

            produto.Produtor = null;
            db.Produtos.Add(produto);
            await db.SaveChangesAsync();

            return Results.Created(
                $"/api/produtos/{produto.Id}", produto);
        })
        .WithSummary("Cadastrar produto")
        .Produces<Produto>(StatusCodes.Status201Created)
        .ProducesValidationProblem()
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status429TooManyRequests);

        grupo.MapDelete("/{id:int}", async (int id, AppDbContext db) =>
        {
            var produto = await db.Produtos.FindAsync(id);

            if (produto is null)
                return Results.NotFound();

            db.Produtos.Remove(produto);
            await db.SaveChangesAsync();

            return Results.NoContent();
        })
        .WithSummary("Excluir produto")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status429TooManyRequests);
    }
}

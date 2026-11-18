
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SupernovaColheita.Api.Data;
using SupernovaColheita.Api.Models;
using SupernovaColheita.Api.Services;

namespace SupernovaColheita.Api.Endpoints;

public record CadastroRequest(string Nome, string Email, string Senha);
public record LoginRequest(string Email, string Senha);

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var grupo = app.MapGroup("/api/auth").WithTags("Autenticação");

        grupo.MapPost("/cadastro", async (
            CadastroRequest request,
            AppDbContext db,
            IPasswordHasher<Usuario> hasher) =>
        {
            if (string.IsNullOrWhiteSpace(request.Nome) ||
                !new EmailAddressAttribute().IsValid(request.Email) ||
                string.IsNullOrWhiteSpace(request.Senha) ||
                request.Senha.Length < 8)
                return Results.BadRequest(
                    "Informe nome, e-mail válido e senha com pelo menos 8 caracteres.");

            var email = request.Email.Trim().ToLowerInvariant();

            if (await db.Usuarios.AnyAsync(u => u.Email == email))
                return Results.Conflict("E-mail já cadastrado.");

            var usuario = new Usuario
            {
                Nome = request.Nome.Trim(),
                Email = email,
                Perfil = "Produtor"
            };

            usuario.SenhaHash = hasher.HashPassword(usuario, request.Senha);

            db.Usuarios.Add(usuario);
            await db.SaveChangesAsync();

            return Results.Created(
                $"/api/usuarios/{usuario.Id}",
                new { usuario.Id, usuario.Nome, usuario.Email, usuario.Perfil });
        })
        .WithSummary("Cadastrar usuário")
        .WithDescription("Cadastra um usuário com nome, e-mail e senha.")
        .Produces(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status409Conflict);

        grupo.MapPost("/login", async (
            LoginRequest request,
            AppDbContext db,
            IPasswordHasher<Usuario> hasher,
            TokenService tokens) =>
        {
            var email = request.Email?.Trim().ToLowerInvariant();

            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(request.Senha))
                return Results.Unauthorized();

            var usuario = await db.Usuarios
                .FirstOrDefaultAsync(u => u.Email == email);

            if (usuario is null ||
                hasher.VerifyHashedPassword(
                    usuario, usuario.SenhaHash, request.Senha)
                == PasswordVerificationResult.Failed)
                return Results.Unauthorized();

            return Results.Ok(new
            {
                token = tokens.GerarToken(usuario),
                tipo = "Bearer",
                expiraEmSegundos = 7200
            });
        })
        .WithSummary("Autenticar usuário")
        .WithDescription("Valida as credenciais e retorna um token JWT.")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized);
    }
}

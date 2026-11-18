using System.ComponentModel.DataAnnotations;

namespace SupernovaColheita.Api.Models;

public class Usuario
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Nome { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string SenhaHash { get; set; } = string.Empty;

    [Required]
    public string Perfil { get; set; } = "Produtor";

    public DateTime DataCadastro { get; set; } = DateTime.UtcNow;
}
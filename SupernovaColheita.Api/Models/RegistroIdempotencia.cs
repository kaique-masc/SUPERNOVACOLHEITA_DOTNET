using System.ComponentModel.DataAnnotations;

namespace SupernovaColheita.Api.Models;

public class RegistroIdempotencia
{
    public int Id { get; set; }

    [Required]
    public string Chave { get; set; } = string.Empty;

    [Required]
    public string Rota { get; set; } = string.Empty;

    [Required]
    public string HashRequisicao { get; set; } = string.Empty;

    public string RespostaJson { get; set; } = string.Empty;

    public int StatusCode { get; set; }

    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
}
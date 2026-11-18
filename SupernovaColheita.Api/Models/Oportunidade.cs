using System.ComponentModel.DataAnnotations;

namespace SupernovaColheita.Api.Models;

public class Oportunidade
{
    public int Id { get; set; }

    [Required]
    [StringLength(150)]
    public string Titulo { get; set; } = string.Empty;

    [Required]
    [StringLength(150)]
    public string Instituicao { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Municipio { get; set; } = string.Empty;

    [Required]
    [StringLength(2, MinimumLength = 2)]
    public string Estado { get; set; } = string.Empty;

    [Range(0.01, double.MaxValue)]
    public decimal ValorEstimado { get; set; }

    public DateTime PrazoInscricao { get; set; }

    public bool Ativa { get; set; } = true;
}
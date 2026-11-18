using System.ComponentModel.DataAnnotations;

namespace SupernovaColheita.Api.Models;

public class Produtor
{
    public int Id { get; set; }

    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(150)]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "O CPF ou CNPJ é obrigatório.")]
    [StringLength(18)]
    public string CpfCnpj { get; set; } = string.Empty;

    [Required(ErrorMessage = "O município é obrigatório.")]
    [StringLength(100)]
    public string Municipio { get; set; } = string.Empty;

    [Required(ErrorMessage = "O estado é obrigatório.")]
    [StringLength(2, MinimumLength = 2)]
    public string Estado { get; set; } = string.Empty;

    public DateTime DataCadastro { get; set; } = DateTime.UtcNow;
}
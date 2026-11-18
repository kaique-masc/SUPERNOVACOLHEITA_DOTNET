using System.ComponentModel.DataAnnotations;

namespace SupernovaColheita.Api.Models;

public class Produto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "O nome do produto é obrigatório.")]
    [StringLength(150)]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "A categoria é obrigatória.")]
    [StringLength(100)]
    public string Categoria { get; set; } = string.Empty;

    [Range(0.01, double.MaxValue,
        ErrorMessage = "O preço deve ser maior que zero.")]
    public decimal Preco { get; set; }

    [Range(0, double.MaxValue,
        ErrorMessage = "A quantidade não pode ser negativa.")]
    public decimal QuantidadeDisponivel { get; set; }

    [Required]
    [StringLength(20)]
    public string UnidadeMedida { get; set; } = string.Empty;

    [Range(1, int.MaxValue,
        ErrorMessage = "Informe um produtor válido.")]
    public int ProdutorId { get; set; }

    public Produtor? Produtor { get; set; }

    public DateTime DataCadastro { get; set; } = DateTime.UtcNow;
}
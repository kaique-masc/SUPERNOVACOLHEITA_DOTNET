
using System.ComponentModel.DataAnnotations;
using SupernovaColheita.Api.Models;
using Xunit;

namespace SupernovaColheita.Tests;

public class ValidacaoModelsTests
{
    private static bool ModeloValido(object modelo)
    {
        var contexto = new ValidationContext(modelo);
        var resultados = new List<ValidationResult>();

        return Validator.TryValidateObject(
            modelo,
            contexto,
            resultados,
            validateAllProperties: true);
    }

    [Fact]
    public void Produtor_ComDadosValidos_DeveSerValido()
    {
        var produtor = new Produtor
        {
            Nome = "Joao Silva",
            CpfCnpj = "12345678901",
            Municipio = "Campinas",
            Estado = "SP"
        };

        Assert.True(ModeloValido(produtor));
    }

    [Fact]
    public void Produtor_SemNome_DeveSerInvalido()
    {
        var produtor = new Produtor
        {
            Nome = "",
            CpfCnpj = "12345678901",
            Municipio = "Campinas",
            Estado = "SP"
        };

        Assert.False(ModeloValido(produtor));
    }

    [Fact]
    public void Produtor_EstadoComTresLetras_DeveSerInvalido()
    {
        var produtor = new Produtor
        {
            Nome = "Joao Silva",
            CpfCnpj = "12345678901",
            Municipio = "Campinas",
            Estado = "SPP"
        };

        Assert.False(ModeloValido(produtor));
    }

    [Fact]
    public void Produto_PrecoNegativo_DeveSerInvalido()
    {
        var produto = new Produto
        {
            Nome = "Tomate",
            Categoria = "Hortalicas",
            Preco = -10,
            QuantidadeDisponivel = 100,
            UnidadeMedida = "kg",
            ProdutorId = 1
        };

        Assert.False(ModeloValido(produto));
    }

    [Fact]
    public void Produto_QuantidadeNegativa_DeveSerInvalido()
    {
        var produto = new Produto
        {
            Nome = "Tomate",
            Categoria = "Hortalicas",
            Preco = 10,
            QuantidadeDisponivel = -5,
            UnidadeMedida = "kg",
            ProdutorId = 1
        };

        Assert.False(ModeloValido(produto));
    }

    [Fact]
    public void Produto_ComDadosValidos_DeveSerValido()
    {
        var produto = new Produto
        {
            Nome = "Tomate",
            Categoria = "Hortalicas",
            Preco = 10,
            QuantidadeDisponivel = 100,
            UnidadeMedida = "kg",
            ProdutorId = 1
        };

        Assert.True(ModeloValido(produto));
    }

    [Fact]
    public void Oportunidade_ValorNegativo_DeveSerInvalida()
    {
        var oportunidade = new Oportunidade
        {
            Titulo = "Compra de alimentos",
            Instituicao = "Prefeitura",
            Municipio = "Campinas",
            Estado = "SP",
            ValorEstimado = -100,
            PrazoInscricao = DateTime.UtcNow.AddDays(10)
        };

        Assert.False(ModeloValido(oportunidade));
    }

    [Fact]
    public void Oportunidade_ComDadosValidos_DeveSerValida()
    {
        var oportunidade = new Oportunidade
        {
            Titulo = "Compra de alimentos",
            Instituicao = "Prefeitura",
            Municipio = "Campinas",
            Estado = "SP",
            ValorEstimado = 5000,
            PrazoInscricao = DateTime.UtcNow.AddDays(10)
        };

        Assert.True(ModeloValido(oportunidade));
    }
}

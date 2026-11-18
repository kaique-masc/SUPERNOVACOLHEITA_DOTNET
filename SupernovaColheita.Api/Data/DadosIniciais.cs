using SupernovaColheita.Api.Models;

namespace SupernovaColheita.Api.Data;

/// <summary>
/// Dados de exemplo para a demonstração: o app não deve abrir sem oportunidades.
/// Os prazos são calculados a partir de hoje, então sempre ficam no futuro.
/// </summary>
public static class DadosIniciais
{
    public static void Semear(AppDbContext db)
    {
        if (db.Oportunidades.Any())
            return;

        var hoje = DateTime.UtcNow.Date;

        db.Oportunidades.AddRange(
            new Oportunidade
            {
                Titulo = "Chamada pública: hortaliças e legumes para a merenda",
                Instituicao = "Prefeitura Municipal de Campinas - Secretaria de Educação",
                Municipio = "Campinas",
                Estado = "SP",
                ValorEstimado = 48000m,
                PrazoInscricao = hoje.AddDays(20),
                Ativa = true
            },
            new Oportunidade
            {
                Titulo = "Aquisição de frutas para a merenda escolar",
                Instituicao = "EMEF Professora Maria Aparecida",
                Municipio = "Ribeirão Preto",
                Estado = "SP",
                ValorEstimado = 27500m,
                PrazoInscricao = hoje.AddDays(30),
                Ativa = true
            },
            new Oportunidade
            {
                Titulo = "Compra de ovos e laticínios da agricultura familiar",
                Instituicao = "Escola Estadual Dom Pedro II",
                Municipio = "Londrina",
                Estado = "PR",
                ValorEstimado = 36200m,
                PrazoInscricao = hoje.AddDays(45),
                Ativa = true
            },
            new Oportunidade
            {
                Titulo = "Chamada pública: raízes e tubérculos",
                Instituicao = "Prefeitura Municipal de Uberlândia",
                Municipio = "Uberlândia",
                Estado = "MG",
                ValorEstimado = 61000m,
                PrazoInscricao = hoje.AddDays(60),
                Ativa = true
            },
            new Oportunidade
            {
                Titulo = "Fornecimento de verduras para restaurante popular",
                Instituicao = "Secretaria de Assistência Social de Sorocaba",
                Municipio = "Sorocaba",
                Estado = "SP",
                ValorEstimado = 19800m,
                PrazoInscricao = hoje.AddDays(90),
                Ativa = true
            });

        db.SaveChanges();
    }
}

using System;
using System.Collections.Generic;
using SmartLMS.Dominio;
using SmartLMS.Dominio.Entidades;

namespace SmartLMS.WebUI.Models
{
    public class AulaViewModel
    {
        public DateTime DataInclusao { get; set; }

        public string NomeCurso { get; set; }

        public Guid Id { get; set; }
        public IEnumerable<ArquivoViewModel> Arquivos { get; private set; }
        public string Conteudo { get; private set; }
        public string Nome { get; private set; }
        public string NomeProfessor { get; private set; }
        public TipoConteudo TipoConteudo { get; private set; }

        internal static IEnumerable<AulaViewModel> FromEntityList(IEnumerable<Aula> aulas, int profundidade)
        {
            foreach (var item in aulas)
            {
                yield return FromEntity(item, profundidade);
            }
        }

        private static AulaViewModel FromEntity(Aula item, int profundidade)
        {
            return new AulaViewModel
            {
                Id = item.Id,
                Nome = item.Nome,
                Conteudo = item.Conteudo,
                TipoConteudo = item.Tipo,
                NomeProfessor = item.Professor.Nome,
                DataInclusao = item.DataInclusao,
                NomeCurso = item.Curso.Nome,
                Arquivos = profundidade > 3 ? ArquivoViewModel.FromEntityList(item.Arquivos) : new List<ArquivoViewModel>()
            };
        }
    }
}
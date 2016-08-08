using System;
using System.Collections.Generic;
using SmartLMS.Dominio;
using SmartLMS.Dominio.Entidades;
using SmartLMS.Dominio.Repositorios;
using Humanizer.DateTimeHumanizeStrategy;
using System.Globalization;
using System.Linq;

namespace SmartLMS.WebUI.Models
{
    public class AulaViewModel
    {
        public bool Disponivel { get; set; }
        public DateTime DataInclusao { get; set; }
        public string DataInclusaoTexto { get; set; }

        public string NomeCurso { get; set; }

        public decimal Percentual { get; set; }

        public decimal Segundos { get; set; }

        public Guid IdCurso { get; set; }

        public Guid Id { get; set; }
        public IEnumerable<ArquivoViewModel> Arquivos { get; private set; }
        public string Conteudo { get; private set; }
        public string Nome { get; private set; }
        public string NomeProfessor { get; private set; }
        public TipoConteudo TipoConteudo { get; private set; }

        internal static IEnumerable<AulaViewModel> FromEntityList(IEnumerable<Aula> aulas, int profundidade, DefaultDateTimeHumanizeStrategy humanizer)
        {
            foreach (var item in aulas)
            {
                yield return FromEntity(item, profundidade, humanizer);
            }
        }

        public static AulaViewModel FromEntity(Aula item, int profundidade, DefaultDateTimeHumanizeStrategy humanizer)
        {
            
            return new AulaViewModel
            {
                Id = item.Id,
                IdCurso = item.Curso.Id,
                Nome = item.Nome,
                Conteudo = item.Conteudo,
                TipoConteudo = item.Tipo,
                NomeProfessor = item.Professor.Nome,
                DataInclusaoTexto = humanizer != null? humanizer.Humanize(item.DataInclusao, DateTime.Now, CultureInfo.CurrentUICulture) : string.Empty,
                DataInclusao = item.DataInclusao,
                NomeCurso = item.Curso.Nome,
                Arquivos = profundidade > 3 ? ArquivoViewModel.FromEntityList(item.Arquivos) : new List<ArquivoViewModel>()
            };
        }

        internal static IEnumerable<AulaViewModel> FromEntityList(IOrderedEnumerable<Aula> aulas, int profundidade)
        {
            return FromEntityList(aulas, profundidade, null);
        }

        internal static IEnumerable<AulaViewModel> FromEntityList(IEnumerable<AulaInfo> aulasInfo)
        {
            foreach (var item in aulasInfo)
            {
                yield return FromEntity(item);
            }
        }

        public static AulaViewModel FromEntity(AulaInfo item)
        {
            return new AulaViewModel
            {
                Id = item.Aula.Id,
                IdCurso = item.Aula.Curso.Id,
                Nome = item.Aula.Nome,
                Conteudo = item.Aula.Conteudo,
                TipoConteudo = item.Aula.Tipo,
                NomeProfessor = item.Aula.Professor.Nome,
                DataInclusao = item.Aula.DataInclusao,
                NomeCurso = item.Aula.Curso.Nome,
                Disponivel = item.Disponivel,
                Percentual = item.Percentual,
                Segundos = item.Segundos
            };
        }

        public static AulaViewModel FromEntityComArquivos(AulaInfo item)
        {
            return new AulaViewModel
            {
                Id = item.Aula.Id,
                IdCurso = item.Aula.Curso.Id,
                Nome = item.Aula.Nome,
                Conteudo = item.Aula.Conteudo,
                TipoConteudo = item.Aula.Tipo,
                NomeProfessor = item.Aula.Professor.Nome,
                DataInclusao = item.Aula.DataInclusao,
                NomeCurso = item.Aula.Curso.Nome,
                Disponivel = item.Disponivel,
                Percentual = item.Percentual,
                Segundos = item.Segundos,
                Arquivos = ArquivoViewModel.FromEntityList(item.Aula.Arquivos.Where(x => x.Ativo))
            };
        }

        
    }
}
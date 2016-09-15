using Carubbi.GenericRepository;
using Humanizer.DateTimeHumanizeStrategy;
using SmartLMS.Dominio.Entidades.Conteudo;
using SmartLMS.Dominio.Entidades.Liberacao;
using SmartLMS.Dominio.Entidades.Pessoa;
using SmartLMS.Dominio.Repositorios;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;

namespace SmartLMS.WebUI.Models
{
    public class AulaViewModel
    {
        [Required(ErrorMessage = "Selecione um professor")]
        [Display(Name = "Professor")]
        public Guid IdProfessor { get; set; }

        [Required]
        public int Ordem { get; set; }

        [Display(Name="Dias para liberaçao")]
        [Required]
        public int DiasLiberacao { get; set; }


        public bool Disponivel { get; set; }
        
        public string DataCriacaoTexto { get; set; }


        public string NomeCurso { get; set; }
 

        public decimal Percentual { get; set; }

        public decimal Segundos { get; set; }

        [Required(ErrorMessage = "Selecione um curso")]
        public Guid IdCurso { get; set; }

        public Guid Id { get; set; }

        [Display(Name = "Material de Apoio")]
        public IEnumerable<ArquivoViewModel> Arquivos { get;  set; }

        [Required]
        public string Conteudo { get; set; }

        [Required]
        public string Nome { get; set; }
        public string NomeProfessor { get;  set; }

        [Required( ErrorMessage = "Selecione o tipo de aula")]
        [Display(Name = "Tipo de Aula")]
        public TipoConteudo TipoConteudo { get;  set; }

        public DateTime DataLiberacao { get;  set; }

        public string DataLiberacaoTexto { get;  set; }

        public string NomeAssunto { get;  set; }
        public string NomeAreaConhecimento { get;  set; }

        public DateTime DataCriacao { get;  set; }


        public bool Ativo { get;  set; }

        internal static IEnumerable<AulaViewModel> FromEntityList(IEnumerable<Aula> aulas, int profundidade, DefaultDateTimeHumanizeStrategy humanizer)
        {
            foreach (var item in aulas)
            {
                yield return FromEntity(item, profundidade, humanizer);
            }
        }


        internal static PagedListResult<AulaViewModel> FromEntityList(PagedListResult<Aula> aulas)
        {
            PagedListResult<AulaViewModel> pagina = new PagedListResult<AulaViewModel>();

            pagina.HasNext = aulas.HasNext;
            pagina.HasPrevious = aulas.HasPrevious;
            pagina.Count = aulas.Count;
            List<AulaViewModel> viewModels = new List<AulaViewModel>();
            foreach (var item in aulas.Entities)
            {
                viewModels.Add(FromEntity(item));
            }

            pagina.Entities = viewModels;
            return pagina;
        }

        private static AulaViewModel FromEntity(Aula item)
        {
            return new AulaViewModel
            {
                Id = item.Id,
                IdCurso = item.Curso.Id,
                Nome = item.Nome,
                Conteudo = item.Conteudo,
                TipoConteudo = item.Tipo,
                NomeProfessor = item.Professor.Nome,
                NomeCurso = item.Curso.Nome,
                NomeAssunto = item.Curso.Assunto.Nome,
                NomeAreaConhecimento = item.Curso.Assunto.AreaConhecimento.Nome,
                DataCriacao = item.DataCriacao,
                Ativo = item.Ativo,
                Ordem = item.Ordem,
                DiasLiberacao = item.DiasLiberacao
            };
        }

        public static AulaViewModel FromEntity(Aula item, int profundidade, DefaultDateTimeHumanizeStrategy humanizer)
        {
            
            return new AulaViewModel
            {
                Id = item.Id,
                IdProfessor = item.Professor.Id,
                NomeProfessor = item.Professor.Nome,
                IdCurso = item.Curso.Id,
                NomeCurso = item.Curso.Nome,

                Nome = item.Nome,
                Conteudo = item.Conteudo,
                TipoConteudo = item.Tipo,
                DataCriacao = item.DataCriacao,
                DiasLiberacao = item.DiasLiberacao,
                DataCriacaoTexto = humanizer != null? humanizer.Humanize(item.DataCriacao, DateTime.Now, CultureInfo.CurrentUICulture) : string.Empty,
                Ativo = item.Ativo,
                Ordem = item.Ordem,
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
                DataCriacao = item.Aula.DataCriacao,
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
                DataCriacao = item.Aula.DataCriacao,
                NomeCurso = item.Aula.Curso.Nome,
                Disponivel = item.Disponivel,
                Percentual = item.Percentual,
                Segundos = item.Segundos,
                Arquivos = ArquivoViewModel.FromEntityList(item.Aula.Arquivos.Where(x => x.Ativo))
            };
        }

        internal static IEnumerable<AulaViewModel> FromEntityList(IEnumerable<AulaPlanejamento> aulas, DefaultDateTimeHumanizeStrategy humanizer)
        {
            foreach (var item in aulas)
            {
                yield return FromEntity(item, humanizer);
            }
        }

        private static AulaViewModel FromEntity(AulaPlanejamento item, DefaultDateTimeHumanizeStrategy humanizer)
        {
            return new AulaViewModel
            {
                Id = item.Aula.Id,
                IdCurso = item.Aula.Curso.Id,
                Nome = item.Aula.Nome,
                Conteudo = item.Aula.Conteudo,
                TipoConteudo = item.Aula.Tipo,
                NomeProfessor = item.Aula.Professor.Nome,
                DataCriacao = item.Aula.DataCriacao,
                DataLiberacao = item.DataLiberacao,
                NomeCurso = item.Aula.Curso.Nome,
                DataCriacaoTexto = humanizer != null ? humanizer.Humanize(item.Aula.DataCriacao, DateTime.Now, CultureInfo.CurrentUICulture) : string.Empty,
                DataLiberacaoTexto = humanizer != null ? humanizer.Humanize(item.DataLiberacao, DateTime.Now, CultureInfo.CurrentUICulture) : string.Empty,
                Disponivel = true,
            };
        }

        internal static Aula ToEntity(AulaViewModel aula, Curso curso, Professor professor)
        {
            return new Aula
            {
                Id = aula.Id,
                DataCriacao = aula.DataCriacao,
                Ativo = aula.Ativo,
                DiasLiberacao = aula.DiasLiberacao,
                Conteudo = aula.Conteudo,
                Nome = aula.Nome,
                Ordem = aula.Ordem,
                Tipo = aula.TipoConteudo,
                Professor = professor,
                Curso = curso
            };
        }
    }
}
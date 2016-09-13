using Carubbi.GenericRepository;
using SmartLMS.Dominio.Entidades.Conteudo;
using SmartLMS.Dominio.Entidades.Liberacao;
using SmartLMS.Dominio.Entidades.Pessoa;
using SmartLMS.Dominio.Repositorios;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace SmartLMS.WebUI.Models
{
    public class CursoViewModel
    {
        [Required(ErrorMessage = "Selecione um professor responsável")]
        [Display(Name = "Professor responsável"  )]
        public Guid IdProfessorResponsavel { get; set; }



        public string NomeAssunto { get; set; }

        [Required(ErrorMessage = "Selecione um assunto")]
        [Display(Name = "Assunto")]
        public Guid IdAssunto { get; set; }

        [Required]
        public int Ordem { get; set; }

        [Required]
        public string Nome { get; set; }

        public Guid Id { get; set; }

        public bool Ativo { get; set; }

        public string NomeArea { get; set; }

        public DateTime DataCriacao { get; set; }

        public string Imagem { get; set; }

        public string NomeProfessorResponsavel { get; set; }

        internal static IEnumerable<CursoViewModel> FromEntityList(IEnumerable<Curso> cursos, int profundidade)
        {
            foreach (var item in cursos)
            {
                yield return FromEntity(item, profundidade);
            }
        }

        internal static PagedListResult<CursoViewModel> FromEntityList(PagedListResult<Curso> cursos)
        {
            PagedListResult<CursoViewModel> pagina = new PagedListResult<CursoViewModel>();

            pagina.HasNext = cursos.HasNext;
            pagina.HasPrevious = cursos.HasPrevious;
            pagina.Count = cursos.Count;
            List<CursoViewModel> viewModels = new List<CursoViewModel>();
            foreach (var item in cursos.Entities)
            {
                viewModels.Add(FromEntity(item, 0));
            }

            pagina.Entities = viewModels;
            return pagina;
        }

        public static CursoViewModel FromEntity(Curso item, int profundidade)
        {
            return new CursoViewModel
            {

                Ativo = item.Ativo,
                DataCriacao = item.DataCriacao,
                IdAssunto = item.Assunto.Id,
                NomeAssunto = item.Assunto.Nome,
                NomeArea = item.Assunto.AreaConhecimento.Nome,
                Imagem = item.Imagem,
                Ordem = item.Ordem,
                Nome = item.Nome,
                Id = item.Id,
                IdProfessorResponsavel = item.ProfessorResponsavel.Id,
                NomeProfessorResponsavel = item.ProfessorResponsavel.Nome,
                Aulas = profundidade > 2
                ? AulaViewModel.FromEntityList(item.Aulas.Where(a => a.Ativo).OrderBy(x => x.Ordem), profundidade) 
                : new List<AulaViewModel>()
            };
        }

        internal static IEnumerable<CursoViewModel> FromEntityList(List<TurmaCurso> cursos)
        {
            foreach (var item in cursos.OrderBy(x => x.Ordem))
            {
                yield return FromEntity(item);
            }
        }

        private static CursoViewModel FromEntity(TurmaCurso item)
        {
            return new CursoViewModel
            {
                Nome = item.Curso.Nome,
                Id = item.Curso.Id,
                Ordem = item.Ordem
            };
        }

        internal static CursoViewModel FromEntity(IndiceCurso indice)
        {
            return new CursoViewModel
            {
                IdAssunto = indice.Curso.Assunto.Id,
                Imagem = indice.Curso.Imagem,
                Ordem = indice.Curso.Ordem,
                Nome = indice.Curso.Nome,
                Id = indice.Curso.Id,
                IdProfessorResponsavel = indice.Curso.ProfessorResponsavel.Id,
                NomeProfessorResponsavel = indice.Curso.ProfessorResponsavel.Nome,
                Aulas = AulaViewModel.FromEntityList(indice.AulasInfo)
            };
        }

        internal static Curso ToEntity(CursoViewModel curso, Assunto assunto, Professor professor)
        {
            return new Curso
            {
                Id = curso.Id,
                Nome = curso.Nome,
                DataCriacao = curso.DataCriacao,
                Assunto = assunto,
                Ativo = curso.Ativo,
                Ordem = curso.Ordem,
                ProfessorResponsavel = professor,
                Imagem = curso.Imagem
            };
        }

        public IEnumerable<AulaViewModel> Aulas { get; set; }

    }
}
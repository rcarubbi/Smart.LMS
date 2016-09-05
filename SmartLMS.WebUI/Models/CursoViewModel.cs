using SmartLMS.Dominio.Entidades.Conteudo;
using SmartLMS.Dominio.Repositorios;
using System;
using System.Collections.Generic;
using System.Linq;
using SmartLMS.Dominio.Entidades.Liberacao;

namespace SmartLMS.WebUI.Models
{
    public class CursoViewModel
    {
        public string NomeAssunto { get; set; }
        public Guid IdAssunto { get; set; }

        public int Ordem { get; set; }

        public string Nome { get; set; }

        public Guid Id { get; set; }

        public string Imagem { get; set; }

        public string NomeProfessorResponsavel { get; set; }

        internal static IEnumerable<CursoViewModel> FromEntityList(IEnumerable<Curso> cursos, int profundidade)
        {
            foreach (var item in cursos)
            {
                yield return FromEntity(item, profundidade);
            }
        }

        public static CursoViewModel FromEntity(Curso item, int profundidade)
        {
            return new CursoViewModel
            {
                IdAssunto = item.Assunto.Id,
                NomeAssunto = item.Assunto.Nome,
                Imagem = item.Imagem,
                Ordem = item.Ordem,
                Nome = item.Nome,
                Id = item.Id,
                NomeProfessorResponsavel = item.ProfessorResponsavel.Nome,
                Aulas = profundidade > 2
                ? AulaViewModel.FromEntityList(item.Aulas.Where(a => a.Ativo).OrderBy(x => x.Ordem), profundidade) 
                : new List<AulaViewModel>()
            };
        }

        internal static IEnumerable<TurmaViewModel> FromEntityList(List<TurmaCurso> cursos)
        {
            foreach (var item in cursos.OrderBy(x => x.Ordem))
            {
                yield return FromEntity(item);
            }
        }

        private static TurmaViewModel FromEntity(TurmaCurso item)
        {
            return new TurmaViewModel
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
                NomeProfessorResponsavel = indice.Curso.ProfessorResponsavel.Nome,
                Aulas = AulaViewModel.FromEntityList(indice.AulasInfo)
            };
        }

        public IEnumerable<AulaViewModel> Aulas { get; set; }

    }
}
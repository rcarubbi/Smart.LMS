using Carubbi.GenericRepository;
using SmartLMS.Dominio.Entidades.Liberacao;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SmartLMS.WebUI.Models
{
    public class TurmaViewModel
    {
        public string Nome { get; set; }

        [Display(Name="Cursos")]
        public List<Guid> IdsCursos { get; set; }

        public bool Ativo { get; set; }

        public Guid Id { get; set; }

        public DateTime DataCriacao { get; set; }
        public int Ordem { get; internal set; }

        internal static PagedListResult<TurmaViewModel> FromEntityList(PagedListResult<Turma> turmas)
        {
            PagedListResult<TurmaViewModel> pagina = new PagedListResult<TurmaViewModel>();

            pagina.HasNext = turmas.HasNext;
            pagina.HasPrevious = turmas.HasPrevious;
            pagina.Count = turmas.Count;
            List<TurmaViewModel> viewModels = new List<TurmaViewModel>();
            foreach (var item in turmas.Entities)
            {
                viewModels.Add(FromEntity(item));
            }

            pagina.Entities = viewModels;
            return pagina;
        }

        private static TurmaViewModel FromEntity(Turma item)
        {
            return new TurmaViewModel
            {
                DataCriacao = item.DataCriacao,
                Ativo = item.Ativo,
                Id = item.Id,
                Nome = item.Nome,
                IdsCursos = item.Cursos.Select(a => a.IdCurso).ToList()
            };
        }
    }
}
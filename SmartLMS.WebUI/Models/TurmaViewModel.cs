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
        [Required]

        public string Nome { get; set; }

        [Required(ErrorMessage= "É necessário incluir ao menos um curso na turma")]
        [Display(Name="Cursos")]
        public List<Guid> IdsCursos { get; set; }

        [Display(Name = "Alunos")]
        public List<Guid> IdsAlunos { get; set; }

        public bool Ativo { get; set; }

        public Guid Id { get; set; }

        public DateTime DataCriacao { get; set; }
  

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

        public static TurmaViewModel FromEntity(Turma item)
        {
            return new TurmaViewModel
            {
                DataCriacao = item.DataCriacao,
                Ativo = item.Ativo,
                Id = item.Id,
                Nome = item.Nome,
                IdsCursos = item.Cursos.Select(a => a.IdCurso).ToList(),
                IdsAlunos = item.Planejamentos.SelectMany(x => x.Alunos).OrderBy(a => a.Nome).Select(x => x.Id).ToList()
            };
        }

      
    }
}
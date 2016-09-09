using Carubbi.GenericRepository;
using SmartLMS.Dominio.Entidades.Conteudo;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace SmartLMS.WebUI.Models
{
    public class AssuntoViewModel
    {
        public bool Ativo { get; set; }
        public string NomeArea { get; set; }

        public DateTime DataCriacao { get; set; }

        [Display(Name = "Área de conhecimento")]
        [Required(ErrorMessage = "Selecione uma área de conhecimento")]
        public Guid IdArea { get; set; }
        public Guid Id { get; set; }

        [Required]
        public int Ordem { get; set; }

        [Required]
        public string Nome { get; set; }

        public IEnumerable<CursoViewModel> Cursos { get; set; }

        internal static IEnumerable<AssuntoViewModel> FromEntityList(IEnumerable<Assunto> assuntos, int profundidade)
        {
            foreach (var item in assuntos)
            {
                yield return FromEntity(item, profundidade);
            }
        }

        internal static PagedListResult<AssuntoViewModel> FromEntityList(PagedListResult<Assunto> assuntos)
        {
            PagedListResult<AssuntoViewModel> pagina = new PagedListResult<AssuntoViewModel>();

            pagina.HasNext = assuntos.HasNext;
            pagina.HasPrevious = assuntos.HasPrevious;
            pagina.Count = assuntos.Count;
            List<AssuntoViewModel> viewModels = new List<AssuntoViewModel>();
            foreach (var item in assuntos.Entities)
            {
                viewModels.Add(FromEntity(item, 0));
            }

            pagina.Entities = viewModels;
            return pagina;
        }


        internal static AssuntoViewModel FromEntity(Assunto item, int profundidade)
        {
            return new AssuntoViewModel
            {
                Ativo = item.Ativo,
                NomeArea = item.AreaConhecimento.Nome,
                DataCriacao = item.DataCriacao,
                Id = item.Id,
                Ordem = item.Ordem,
                Nome = item.Nome,
                Cursos = profundidade > 1
                ? CursoViewModel.FromEntityList(item.Cursos.Where(x => x.Ativo).OrderBy(x => x.Ordem), profundidade)
                : new List<CursoViewModel>(),
                IdArea = item.AreaConhecimento.Id
            };
        }

        internal static Assunto ToEntity(AssuntoViewModel assunto, AreaConhecimento area)
        {
            return new Assunto
            {
                Id = assunto.Id,
                AreaConhecimento = area,
                DataCriacao = assunto.DataCriacao,
                Ativo = assunto.Ativo,
                Nome = assunto.Nome,
                Ordem = assunto.Ordem
            };
        }
    }
}
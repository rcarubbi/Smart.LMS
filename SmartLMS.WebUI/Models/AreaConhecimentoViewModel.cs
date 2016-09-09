using SmartLMS.Dominio.Entidades;
using SmartLMS.Dominio.Entidades.Conteudo;
using SmartLMS.Dominio.Repositorios;
using System;
using System.Collections.Generic;
using System.Linq;
using Carubbi.GenericRepository;
using System.ComponentModel.DataAnnotations;

namespace SmartLMS.WebUI.Models
{
    public class AreaConhecimentoViewModel
    {
        public DateTime DataCriacao { get; set; }
        public bool Ativo { get; set; }

        [Required]
        public int Ordem { get; set; }


        public IEnumerable<AssuntoViewModel> Assuntos { get; set; }

        public Guid Id { get; set; }

        [Required]
        public string Nome { get; set; }

        internal static IEnumerable<AreaConhecimentoViewModel> FromEntityList(IEnumerable<AreaConhecimento> areas, int profundidade)
        {
            foreach (var item in areas)
            {
                yield return FromEntity(item, profundidade);
            }
        }

        public static AreaConhecimentoViewModel FromEntity(AreaConhecimento area, int profundidade )
        {
            return new AreaConhecimentoViewModel
            {
                Ativo = area.Ativo,
                Ordem = area.Ordem,
                Nome = area.Nome,
                Id = area.Id,
                DataCriacao = area.DataCriacao,
                Assuntos = profundidade > 0
                ? AssuntoViewModel.FromEntityList(area.Assuntos.Where(x =>x.Ativo).OrderBy(x => x.Ordem), profundidade)
                : new List<AssuntoViewModel>()
            };
        }

        internal static PagedListResult<AreaConhecimentoViewModel> FromEntityList(PagedListResult<AreaConhecimento> areasConhecimento)
        {
            PagedListResult<AreaConhecimentoViewModel> pagina = new PagedListResult<AreaConhecimentoViewModel>();

            pagina.HasNext = areasConhecimento.HasNext;
            pagina.HasPrevious = areasConhecimento.HasPrevious;
            pagina.Count = areasConhecimento.Count;
            List<AreaConhecimentoViewModel> viewModels = new List<AreaConhecimentoViewModel>();
            foreach (var item in areasConhecimento.Entities)
            {
                viewModels.Add(FromEntity(item, 0));
            }

            pagina.Entities = viewModels;
            return pagina;
        }

        internal static AreaConhecimento ToEntity(AreaConhecimentoViewModel areaConhecimento)
        {
            return new AreaConhecimento
            {
                Ativo = areaConhecimento.Ativo,
                DataCriacao = areaConhecimento.DataCriacao,
                Nome = areaConhecimento.Nome,
                Ordem = areaConhecimento.Ordem,
                Id = areaConhecimento.Id,
            };
        }
    }
}

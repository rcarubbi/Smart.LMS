using SmartLMS.Dominio.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartLMS.WebUI.Models
{
    public class AreaConhecimentoViewModel
    {
        public int Ordem { get; set; }

        public IEnumerable<AssuntoViewModel> Assuntos { get; set; }

        public Guid Id { get; set; }
        public string Nome { get; set; }

        internal static IEnumerable<AreaConhecimentoViewModel> FromEntityList(IEnumerable<AreaConhecimento> areas, int profundidade)
        {
            foreach (var item in areas)
            {
                yield return FromEntity(item, profundidade);
            }
        }

        internal static AreaConhecimentoViewModel FromEntity(AreaConhecimento area, int profundidade)
        {
            return new AreaConhecimentoViewModel
            {
                Ordem = area.Ordem,
                Nome = area.Nome,
                Id = area.Id,
                Assuntos = profundidade > 0
                ? AssuntoViewModel.FromEntityList(area.Assuntos.Where(x =>x.Ativo).OrderBy(x => x.Ordem), profundidade)
                : new List<AssuntoViewModel>()
            };
        }
    }
}

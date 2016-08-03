using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartLMS.Dominio.Entidades;

namespace SmartLMS.WebUI.Models
{
    public class AreaConhecimentoViewModel
    {
        public int Ordem { get; set; }

        public IEnumerable<AssuntoViewModel> Assuntos { get; set; }

        public Guid Id { get; set; }
        public string Nome { get; set; }

        internal static IEnumerable<AreaConhecimentoViewModel> FromEntityList(IEnumerable<AreaConhecimento> areas)
        {
            foreach (var item in areas)
            {
                yield return FromEntity(item);
            }
        }

        internal static AreaConhecimentoViewModel FromEntity(AreaConhecimento area)
        {
            return new AreaConhecimentoViewModel
            {
                Ordem = area.Ordem,
                Nome = area.Nome,
                Id = area.Id,
                Assuntos = AssuntoViewModel.FromEntityList(area.Assuntos.Where(x =>x.Ativo).OrderBy(x => x.Ordem))
            };
        }
    }
}

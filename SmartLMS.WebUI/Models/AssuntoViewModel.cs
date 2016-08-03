using System;
using System.Collections.Generic;
using SmartLMS.Dominio.Entidades;
using System.Linq;
namespace SmartLMS.WebUI.Models
{
    public class AssuntoViewModel
    {
        public Guid IdArea { get; set; }
        public Guid Id { get; set; }

        public int Ordem { get; set; }

        public string Nome { get; set; }

        public IEnumerable<CursoViewModel> Cursos { get; set; }

        internal static IEnumerable<AssuntoViewModel> FromEntityList(IEnumerable<Assunto> assuntos)
        {
            foreach (var item in assuntos)
            {
                yield return FromEntity(item);
            }
        }

        internal static AssuntoViewModel FromEntity(Assunto item)
        {
            return new AssuntoViewModel
            {
                Id = item.Id,
                Ordem = item.Ordem,
                Nome = item.Nome,
                Cursos = CursoViewModel.FromEntityList(item.Cursos.Where(x => x.Ativo).OrderBy(x => x.Ordem)),
                IdArea = item.AreaConhecimento.Id
            };
        }

        
    }
}
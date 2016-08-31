using SmartLMS.Dominio.Entidades.Conteudo;
using System;
using System.Collections.Generic;
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

        internal static IEnumerable<AssuntoViewModel> FromEntityList(IEnumerable<Assunto> assuntos, int profundidade)
        {
            foreach (var item in assuntos)
            {
                yield return FromEntity(item, profundidade);
            }
        }

        internal static AssuntoViewModel FromEntity(Assunto item, int profundidade)
        {
            return new AssuntoViewModel
            {
                Id = item.Id,
                Ordem = item.Ordem,
                Nome = item.Nome,
                Cursos = profundidade > 1
                ? CursoViewModel.FromEntityList(item.Cursos.Where(x => x.Ativo).OrderBy(x => x.Ordem), profundidade)
                : new List<CursoViewModel>(),
                IdArea = item.AreaConhecimento.Id
            };
        }

        
    }
}
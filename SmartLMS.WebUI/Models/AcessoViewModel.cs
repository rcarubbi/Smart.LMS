using Carubbi.GenericRepository;
using SmartLMS.Dominio.Entidades;
using SmartLMS.Dominio.Servicos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartLMS.WebUI.Models
{
    public class AcessoViewModel
    {
        public int Percentual { get; set; }

        public Guid IdConteudo { get; set; }

        public TipoAcesso TipoAcesso { get; set; }

        public string Nome { get; private set; }

        public string NomeCurso { get; private set; }

        public string DataHoraTexto { get; private set; }

        public static PagedListResult<AcessoViewModel> FromEntityList(PagedListResult<AcessoInfo> acessos)
        {
            PagedListResult<AcessoViewModel> pagina = new PagedListResult<AcessoViewModel>();

            pagina.HasNext = acessos.HasNext;
            pagina.HasPrevious = acessos.HasPrevious;
            pagina.Count = acessos.Count;
            List<AcessoViewModel> viewModels = new List<AcessoViewModel>();
            foreach (var item in acessos.Entities)
            {
                viewModels.Add(FromEntity(item));
            }

            pagina.Entities = viewModels;
            return pagina;
        }

        private static AcessoViewModel FromEntity(AcessoInfo item)
        {
            var acesso = new AcessoViewModel
            {
                TipoAcesso = item.Tipo,
                DataHoraTexto = item.DataHoraTexto
            };

            if (item.Tipo == TipoAcesso.Arquivo)
            {
                acesso.IdConteudo = item.AcessoArquivo.Arquivo.Id;
                acesso.Nome = item.AcessoArquivo.Arquivo.Nome;
                acesso.NomeCurso= item.AcessoArquivo.Arquivo.Curso.Nome;
                acesso.Percentual = 100;
            }
            else
            {
                acesso.IdConteudo = item.AcessoAula.Aula.Id;
                acesso.Nome = item.AcessoAula.Aula.Nome;
                acesso.NomeCurso = item.AcessoAula.Aula.Curso.Nome;
                acesso.Percentual = item.AcessoAula.Percentual;
            }
            return acesso;
        }
    }
}

using SmartLMS.Dominio.Entidades;
using SmartLMS.Dominio.Repositorios;
using System;
using System.Data.Entity.Core.Objects;
using System.Linq;
namespace SmartLMS.Dominio.Servicos
{
    public class ResultadoBusca
    {
        public string Icone
        {
            get
            {
                switch (Tipo)
                {
                    case TipoResultado.AreaConhecimento:
                        return "fa-map-signs";
                    case TipoResultado.Assunto:
                        return "fa-map";
                    case TipoResultado.Curso:
                        return "fa-graduation-cap";
                    case TipoResultado.Aula:
                        return "fa-laptop";
                    case TipoResultado.Arquivo:
                        return "fa-file-text-o";
                    default:
                        return "fa-file-o";
                }
            }
        }
        public string Link
        {
            get
            {
                switch (Tipo)
                {
                    case TipoResultado.AreaConhecimento:
                        return string.Format("Assunto/Index/{0}", Id);
                    case TipoResultado.Assunto:
                        return string.Format("Curso/Index/{0}", Id);
                    case TipoResultado.Curso:
                        return string.Format("Aula/Index/{0}", Id);
                    case TipoResultado.Aula:
                        return string.Format("Aula/Ver/{0}", Id);
                    case TipoResultado.Arquivo:
                        return string.Format("Arquivo/Download/{0}", Id);
                    default:
                        return string.Empty;
                }
            }
        }

        public string TipoDescricao
        {
            get
            {
                switch (Tipo)
                {
                    case TipoResultado.AreaConhecimento:
                        return Parametro.AREA_CONHECIMENTO;
                    case TipoResultado.Assunto:
                        return Parametro.ASSUNTO;
                    case TipoResultado.Curso:
                        return Parametro.CURSO;
                    case TipoResultado.Aula:
                        return Parametro.AULA;
                    case TipoResultado.Arquivo:
                        return Parametro.ARQUIVO;
                    default:
                        return string.Empty;
                }
            }
        }
        public Guid Id { get; set; }

        public int Percentual { get; set; }

        public TipoResultado Tipo { get; set; }

        public string Descricao { get; set; }

     

        public static ResultadoBusca Parse<T>(T item, Usuario _usuarioLogado, IContexto contexto)
            where T: IResultadoBusca
        {
            var resultado = new ResultadoBusca
            {
                Id = item.Id,
                Descricao = item.Nome,
                Tipo = (TipoResultado)(Enum.Parse(typeof(TipoResultado), ObjectContext.GetObjectType(item.GetType()).Name))
            };

            if (resultado.Tipo == TipoResultado.Arquivo && contexto.ObterLista<AcessoArquivo>().Any(x => x.Arquivo.Id == resultado.Id))
            {
                resultado.Percentual = 100;
            }
 
            if (resultado.Tipo == TipoResultado.Aula)
            {
                var acessoRepo = new RepositorioAcessoAula(contexto, _usuarioLogado.Id);
                var acessoMaisLongo = acessoRepo.ObterMaiorPercentual(resultado.Id);
                if (acessoMaisLongo != null)
                {
                    resultado.Percentual = acessoMaisLongo.Percentual;
                }
            }
 
            return resultado;
        }

     
    }
}
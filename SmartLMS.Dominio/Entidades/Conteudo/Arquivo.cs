using SmartLMS.Dominio.Entidades.Historico;
using System.Collections.Generic;

namespace SmartLMS.Dominio.Entidades.Conteudo
{
    public class Arquivo : Entidade, IResultadoBusca
    {
        public string Nome { get; set; }

        public string ArquivoFisico { get; set; }

        public virtual ICollection<AcessoArquivo> Acessos { get; set; }

        public virtual Curso Curso { get; set; }

        public virtual Aula Aula { get; set; }

       

    }
}

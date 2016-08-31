using SmartLMS.Dominio.Entidades.Conteudo;
using SmartLMS.Dominio.Entidades.Pessoa;
using System;

namespace SmartLMS.Dominio.Entidades.Historico
{
    public class AcessoArquivo
    {
        public long Id { get; set; }

        public virtual Usuario Usuario { get; set; }

        public virtual Arquivo Arquivo { get; set; }

        public DateTime DataHoraAcesso { get; set; }


    }
}

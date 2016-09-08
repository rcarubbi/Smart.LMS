using SmartLMS.Dominio.Entidades.Historico;
using System;

namespace SmartLMS.Dominio.Servicos
{
    public class AcessoInfo
    {
        public string DataHoraTexto { get; set; }

        public TipoAcesso Tipo { get; set; }

        public AcessoAula AcessoAula { get; set; }

        public AcessoArquivo AcessoArquivo { get; set; }

        public DateTime DataHoraAcesso { get; set; }

    }
}

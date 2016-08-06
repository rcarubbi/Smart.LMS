using System;

namespace SmartLMS.Dominio.Entidades
{
    public class AcessoArquivo
    {
        public long Id { get; set; }

        public virtual Usuario Usuario { get; set; }

        public virtual Arquivo Arquivo { get; set; }

        public DateTime DataHoraAcesso{ get; set; }


    }
}

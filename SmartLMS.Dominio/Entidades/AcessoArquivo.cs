using System;

namespace SmartLMS.Dominio.Entidades
{
    public class AcessoArquivo
    {
        public long Id { get; set; }

        public virtual Aluno Aluno { get; set; }

        public virtual Arquivo Arquivo { get; set; }

        public DateTime DataHoraAcesso{ get; set; }


    }
}

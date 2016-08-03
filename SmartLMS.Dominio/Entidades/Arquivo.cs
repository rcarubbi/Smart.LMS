using System;

namespace SmartLMS.Dominio.Entidades
{
    public class Arquivo : Entidade, IResultadoBusca
    {
        public string Nome { get; set; }

        public string ArquivoFisico { get; set; }

     
    }
}

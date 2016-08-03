using System;
using System.Collections.Generic;

namespace SmartLMS.Dominio.Entidades
{
    public class AreaConhecimento : Entidade, IResultadoBusca
    {
        public int Ordem { get; set; }
        public string Nome { get; set; }

        public virtual ICollection<Assunto> Assuntos { get; set; }

        
    }
}

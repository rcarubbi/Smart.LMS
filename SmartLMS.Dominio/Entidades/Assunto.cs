using System;
using System.Collections.Generic;

namespace SmartLMS.Dominio.Entidades
{
    public class Assunto : Entidade, IResultadoBusca
    {
        public int Ordem { get; set; }
        public virtual AreaConhecimento AreaConhecimento { get; set; }
        public virtual ICollection<Curso> Cursos { get; set; }

        public string Nome { get; set; }

      
    }
}

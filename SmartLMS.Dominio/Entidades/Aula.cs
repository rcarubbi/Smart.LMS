using System;
using System.Collections.Generic;

namespace SmartLMS.Dominio.Entidades
{
    public class Aula : Entidade, IResultadoBusca
    {
        public virtual Professor Professor { get; set; }

        public string Nome { get; set; }

        public virtual ICollection<Arquivo> Arquivos { get; set; }

        public string Conteudo { get; set; }

        public TipoConteudo Tipo { get; set; }
        public virtual Curso Curso { get; set; }

        public int Ordem { get; set; }

    
    }
}

using System;
using System.Collections.Generic;

namespace SmartLMS.Dominio.Entidades
{
    public class Aula : Entidade, IResultadoBusca
    {

        public virtual ICollection<AcessoAula> Acessos { get; set; }

        public virtual ICollection<Turma> Turmas { get; set; }

        public virtual Professor Professor { get; set; }

        public string Nome { get; set; }

        public virtual ICollection<Arquivo> Arquivos { get; set; }

        public string Conteudo { get; set; }

        public TipoConteudo Tipo { get; set; }
        public virtual Curso Curso { get; set; }

        public int Ordem { get; set; }

        public DateTime DataInclusao { get; set; }
    }
}

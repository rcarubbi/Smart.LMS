using SmartLMS.Dominio.Servicos;
using System;
using System.Collections.Generic;

namespace SmartLMS.Dominio.Entidades
{
    public class Aluno : Usuario
    {
        public virtual ICollection<AcessoAula> AcessosAula { get; set; }

        public virtual ICollection<AcessoArquivo> AcessosArquivo { get; set; }

        public virtual ICollection<TurmaAluno> Turmas { get; set; }

      

      
    }
}

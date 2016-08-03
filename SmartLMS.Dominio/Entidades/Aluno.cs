using System.Collections.Generic;

namespace SmartLMS.Dominio.Entidades
{
    public class Aluno : Usuario
    {


        public virtual ICollection<TurmaAluno> Turmas { get; set; }

     
    }
}

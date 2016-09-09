using SmartLMS.Dominio.Entidades.Historico;
using SmartLMS.Dominio.Entidades.Liberacao;
using System.Collections.Generic;

namespace SmartLMS.Dominio.Entidades.Pessoa
{
    public class Aluno : Usuario
    {
    
        public virtual ICollection<Planejamento> Planejamentos { get; set; }
 
    }
}

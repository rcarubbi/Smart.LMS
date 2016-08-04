using System.Linq;
using SmartLMS.Dominio.Entidades;
using System.Collections.Generic;

namespace SmartLMS.Dominio.Repositorios
{
    public class IndiceCurso
    {

        public IEnumerable<AulaInfo> AulasInfo { get; set; }
        public Curso Curso { get; internal set; }
    }
}
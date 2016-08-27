using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace SmartLMS.Domain.Servicos
{
    public enum TipoAviso : int
    {
        [Description("Todos")]
        Todos,
        [Description("Geral")]
        Geral,
        [Description("Turma")]
        Turma,
        [Description("Pessoal")]
        Pessoal
    }
}
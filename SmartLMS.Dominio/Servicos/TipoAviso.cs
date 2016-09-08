using System.ComponentModel;

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
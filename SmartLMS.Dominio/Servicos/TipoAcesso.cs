using System.ComponentModel;

namespace SmartLMS.Dominio.Servicos
{
    public enum TipoAcesso
    {
        [Description("Todos")]
        Todos,
        [Description("Aulas")]
        Aula,
        [Description("Materiais de apoio")]
        Arquivo
            
    }
}
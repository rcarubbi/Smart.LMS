using SmartLMS.Dominio.Entidades.Conteudo;

namespace SmartLMS.Dominio.Repositorios
{
    public class AulaInfo
    {
        public Aula Aula { get; set; }

        public bool Disponivel { get; set; }
        public int Percentual { get; set; }
        public decimal Segundos { get; set; }
    }
}
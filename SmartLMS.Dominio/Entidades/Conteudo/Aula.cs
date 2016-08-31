using SmartLMS.Dominio.Entidades.Comunicacao;
using SmartLMS.Dominio.Entidades.Historico;
using SmartLMS.Dominio.Entidades.Liberacao;
using SmartLMS.Dominio.Entidades.Pessoa;
using System.Collections.Generic;
namespace SmartLMS.Dominio.Entidades.Conteudo
{
    public class Aula : Entidade, IResultadoBusca
    {
        public int DiasLiberacao { get; set; }

        public virtual ICollection<Comentario> Comentarios { get; set; }
        public virtual ICollection<AcessoAula> Acessos { get; set; }

        public virtual ICollection<AulaPlanejamento> PlanejamentosLiberados { get; set; }

        public virtual Professor Professor { get; set; }

        public string Nome { get; set; }

        public virtual ICollection<Arquivo> Arquivos { get; set; }

        public string Conteudo { get; set; }

        public TipoConteudo Tipo { get; set; }
        public virtual Curso Curso { get; set; }

        public int Ordem { get; set; }

    

       
    }
}

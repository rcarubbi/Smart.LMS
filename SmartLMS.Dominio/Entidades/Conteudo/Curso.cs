using SmartLMS.Dominio.Entidades.Liberacao;
using SmartLMS.Dominio.Entidades.Pessoa;
using System.Collections.Generic;

namespace SmartLMS.Dominio.Entidades.Conteudo
{
    public class Curso : Entidade, IResultadoBusca
    {
        public int Ordem { get; set; }
        public virtual Professor ProfessorResponsavel { get; set; }

        public string Imagem { get; set; }

        public string Nome { get; set; }

        public virtual ICollection<Aula> Aulas { get; set; }
        public virtual Assunto Assunto { get; set; }
        public virtual ICollection<Arquivo> Arquivos { get; set; }

        public virtual ICollection<TurmaCurso> Turmas { get; set; }
      
    }
}

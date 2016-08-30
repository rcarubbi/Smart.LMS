using System;
using System.Collections.Generic;
using System.Linq;
namespace SmartLMS.Dominio.Entidades
{
    public class Aula : Entidade, IResultadoBusca
    {
        public virtual ICollection<Comentario> Comentarios { get; set; }
        public virtual ICollection<AcessoAula> Acessos { get; set; }

        public virtual ICollection<AulaTurma> Turmas { get; set; }

        public virtual Professor Professor { get; set; }

        public string Nome { get; set; }

        public virtual ICollection<Arquivo> Arquivos { get; set; }

        public string Conteudo { get; set; }

        public TipoConteudo Tipo { get; set; }
        public virtual Curso Curso { get; set; }

        public int Ordem { get; set; }

        public DateTime DataInclusao { get; set; }

        internal bool VerificarDisponibilidade(Guid idUsuario)
        {
            return Turmas.Any(t => t.Turma.Alunos.Any(al => al.IdAluno == idUsuario)) ||
                Professor.Id == idUsuario ||
                Curso.ProfessorResponsavel.Id == idUsuario;
        }
    }
}

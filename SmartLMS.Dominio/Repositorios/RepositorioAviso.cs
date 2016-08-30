using SmartLMS.Dominio.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartLMS.Dominio.Repositorios
{
    public class RepositorioAviso
    {
        private IContexto _contexto;
        public RepositorioAviso(IContexto contexto)
        {
            _contexto = contexto;
        }


        public IEnumerable<Aviso> ListarAvisosNaoVistos(Guid idUsuario)
        {
            var repo = new RepositorioTurma(_contexto);
            var repoUsuario = new RepositorioUsuario(_contexto);
            var turmas = repo.ListarTurmasPorAluno(idUsuario);
            var usuario = repoUsuario.ObterPorId(idUsuario);

            return _contexto.ObterLista<Aviso>().Where(a =>
                    a.DataHora > usuario.DataCriacao &&
                    ((a.Turma != null && turmas.Any(t => t.Id == a.Turma.Id))
                    || (a.Usuario != null && a.Usuario.Id == idUsuario)
                    || (a.Turma == null && a.Usuario == null))
                    && (!a.Usuarios.Any(u => u.DataVisualizacao.HasValue && u.IdUsuario == idUsuario)))
                    .OrderByDescending( x=> x.DataHora)
                    .Take(2).ToList();
        }

    }
}

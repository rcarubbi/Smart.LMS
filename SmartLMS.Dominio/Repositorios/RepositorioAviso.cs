using SmartLMS.Dominio.Entidades.Comunicacao;
using System;
using System.Collections.Generic;
using System.Linq;

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

            var repoUsuario = new RepositorioUsuario(_contexto);
            var usuario = repoUsuario.ObterPorId(idUsuario);
            var turmas = repoUsuario.ListarTurmas(idUsuario).Select(x => x.Id).ToList();
   

            return _contexto.ObterLista<Aviso>().Where(a =>
                    a.DataHora >= usuario.DataCriacao &&
                    ((a.Turma != null && turmas.Contains(a.Turma.Id))
                    || (a.Usuario != null && a.Usuario.Id == idUsuario)
                    || (a.Turma == null && a.Usuario == null))
                    && (!a.Usuarios.Any(u => u.IdUsuario == idUsuario)))
                    .OrderByDescending( x=> x.DataHora)
                    .Take(2).ToList();
        }

    }
}

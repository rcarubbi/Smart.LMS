using SmartLMS.Dominio.Entidades.Comunicacao;
using SmartLMS.Dominio.Entidades.Pessoa;
using System;
using System.Collections.Generic;
using System.Data.Entity;
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
            var planejamentos = new List<long>();

            if (usuario is Aluno)
            {
                planejamentos = (usuario as Aluno).Planejamentos.Select(x => x.Id).ToList();
            }
   

            return _contexto.ObterLista<Aviso>().Where(a =>
                    DbFunctions.TruncateTime(a.DataHora) >= DbFunctions.TruncateTime(usuario.DataCriacao) &&
                    ((a.Planejamento != null && planejamentos.Contains(a.Planejamento.Id))
                    || (a.Usuario != null && a.Usuario.Id == idUsuario)
                    || (a.Planejamento == null && a.Usuario == null))
                    && (!a.Usuarios.Any(u => u.IdUsuario == idUsuario)))
                    .OrderByDescending( x=> x.DataHora)
                    .Take(2).ToList();
        }

    }
}

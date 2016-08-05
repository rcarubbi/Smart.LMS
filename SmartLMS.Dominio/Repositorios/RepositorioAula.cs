using SmartLMS.Dominio;
using SmartLMS.Dominio.Entidades;
using SmartLMS.Dominio.Repositorios;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartLMS.WebUI.Controllers
{
    public class RepositorioAula
    {
        private IContexto _contexto;

        public RepositorioAula(IContexto contexto)
        {
            _contexto = contexto;
        }

        public IEnumerable<Aula> ListarUltimasAulasAdicionadas(Guid idAluno)
        {
            return _contexto.ObterLista<TurmaAluno>().Where(t => t.IdAluno == idAluno)
                .SelectMany(x => x.Turma.AulasDisponiveis)
                .OrderByDescending(x => x.DataInclusao).Take(6);
        }

        public AulaInfo ObterAula(Guid id, Guid idUsuario)
        {
            var aula = _contexto.ObterLista<Aula>().Find(id);
            var ultimoAcesso = aula.Acessos.Where(x => x.Usuario.Id == idUsuario).LastOrDefault();

            return new AulaInfo
            {
                Aula = _contexto.ObterLista<Aula>().Find(id),
                Disponivel = aula.Turmas.Any(t => t.Alunos.Any(al => al.IdAluno == idUsuario)),
                Percentual = ultimoAcesso == null ? 0 : ultimoAcesso.Percentual,
                Segundos = ultimoAcesso == null ? 0 : ultimoAcesso.Segundos,
            };
        }
    }
}
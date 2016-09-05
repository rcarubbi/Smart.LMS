using SmartLMS.Dominio.Entidades.Conteudo;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartLMS.Dominio.Repositorios
{
    public class RepositorioCurso
    {

        private IContexto _contexto;
        public RepositorioCurso(IContexto contexto)
        {
            _contexto = contexto;
        }

        public List<Curso> ListarAtivos()
        {
            return _contexto.ObterLista<Curso>().Where(a => a.Ativo).OrderBy(a => a.Nome).ToList();
        }

        public IndiceCurso ObterIndiceCurso(Guid id, Guid idUsuario)
        {
            var indiceCurso = new IndiceCurso();
            RepositorioAula repoAula = new RepositorioAula(_contexto);
            

            indiceCurso.Curso = _contexto.ObterLista<Curso>().Find(id);
            indiceCurso.AulasInfo = indiceCurso.Curso.Aulas.Where(a => a.Ativo = true)
                .OrderBy(x => x.Ordem)
                .Select(a => new AulaInfo {
                    Aula = a,
                    Disponivel = repoAula.VerificarDisponibilidadeAula(a.Id, idUsuario),
                    Percentual = a.Acessos.Where(x => x.Usuario.Id == idUsuario).LastOrDefault()?.Percentual ?? 0
        });

            return indiceCurso;
            
        }
    }
}

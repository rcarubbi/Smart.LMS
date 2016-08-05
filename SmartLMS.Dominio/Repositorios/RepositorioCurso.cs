using System;
using System.Collections.Generic;
using System.Linq;
using SmartLMS.Dominio.Entidades;

namespace SmartLMS.Dominio.Repositorios
{
    public class RepositorioCurso
    {

        private IContexto _contexto;
        public RepositorioCurso(IContexto contexto)
        {
            _contexto = contexto;
        }

        public IndiceCurso ObterIndiceCurso(Guid id, Guid idUsuario)
        {
            var indiceCurso = new IndiceCurso();
            

            indiceCurso.Curso = _contexto.ObterLista<Curso>().Find(id);
            indiceCurso.AulasInfo = indiceCurso.Curso.Aulas.Where(a => a.Ativo = true)
                .OrderBy(x => x.Ordem)
                .Select(a => new AulaInfo {
                    Aula = a,
                    Disponivel = a.Turmas.Any(t => t.Alunos.Any(al => al.IdAluno == idUsuario)),
                    Percentual = a.Acessos.Where(x => x.Usuario.Id == idUsuario).LastOrDefault()?.Percentual ?? 0
        });

            return indiceCurso;
            
        }
    }
}

using SmartLMS.Dominio;
using SmartLMS.Dominio.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartLMS.Dominio.Repositorios
{
    public class RepositorioAula
    {
        private IContexto _contexto;

        public RepositorioAula(IContexto contexto)
        {
            _contexto = contexto;
        }

        public IEnumerable<AulaTurma> ListarUltimasAulasAdicionadas(Guid idAluno)
        {
            var turmasAluno = _contexto
                .ObterLista<TurmaAluno>()
                .Where(t => t.IdAluno == idAluno)
                .Select(x => x.Turma);

            return turmasAluno.SelectMany(a => a.AulasDisponiveis)
                .OrderByDescending(x => x.DataDisponibilizacao)
                .Take(6).ToList();
        }

        public AulaInfo ObterAula(Guid id, Guid idUsuario)
        {
            var aula = _contexto.ObterLista<Aula>().Find(id);
            var ultimoAcesso = aula.Acessos.Where(x => x.Usuario.Id == idUsuario).LastOrDefault();

            return new AulaInfo
            {
                Aula = _contexto.ObterLista<Aula>().Find(id),
                Disponivel = aula.VerificarDisponibilidade(idUsuario),
                Percentual = ultimoAcesso == null ? 0 : ultimoAcesso.Percentual,
                Segundos = ultimoAcesso == null ? 0 : ultimoAcesso.Segundos,
            };
        }

        public void Comentar(Comentario comentario)
        {
            _contexto.ObterLista<Comentario>().Add(comentario);
            _contexto.Salvar();
        }

        public void ExcluirComentario(long idComentario)
        {
            var comentario = _contexto.ObterLista<Comentario>().Find(idComentario);
            _contexto.ObterLista<Comentario>().Remove(comentario);
            _contexto.Salvar();
        }

        public Arquivo ObterArquivo(Guid id)
        {
            return _contexto.ObterLista<Arquivo>().Find(id);
        }

      

        public void GravarAcesso(Arquivo arquivo, Usuario usuario)
        {
            
            AcessoArquivo acesso = new AcessoArquivo {
                Arquivo = arquivo,
                Usuario = usuario,
                DataHoraAcesso = DateTime.Now
            };

            _contexto.ObterLista<AcessoArquivo>().Add(acesso);
            _contexto.Salvar();
        }
    }
}
using SmartLMS.Dominio.Entidades.Comunicacao;
using SmartLMS.Dominio.Entidades.Conteudo;
using SmartLMS.Dominio.Entidades.Historico;
using SmartLMS.Dominio.Entidades.Liberacao;
using SmartLMS.Dominio.Entidades.Pessoa;
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

        public IEnumerable<AulaPlanejamento> ListarUltimasAulasLiberadas(Guid idAluno)
        {
            
            RepositorioUsuario usuRepo = new RepositorioUsuario(_contexto);
            var usuario = usuRepo.ObterPorId(idAluno);

            if (usuario is Aluno)
            {
                return (usuario as Aluno).Planejamentos
                    .SelectMany(x => x.AulasDisponiveis)
                    .OrderByDescending(x => x.DataLiberacao)
                    .Take(6)
                    .ToList();
            }
            else
                return new List<AulaPlanejamento>();
        }


        public bool VerificarDisponibilidadeAula(Guid idAula, Guid idUsuario)
        {
            RepositorioUsuario usuRepo = new RepositorioUsuario(_contexto);
            var usuario = usuRepo.ObterPorId(idUsuario);

            if (usuario is Aluno)
            {
                return ((Aluno)usuario).Planejamentos
                    .SelectMany(x => x.AulasDisponiveis)
                    .Any(x => x.Aula.Id == idAula);
            }
            else if (usuario is Professor)
            {
                var aula = _contexto.ObterLista<Aula>().Find(idAula);
                return aula.Professor.Id == idUsuario || aula.Curso.ProfessorResponsavel.Id == idUsuario;
            }
            else {
                return true;
            }
        }


        public AulaInfo ObterAula(Guid id, Guid idUsuario)
        {
            var aula = _contexto.ObterLista<Aula>().Find(id);
            var ultimoAcesso = aula.Acessos.Where(x => x.Usuario.Id == idUsuario).LastOrDefault();
            
            return new AulaInfo
            {
                Aula = _contexto.ObterLista<Aula>().Find(id),
                Disponivel = VerificarDisponibilidadeAula(id, idUsuario),
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

        internal void Excluir(Guid id)
        {
            var aula = _contexto.ObterLista<Aula>().Find(id);
             _contexto.ObterLista<Aula>().Remove(aula);
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
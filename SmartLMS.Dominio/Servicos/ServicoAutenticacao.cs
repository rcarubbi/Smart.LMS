using Carubbi.Utils.Security;
using SmartLMS.Dominio.Entidades;
using SmartLMS.Dominio.Repositorios;
using System;
using System.Linq;

namespace SmartLMS.Dominio.Servicos
{
    public class ServicoAutenticacao
    {
        private IContexto _contexto;
        public ServicoAutenticacao(IContexto contexo)
        {
            _contexto = contexo;

            _criptografia = new CriptografiaSimetrica(SymmetricCryptProvider.TripleDES);
            RepositorioParametro parametroRepo = new RepositorioParametro(_contexto);
            _criptografia.Key = parametroRepo.ObterValorPorChave(Parametro.CHAVE_CRIPTOGRAFIA);
        }

        private CriptografiaSimetrica _criptografia;

        public bool Login(string login, string senha)
        {
            var senhaCriptografada = _criptografia.Encrypt(senha);
            return _contexto.ObterLista<Usuario>().Any(u => u.Login == login && u.Senha == senhaCriptografada && u.Ativo);
        }

        public void AlterarUsuario(Guid id, string nome, string email, string login, string senha, bool ativo)
        {
            RepositorioUsuario usuarioRepo = new RepositorioUsuario(_contexto);
            var entidade = usuarioRepo.ObterPorId(id);
            var usuarioPorConta = usuarioRepo.ObterPorLogin(login);

            if (usuarioPorConta != null && usuarioPorConta.Id != id)
            {
                throw new ApplicationException("Já existe outro usuário com esta conta");
            }

            var usuarioAlterado = new Administrador
            {
                Id = id,
                Nome = nome,
                Ativo = ativo,
                Senha = _criptografia.Encrypt(senha),
                Login = login,
                Email = email,
            };

            _contexto.Atualizar(entidade, usuarioAlterado);
            _contexto.Salvar();
        }

        public Usuario CriarUsuario(string nome, string login, string email, string senha, Perfil perfil)
        {
            RepositorioUsuario usuarioRepo = new RepositorioUsuario(_contexto);

            if (usuarioRepo.ObterPorLogin(login) != null)
                throw new ApplicationException("Já existe um usuário com esta conta");

            var senhaCriptografada = _criptografia.Encrypt(senha);
            Usuario usuario = null;

            switch (perfil)
            {
                case Perfil.Administrador:
                    usuario = new Administrador();
                    break;
                case Perfil.Professor:
                    usuario = new Professor();
                    break;
                case Perfil.Aluno:
                    usuario = new Aluno();
                    break;
            }

            usuario.Login = login;
            usuario.Senha = senhaCriptografada;
            usuario.Ativo = true;
            usuario.Email = email;
            usuario.Nome = nome;

            usuarioRepo.Salvar(usuario);

            NotificarUsuario(usuario, senha);
            return usuario;
        }

        private void NotificarUsuario(Usuario usuario, string senha)
        {
             
        }

        public string RecuperarSenha(string email)
        {
            RepositorioUsuario repo = new RepositorioUsuario(_contexto);
            Usuario usuario = repo.ObterPorEmail(email);
            if (usuario == null)
                return null;
            else
                return _criptografia.Decrypt(usuario.Senha);

        }
    }
}

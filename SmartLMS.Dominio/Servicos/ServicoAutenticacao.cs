using Carubbi.DiffAnalyzer;
using Carubbi.Utils.Security;
using SmartLMS.Dominio.Entidades;
using SmartLMS.Dominio.Entidades.Comunicacao;
using SmartLMS.Dominio.Entidades.Pessoa;
using SmartLMS.Dominio.Repositorios;
using System;
using System.Linq;
using System.Text;

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

        public void AlterarUsuario(Guid id, string nome, string email, string login, string senha, bool ativo, Perfil perfil)
        {
            RepositorioUsuario usuarioRepo = new RepositorioUsuario(_contexto);
            var entidade = usuarioRepo.ObterPorId(id);
            var usuarioPorConta = usuarioRepo.ObterPorLogin(login);

            if (usuarioPorConta != null && usuarioPorConta.Id != id)
            {
                throw new ApplicationException("Já existe outro usuário com esta conta");
            }

            Usuario usuarioAlterado = null;

            switch (perfil)
            {
                case Perfil.Administrador:
                    usuarioAlterado = new Administrador();
                    break;
                case Perfil.Professor:
                    usuarioAlterado = new Professor();
                    break;
                case Perfil.Aluno:
                    usuarioAlterado = new Aluno();
                    break;
             
            }


            usuarioAlterado.Id = id;
            usuarioAlterado.Nome = nome;
            usuarioAlterado.Ativo = ativo;
            if (senha != entidade.Senha)
            {
                usuarioAlterado.Senha = _criptografia.Encrypt(senha);
            }
            else
            {
                usuarioAlterado.Senha = senha;
            }

            usuarioAlterado.Login = login;
            usuarioAlterado.Email = email;
            usuarioAlterado.DataCriacao = entidade.DataCriacao;
            

            DiffAnalyzer analyzer = new DiffAnalyzer(1);
            var diferencas = analyzer.Compare(_contexto.UnProxy(entidade), usuarioAlterado, a => a.State == DiffState.Modified);

            
            StringBuilder textoDiferencas = new StringBuilder($"Alteração de dados Cadastrais:{Environment.NewLine}<br />");
            foreach (var item in diferencas)
            {
                if (item.PropertyName == "Senha")
                {
                    textoDiferencas.AppendLine("- Sua senha foi alterada <br />");
                }
                else
                {
                    textoDiferencas.AppendLine($"- {item.PropertyName} de {item.OldValue} para {item.NewValue}<br />");
                }
            }

            Aviso avisoAlteracao = new Aviso {
                Texto = textoDiferencas.ToString(),
                DataHora = DateTime.Now,
                Usuario = entidade,
            };

            _contexto.ObterLista<Aviso>().Add(avisoAlteracao);
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
            usuario.DataCriacao = DateTime.Now;


            var aviso = new Aviso
            {
                Usuario = usuario,
                Texto = $"Bem vindo ao {Parametro.PROJETO}! Bons estudos!",
                DataHora = DateTime.Now,
            };
            _contexto.ObterLista<Aviso>().Add(aviso);
            usuarioRepo.Salvar(usuario);

 
            NotificarUsuario(usuario, senha);
            return usuario;
        }

        private void NotificarUsuario(Usuario usuario, string senha)
        {
             // TODO: Implementar
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

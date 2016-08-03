namespace SmartLMS.DAL.Migrations
{
    using Carubbi.Utils.Security;
    using Dominio.Entidades;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<SmartLMS.DAL.Contexto>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(SmartLMS.DAL.Contexto context)
        {
            var criptografia = new CriptografiaSimetrica(SymmetricCryptProvider.TripleDES);
            criptografia.Key = "IT_Newest_49387_In";

            #region Usuario
            context.Set<Usuario>().AddOrUpdate(u => u.Login,
                new Administrador
                {
                    Nome = "Administrador",
                    Ativo = true,
                    Login = "administrador@itanio.com.br",
                    Email = "raphael@itanio.com.br",
                    Senha = criptografia.Encrypt("Administrador")
                });

            context.Set<Usuario>().AddOrUpdate(u => u.Login,
             new Aluno
             {
                 Nome = "Aluno",
                 Ativo = true,
                 Login = "aluno@itanio.com.br",
                 Email = "raphael@itanio.com.br",
                 Senha = criptografia.Encrypt("Aluno")
             });

            context.Set<Usuario>().AddOrUpdate(u => u.Login,
             new Professor
             {
                 Nome = "Professor",
                 Ativo = true,
                 Login = "professor@itanio.com.br",
                 Email = "raphael@itanio.com.br",
                 Senha = criptografia.Encrypt("Professor")
             });


            context.Salvar();
            #endregion

            #region Parametro
            context.Set<Parametro>()
                .AddOrUpdate(p => p.Chave,
                    new Parametro
                    {
                        Chave = Parametro.CHAVE_CRIPTOGRAFIA,
                        Valor = "IT_Newest_49387_In",
                        Ativo = true
                    },
                    new Parametro
                    {
                        Chave = Parametro.REMETENTE_EMAIL,
                        Valor = "raphael@itanio.com.br",
                        Ativo = true
                    },
                    new Parametro
                    {
                        Chave = Parametro.SMTP_PORTA,
                        Valor = "587",
                        Ativo = true
                    },
                    new Parametro
                    {
                        Chave = Parametro.SMTP_SENHA,
                        Valor = "IT_Newest_0773_In",
                        Ativo = true
                    },
                    new Parametro
                    {
                        Chave = Parametro.SMTP_SERVIDOR,
                        Valor = "mail.exchange.locaweb.com.br",
                        Ativo = true
                    },
                    new Parametro
                    {
                        Chave = Parametro.SMTP_USA_SSL,
                        Valor = "true",
                        Ativo = true
                    },
                    new Parametro
                    {
                        Chave = Parametro.SMTP_USUARIO,
                        Valor = "raphael@itanio.com.br",
                        Ativo = true
                    },
                    new Parametro
                    {
                        Chave = Parametro.NOME_PROJETO,
                        Valor = "Código Nerd",
                        Ativo = true
                    },
                    new Parametro
                    {
                        Chave = Parametro.SMTP_USAR_CREDENCIAIS_PADRAO,
                        Valor = "true",
                        Ativo = true
                    },
                    new Parametro
                    {
                        Chave = Parametro.CHAVE_AREA_CONHECIMENTO_PLURAL,
                        Valor = "Direções",
                        Ativo = true
                    },
                    new Parametro
                    {
                        Chave = Parametro.CHAVE_AREA_CONHECIMENTO,
                        Valor = "Direção",
                        Ativo = true
                    },
                    new Parametro
                    {
                        Chave = Parametro.CHAVE_ASSUNTO,
                        Valor = "Roteiro",
                        Ativo = true
                    },
                    new Parametro
                    {
                        Chave = Parametro.CHAVE_CURSO,
                        Valor = "Trilha",
                        Ativo = true
                    },
                    new Parametro
                    {
                        Chave = Parametro.CHAVE_AULA,
                        Valor = "Trecho",
                        Ativo = true
                    },
                    new Parametro
                    {
                        Chave = Parametro.CHAVE_ARQUIVO,
                        Valor = "Material de apoio",
                        Ativo = true
                    },
                    new Parametro
                    {
                        Chave = Parametro.CHAVE_TITULO_AULAS_ASSISTIDAS,
                        Valor = "Últimos trechos assistidos",
                        Ativo = true
                    },
                    new Parametro
                    {
                        Chave = Parametro.CHAVE_TITULO_ULTIMOS_CURSOS,
                        Valor = "Novas trilhas",
                        Ativo = true
                    },
                    new Parametro
                    {
                        Chave = Parametro.CHAVE_TITULO_ULTIMAS_AULAS,
                        Valor = "Novos trechos",
                        Ativo = true
                    });



            context.Salvar();

            #endregion

        }
    }
}

namespace SmartLMS.DAL.Migrations
{
    using Carubbi.Utils.Security;
    using Dominio.Entidades;
    using Dominio.Entidades.Conteudo;
    using Dominio.Entidades.Liberacao;
    using Dominio.Entidades.Pessoa;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Migrations;

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
            context.Set<Usuario>().AddOrUpdate(u => u.Nome,
                new Administrador
                {
                    Nome = "Administrador",
                    Ativo = true,
                    Login = "administrador@itanio.com.br",
                    Email = "raphael@itanio.com.br",
                    Senha = criptografia.Encrypt("Administrador"),
                    DataCriacao = DateTime.Now
                });
 
      
            context.Salvar();


            #endregion

            #region Parametro
            context.Set<Parametro>()
                .AddOrUpdate(p => p.Chave,
                    new Parametro
                    {
                        DataCriacao = DateTime.Now,
                        Chave = Parametro.CHAVE_CRIPTOGRAFIA,
                        Valor = "IT_Newest_49387_In",
                        Ativo = true
                    },
                    new Parametro
                    {
                        DataCriacao = DateTime.Now,
                        Chave = Parametro.REMETENTE_EMAIL,
                        Valor = "raphael@itanio.com.br",
                        Ativo = true
                    },
                    new Parametro
                    {
                        DataCriacao = DateTime.Now,
                        Chave = Parametro.SMTP_PORTA,
                        Valor = "587",
                        Ativo = true
                    },
                    new Parametro
                    {
                        DataCriacao = DateTime.Now,
                        Chave = Parametro.SMTP_SENHA,
                        Valor = "raphakf061208",
                        Ativo = true
                    },
                    new Parametro
                    {
                        DataCriacao = DateTime.Now,
                        Chave = Parametro.SMTP_SERVIDOR,
                        Valor = "mail.exchange.locaweb.com.br",
                        Ativo = true
                    },
                    new Parametro
                    {
                        DataCriacao = DateTime.Now,
                        Chave = Parametro.SMTP_USA_SSL,
                        Valor = "true",
                        Ativo = true
                    },
                    new Parametro
                    {
                        DataCriacao = DateTime.Now,
                        Chave = Parametro.SMTP_USUARIO,
                        Valor = "raphael@itanio.com.br",
                        Ativo = true
                    },
                    new Parametro
                    {
                        DataCriacao = DateTime.Now,
                        Chave = Parametro.NOME_PROJETO,
                        Valor = "Código Nerd",
                        Ativo = true
                    },
                    new Parametro
                    {
                        DataCriacao = DateTime.Now,
                        Chave = Parametro.SMTP_USAR_CREDENCIAIS_PADRAO,
                        Valor = "false",
                        Ativo = true
                    },
                    new Parametro
                    {
                        DataCriacao = DateTime.Now,
                        Chave = Parametro.CHAVE_AREA_CONHECIMENTO_PLURAL,
                        Valor = "Áraes de Conhecimento",
                        Ativo = true
                    },
                    new Parametro
                    {
                        DataCriacao = DateTime.Now,
                        Chave = Parametro.CHAVE_AREA_CONHECIMENTO,
                        Valor = "Árae de Conhecimento",
                        Ativo = true
                    },
                    new Parametro
                    {
                        DataCriacao = DateTime.Now,
                        Chave = Parametro.CHAVE_ASSUNTO_PLURAL,
                        Valor = "Assuntos",
                        Ativo = true
                    },
                    new Parametro
                    {
                        DataCriacao = DateTime.Now,
                        Chave = Parametro.CHAVE_ASSUNTO,
                        Valor = "Assunto",
                        Ativo = true
                    },
                    new Parametro
                    {
                        DataCriacao = DateTime.Now,
                        Chave = Parametro.CHAVE_CURSO_PLURAL,
                        Valor = "Cursos",
                        Ativo = true
                    },
                    new Parametro
                    {
                        DataCriacao = DateTime.Now,
                        Chave = Parametro.CHAVE_CURSO,
                        Valor = "Curso",
                        Ativo = true
                    },
                    new Parametro
                    {
                        DataCriacao = DateTime.Now,
                        Chave = Parametro.CHAVE_AULA,
                        Valor = "Aula",
                        Ativo = true
                    },
                    new Parametro
                    {
                        DataCriacao = DateTime.Now,
                        Chave = Parametro.CHAVE_AULA_PLURAL,
                        Valor = "Aulas",
                        Ativo = true
                    },
                    new Parametro
                    {
                        DataCriacao = DateTime.Now,
                        Chave = Parametro.CHAVE_ARQUIVO,
                        Valor = "Material de apoio",
                        Ativo = true
                    },
                    new Parametro
                    {
                        DataCriacao = DateTime.Now,
                        Chave = Parametro.CHAVE_TITULO_AULAS_ASSISTIDAS,
                        Valor = "Últimas aulas assistidas",
                        Ativo = true
                    },
                    new Parametro
                    {
                        DataCriacao = DateTime.Now,
                        Chave = Parametro.CHAVE_TITULO_ULTIMAS_AULAS,
                        Valor = "Novas aulas",
                        Ativo = true
                    },

                     new Parametro
                     {
                         DataCriacao = DateTime.Now,
                         Chave = Parametro.CHAVE_STORAGE_ARQUIVOS,
                         Valor = "Content/Apoio",
                         Ativo = true
                     },
                    new Parametro
                    {
                        DataCriacao = DateTime.Now,
                        Chave = Parametro.CHAVE_NOME_DESTINATARIO_FALE_CONOSCO,
                        Valor = "Raphael Carubbi Neto",
                        Ativo = true
                    },
                    new Parametro
                    {
                        DataCriacao = DateTime.Now,
                        Chave = Parametro.CHAVE_EMAIL_DESTINATARIO_FALE_CONOSCO,
                        Valor = "rcarubbi@gmail.com",
                        Ativo = true
                    },
                      new Parametro
                      {
                          DataCriacao = DateTime.Now,
                          Chave = Parametro.CHAVE_CORPO_NOTIFICACAO_AULA_LIBERADA,
                          Valor = "Olá {Nome}, Tudo bem com você? <br /> Foi disponibilizado um novo trecho <a href='www.codigonerd.net/SmartLMS/Aula/Ver/{IdAula}'>{Aula}</a> na trilha <a href='www.codigonerd.net/SmartLMS/Aula/Index/{IdCurso}'>{Curso}</a> <br />Bons estudos! <br /><br /> Código Nerd",
                          Ativo = true
                      });



            context.Salvar();

            #endregion
        }
    }
}





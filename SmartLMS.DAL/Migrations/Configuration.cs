namespace SmartLMS.DAL.Migrations
{
    using Carubbi.Utils.Security;
    using Dominio.Entidades;
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

            var aluno = new Aluno
            {
                Nome = "Aluno",
                Ativo = true,
                Login = "aluno@itanio.com.br",
                Email = "raphael@itanio.com.br",
                Senha = criptografia.Encrypt("Aluno"),
                DataCriacao = DateTime.Now
            };

            context.Set<Usuario>().AddOrUpdate(u => u.Login, aluno);

            var professor = new Professor
            {
                Nome = "Professor",
                Ativo = true,
                Login = "professor@itanio.com.br",
                Email = "raphael@itanio.com.br",
                Senha = criptografia.Encrypt("Professor"),
                DataCriacao = DateTime.Now
            };

            context.Set<Usuario>().AddOrUpdate(u => u.Login, professor);

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
                        Valor = "raphakf061208",
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
                        Chave = Parametro.CHAVE_TITULO_ULTIMAS_AULAS,
                        Valor = "Novos trechos",
                        Ativo = true
                    },

                     new Parametro
                     {
                         Chave = Parametro.CHAVE_STORAGE_ARQUIVOS,
                         Valor = "Content/Apoio",
                         Ativo = true
                     },
                    new Parametro
                    {
                    Chave = Parametro.CHAVE_NOME_DESTINATARIO_FALE_CONOSCO,
                    Valor = "Raphael Carubbi Neto",
                    Ativo = true
                    },
                    new Parametro
                    {
                    Chave = Parametro.CHAVE_EMAIL_DESTINATARIO_FALE_CONOSCO,
                    Valor = "rcarubbi@gmail.com",
                    Ativo = true
                    });



            context.Salvar();

            #endregion

            #region AreasConhecimento
            var dotnet = new AreaConhecimento { Nome = ".net", Ordem = 1, Ativo = true };
            var web = new AreaConhecimento { Nome = "web", Ordem = 2, Ativo = true };
            var bancoDados = new AreaConhecimento { Nome = "Banco de dados", Ordem = 3, Ativo = true };

            context.Set<AreaConhecimento>().AddOrUpdate(u => u.Nome, dotnet, web, bancoDados);
            context.Salvar();
            #endregion

            #region Assuntos
            var csharp = new Assunto { Nome = "c#", Ordem = 1, Ativo = true, AreaConhecimento = dotnet };
            var windowsapps = new Assunto { Nome = "Windows Apps", Ordem = 2, Ativo = true, AreaConhecimento = dotnet };
            var aspnet = new Assunto { Nome = "Asp.net", Ordem = 3, Ativo = true, AreaConhecimento = dotnet };
            var xamarin = new Assunto { Nome = "Xamarin", Ordem = 4, Ativo = true, AreaConhecimento = dotnet };
            var WCF = new Assunto { Nome = "WCF", Ordem = 5, Ativo = true, AreaConhecimento = dotnet };
            var ORMs = new Assunto { Nome = "ORMs", Ordem = 6, Ativo = true, AreaConhecimento = dotnet };
            var HTML = new Assunto { Nome = "HTML", Ordem = 1, Ativo = true, AreaConhecimento = web };
            var CSS = new Assunto { Nome = "CSS", Ordem = 2, Ativo = true, AreaConhecimento = web };
            var Javascript = new Assunto { Nome = "Javascript", Ordem = 3, Ativo = true, AreaConhecimento = web };
            var SQLServer = new Assunto { Nome = "Microsoft SQL Server", Ordem = 1, Ativo = true, AreaConhecimento = bancoDados };

            context.Set<Assunto>().AddOrUpdate(u => u.Nome, csharp,
                windowsapps
                , aspnet
                , xamarin
                , WCF
                , ORMs
                , HTML
                , CSS
                , Javascript
                , SQLServer);
            context.Salvar();
            #endregion

            #region Cursos        
            var c1 = new Curso { Nome = "Conhecendo a linguagem e o ecossistema .net", Ordem = 1, Ativo = true, Assunto = csharp, Imagem = "csharp.png", ProfessorResponsavel = professor };
            var c2 = new Curso { Nome = "Explorando o Class Library mais a fundo", Ordem = 1, Ativo = true, Assunto = csharp, Imagem = "universalapps.jpg", ProfessorResponsavel = professor };
            var c3 = new Curso { Nome = "Manipulando dados com Ado.net", Ordem = 1, Ativo = true, Assunto = csharp, Imagem = "jquery.jpg", ProfessorResponsavel = professor };
            var c4 = new Curso { Nome = "Manipulando dados com LINQ", Ordem = 1, Ativo = true, Assunto = csharp, Imagem = "html.png", ProfessorResponsavel = professor };
            var c5 = new Curso { Nome = "Windows Forms", Ordem = 1, Ativo = true, Assunto = windowsapps, Imagem = "html.png", ProfessorResponsavel = professor };
            var c6 = new Curso { Nome = "Windows Services", Ordem = 1, Ativo = true, Assunto = windowsapps, Imagem = null, ProfessorResponsavel = professor };
            var c7 = new Curso { Nome = " WPF", Ordem = 1, Ativo = true, Assunto = windowsapps, Imagem = null, ProfessorResponsavel = professor };
            var c8 = new Curso { Nome = "Universal Apps", Ordem = 1, Ativo = true, Assunto = windowsapps, Imagem = "universalapps.jpg", ProfessorResponsavel = professor };
            var c9 = new Curso { Nome = "Web Forms", Ordem = 1, Ativo = true, Assunto = aspnet, Imagem = "aspnet.png", ProfessorResponsavel = professor };
            var c10 = new Curso { Nome = "Web Services", Ordem = 1, Ativo = true, Assunto = aspnet, Imagem = "mvc.png", ProfessorResponsavel = professor };
            var c11 = new Curso { Nome = "WebForms Ajax", Ordem = 1, Ativo = true, Assunto = aspnet, Imagem = "aspnet.png", ProfessorResponsavel = professor };
            var c12 = new Curso { Nome = "CSS Básico", Ordem = 1, Ativo = true, Assunto = CSS, Imagem = "css.png", ProfessorResponsavel = professor };
            var c13 = new Curso { Nome = "Entity Framework", Ordem = 1, Ativo = true, Assunto = ORMs, Imagem = "css.png", ProfessorResponsavel = professor };
            var c14 = new Curso { Nome = "NHibernate", Ordem = 1, Ativo = true, Assunto = ORMs, Imagem = "jquery.jpg", ProfessorResponsavel = professor };
            var c15 = new Curso { Nome = "Micro ORM's", Ordem = 1, Ativo = true, Assunto = ORMs, Imagem = "mvc.png", ProfessorResponsavel = professor };
            var c16 = new Curso { Nome = "Protocolo HTTP", Ordem = 1, Ativo = true, Assunto = HTML, Imagem = "jquery.jpg", ProfessorResponsavel = professor };
            var c17 = new Curso { Nome = "HTML5", Ordem = 1, Ativo = true, Assunto = HTML, Imagem = "html.png", ProfessorResponsavel = professor };
            var c18 = new Curso { Nome = "XML", Ordem = 1, Ativo = true, Assunto = HTML, Imagem = "csharp.png", ProfessorResponsavel = professor };

            context.Set<Curso>().AddOrUpdate(u => u.Nome, c1, c2, c3, c4, c5, c6, c7, c8, c9, c10, c11, c12, c13, c14, c15, c16, c17, c18);
            context.Salvar();
            #endregion

            #region Aulas         
            var a1 = new Aula { Curso = c1, Ativo = true, DataInclusao = DateTime.Now, Ordem = 1, Professor = professor, Tipo = Dominio.TipoConteudo.Vimeo, Nome = "visão geral da plataforma .net", Conteudo = "177458612" };
            var a2 = new Aula { Curso = c1, Ativo = true, DataInclusao = DateTime.Now, Ordem = 2, Professor = professor, Tipo = Dominio.TipoConteudo.Vimeo, Nome = "IDE's - Interface de desenvolvimento integrado", Conteudo = "177880740" };
            var a3 = new Aula { Curso = c1, Ativo = true, DataInclusao = DateTime.Now, Ordem = 3, Professor = professor, Tipo = Dominio.TipoConteudo.Vimeo, Nome = "Construindo a primeira aplicação em C#", Conteudo = "178036554" };
            var a4 = new Aula { Curso = c1, Ativo = true, DataInclusao = DateTime.Now, Ordem = 4, Professor = professor, Tipo = Dominio.TipoConteudo.Vimeo, Nome = "variáveis e operações", Conteudo = "153695056" };

            context.Set<Aula>().AddOrUpdate(u => u.Nome, a1, a2, a3, a4);
            context.Salvar();
            #endregion

            #region Arquivos 

            var ar1 = new Arquivo
            {
                Aula = a1,
                Curso = c1,
                ArquivoFisico = "Trecho 1 – Visão geral da plataforma .net.pdf",
                Nome = "Visão geral da plataforma .net",
                Ativo = true
            };

            context.Set<Arquivo>().AddOrUpdate(a => a.Nome, ar1);
            context.Salvar();

            #endregion

            #region Turmas 

            var t = new Turma { Curso = c1, DataInicio = new System.DateTime(2016, 08, 04), Ativo = true };
            context.Set<Turma>().AddOrUpdate(u => u.DataInicio, t);
            context.Salvar();

            var ta = new TurmaAluno { Aluno = aluno, DataIngresso = new System.DateTime(2016, 08, 04), Turma = t };
            context.Set<TurmaAluno>().AddOrUpdate(u => new { u.IdTurma, u.IdAluno }, ta);
            context.Salvar();


            var at = new AulaTurma { Aula = a1, Turma = t, DataDisponibilizacao = DateTime.Now };
            context.Set<AulaTurma>().AddOrUpdate(a => new { a.IdAula, a.IdTurma }, at);


            #endregion


        }
    }
}





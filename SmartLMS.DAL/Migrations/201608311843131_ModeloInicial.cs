namespace SmartLMS.DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ModeloInicial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Parametro",
                c => new
                    {
                        Id = c.Guid(nullable: false, identity: true),
                        Chave = c.String(),
                        Valor = c.String(),
                        Ativo = c.Boolean(nullable: false),
                        DataCriacao = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Log",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        EstadoAntigo = c.String(),
                        EstadoNovo = c.String(),
                        DataHora = c.DateTime(nullable: false),
                        IdEntitdade = c.Guid(nullable: false),
                        Tipo = c.String(),
                        Usuario_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Usuario", t => t.Usuario_Id)
                .Index(t => t.Usuario_Id);
            
            CreateTable(
                "dbo.Usuario",
                c => new
                    {
                        Id = c.Guid(nullable: false, identity: true),
                        Nome = c.String(),
                        Login = c.String(),
                        Senha = c.String(),
                        Email = c.String(),
                        Ativo = c.Boolean(nullable: false),
                        DataCriacao = c.DateTime(nullable: false),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.UsuarioAviso",
                c => new
                    {
                        IdUsuario = c.Guid(nullable: false),
                        IdAviso = c.Long(nullable: false),
                        DataVisualizacao = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => new { t.IdUsuario, t.IdAviso })
                .ForeignKey("dbo.Aviso", t => t.IdAviso, cascadeDelete: true)
                .ForeignKey("dbo.Usuario", t => t.IdUsuario, cascadeDelete: true)
                .Index(t => t.IdUsuario)
                .Index(t => t.IdAviso);
            
            CreateTable(
                "dbo.Aviso",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        Texto = c.String(),
                        DataHora = c.DateTime(nullable: false),
                        Turma_Id = c.Guid(),
                        Usuario_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Turma", t => t.Turma_Id)
                .ForeignKey("dbo.Usuario", t => t.Usuario_Id)
                .Index(t => t.Turma_Id)
                .Index(t => t.Usuario_Id);
            
            CreateTable(
                "dbo.Turma",
                c => new
                    {
                        Id = c.Guid(nullable: false, identity: true),
                        Nome = c.String(),
                        Ativo = c.Boolean(nullable: false),
                        DataCriacao = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.TurmaCurso",
                c => new
                    {
                        IdCurso = c.Guid(nullable: false),
                        IdTurma = c.Guid(nullable: false),
                        Ordem = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.IdCurso, t.IdTurma })
                .ForeignKey("dbo.Curso", t => t.IdCurso, cascadeDelete: true)
                .ForeignKey("dbo.Turma", t => t.IdTurma, cascadeDelete: true)
                .Index(t => t.IdCurso)
                .Index(t => t.IdTurma);
            
            CreateTable(
                "dbo.Curso",
                c => new
                    {
                        Id = c.Guid(nullable: false, identity: true),
                        Ordem = c.Int(nullable: false),
                        Imagem = c.String(),
                        Nome = c.String(),
                        Ativo = c.Boolean(nullable: false),
                        DataCriacao = c.DateTime(nullable: false),
                        Assunto_Id = c.Guid(nullable: false),
                        ProfessorResponsavel_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Assunto", t => t.Assunto_Id, cascadeDelete: true)
                .ForeignKey("dbo.Usuario", t => t.ProfessorResponsavel_Id)
                .Index(t => t.Assunto_Id)
                .Index(t => t.ProfessorResponsavel_Id);
            
            CreateTable(
                "dbo.Arquivo",
                c => new
                    {
                        Id = c.Guid(nullable: false, identity: true),
                        Nome = c.String(),
                        ArquivoFisico = c.String(),
                        Ativo = c.Boolean(nullable: false),
                        DataCriacao = c.DateTime(nullable: false),
                        Aula_Id = c.Guid(),
                        Curso_Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Aula", t => t.Aula_Id)
                .ForeignKey("dbo.Curso", t => t.Curso_Id, cascadeDelete: true)
                .Index(t => t.Aula_Id)
                .Index(t => t.Curso_Id);
            
            CreateTable(
                "dbo.AcessoArquivo",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        DataHoraAcesso = c.DateTime(nullable: false),
                        Arquivo_Id = c.Guid(nullable: false),
                        Usuario_Id = c.Guid(nullable: false),
                        Aluno_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Arquivo", t => t.Arquivo_Id, cascadeDelete: true)
                .ForeignKey("dbo.Usuario", t => t.Usuario_Id, cascadeDelete: true)
                .ForeignKey("dbo.Usuario", t => t.Aluno_Id)
                .Index(t => t.Arquivo_Id)
                .Index(t => t.Usuario_Id)
                .Index(t => t.Aluno_Id);
            
            CreateTable(
                "dbo.Aula",
                c => new
                    {
                        Id = c.Guid(nullable: false, identity: true),
                        DiasLiberacao = c.Int(nullable: false),
                        Nome = c.String(),
                        Conteudo = c.String(),
                        Tipo = c.Int(nullable: false),
                        Ordem = c.Int(nullable: false),
                        Ativo = c.Boolean(nullable: false),
                        DataCriacao = c.DateTime(nullable: false),
                        Professor_Id = c.Guid(),
                        Curso_Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Usuario", t => t.Professor_Id)
                .ForeignKey("dbo.Curso", t => t.Curso_Id, cascadeDelete: true)
                .Index(t => t.Professor_Id)
                .Index(t => t.Curso_Id);
            
            CreateTable(
                "dbo.AcessoAula",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        DataHoraAcesso = c.DateTime(nullable: false),
                        Percentual = c.Int(nullable: false),
                        Segundos = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Aula_Id = c.Guid(nullable: false),
                        Usuario_Id = c.Guid(nullable: false),
                        Aluno_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Aula", t => t.Aula_Id, cascadeDelete: true)
                .ForeignKey("dbo.Usuario", t => t.Usuario_Id, cascadeDelete: true)
                .ForeignKey("dbo.Usuario", t => t.Aluno_Id)
                .Index(t => t.Aula_Id)
                .Index(t => t.Usuario_Id)
                .Index(t => t.Aluno_Id);
            
            CreateTable(
                "dbo.Comentario",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        DataHora = c.DateTime(nullable: false),
                        TextoComentario = c.String(),
                        Aula_Id = c.Guid(),
                        Usuario_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Aula", t => t.Aula_Id)
                .ForeignKey("dbo.Usuario", t => t.Usuario_Id)
                .Index(t => t.Aula_Id)
                .Index(t => t.Usuario_Id);
            
            CreateTable(
                "dbo.AulaPlanejamento",
                c => new
                    {
                        IdAula = c.Guid(nullable: false),
                        IdPlanejamento = c.Long(nullable: false),
                        DataLiberacao = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => new { t.IdAula, t.IdPlanejamento })
                .ForeignKey("dbo.Aula", t => t.IdAula, cascadeDelete: true)
                .ForeignKey("dbo.Planejamento", t => t.IdPlanejamento, cascadeDelete: true)
                .Index(t => t.IdAula)
                .Index(t => t.IdPlanejamento);
            
            CreateTable(
                "dbo.Planejamento",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        DataInicio = c.DateTime(nullable: false),
                        Concluido = c.Boolean(nullable: false),
                        Turma_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Turma", t => t.Turma_Id)
                .Index(t => t.Turma_Id);
            
            CreateTable(
                "dbo.Assunto",
                c => new
                    {
                        Id = c.Guid(nullable: false, identity: true),
                        Ordem = c.Int(nullable: false),
                        Nome = c.String(),
                        Ativo = c.Boolean(nullable: false),
                        DataCriacao = c.DateTime(nullable: false),
                        AreaConhecimento_Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AreaConhecimento", t => t.AreaConhecimento_Id, cascadeDelete: true)
                .Index(t => t.AreaConhecimento_Id);
            
            CreateTable(
                "dbo.AreaConhecimento",
                c => new
                    {
                        Id = c.Guid(nullable: false, identity: true),
                        Ordem = c.Int(nullable: false),
                        Nome = c.String(),
                        Ativo = c.Boolean(nullable: false),
                        DataCriacao = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.AlunoPlanejamento",
                c => new
                    {
                        Aluno_Id = c.Guid(nullable: false),
                        Planejamento_Id = c.Long(nullable: false),
                    })
                .PrimaryKey(t => new { t.Aluno_Id, t.Planejamento_Id })
                .ForeignKey("dbo.Usuario", t => t.Aluno_Id, cascadeDelete: true)
                .ForeignKey("dbo.Planejamento", t => t.Planejamento_Id, cascadeDelete: true)
                .Index(t => t.Aluno_Id)
                .Index(t => t.Planejamento_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Log", "Usuario_Id", "dbo.Usuario");
            DropForeignKey("dbo.UsuarioAviso", "IdUsuario", "dbo.Usuario");
            DropForeignKey("dbo.UsuarioAviso", "IdAviso", "dbo.Aviso");
            DropForeignKey("dbo.Aviso", "Usuario_Id", "dbo.Usuario");
            DropForeignKey("dbo.Aviso", "Turma_Id", "dbo.Turma");
            DropForeignKey("dbo.TurmaCurso", "IdTurma", "dbo.Turma");
            DropForeignKey("dbo.TurmaCurso", "IdCurso", "dbo.Curso");
            DropForeignKey("dbo.Curso", "ProfessorResponsavel_Id", "dbo.Usuario");
            DropForeignKey("dbo.Aula", "Curso_Id", "dbo.Curso");
            DropForeignKey("dbo.Curso", "Assunto_Id", "dbo.Assunto");
            DropForeignKey("dbo.Assunto", "AreaConhecimento_Id", "dbo.AreaConhecimento");
            DropForeignKey("dbo.Arquivo", "Curso_Id", "dbo.Curso");
            DropForeignKey("dbo.Arquivo", "Aula_Id", "dbo.Aula");
            DropForeignKey("dbo.Aula", "Professor_Id", "dbo.Usuario");
            DropForeignKey("dbo.AulaPlanejamento", "IdPlanejamento", "dbo.Planejamento");
            DropForeignKey("dbo.Planejamento", "Turma_Id", "dbo.Turma");
            DropForeignKey("dbo.AlunoPlanejamento", "Planejamento_Id", "dbo.Planejamento");
            DropForeignKey("dbo.AlunoPlanejamento", "Aluno_Id", "dbo.Usuario");
            DropForeignKey("dbo.AcessoAula", "Aluno_Id", "dbo.Usuario");
            DropForeignKey("dbo.AcessoArquivo", "Aluno_Id", "dbo.Usuario");
            DropForeignKey("dbo.AulaPlanejamento", "IdAula", "dbo.Aula");
            DropForeignKey("dbo.Comentario", "Usuario_Id", "dbo.Usuario");
            DropForeignKey("dbo.Comentario", "Aula_Id", "dbo.Aula");
            DropForeignKey("dbo.AcessoAula", "Usuario_Id", "dbo.Usuario");
            DropForeignKey("dbo.AcessoAula", "Aula_Id", "dbo.Aula");
            DropForeignKey("dbo.AcessoArquivo", "Usuario_Id", "dbo.Usuario");
            DropForeignKey("dbo.AcessoArquivo", "Arquivo_Id", "dbo.Arquivo");
            DropIndex("dbo.AlunoPlanejamento", new[] { "Planejamento_Id" });
            DropIndex("dbo.AlunoPlanejamento", new[] { "Aluno_Id" });
            DropIndex("dbo.Assunto", new[] { "AreaConhecimento_Id" });
            DropIndex("dbo.Planejamento", new[] { "Turma_Id" });
            DropIndex("dbo.AulaPlanejamento", new[] { "IdPlanejamento" });
            DropIndex("dbo.AulaPlanejamento", new[] { "IdAula" });
            DropIndex("dbo.Comentario", new[] { "Usuario_Id" });
            DropIndex("dbo.Comentario", new[] { "Aula_Id" });
            DropIndex("dbo.AcessoAula", new[] { "Aluno_Id" });
            DropIndex("dbo.AcessoAula", new[] { "Usuario_Id" });
            DropIndex("dbo.AcessoAula", new[] { "Aula_Id" });
            DropIndex("dbo.Aula", new[] { "Curso_Id" });
            DropIndex("dbo.Aula", new[] { "Professor_Id" });
            DropIndex("dbo.AcessoArquivo", new[] { "Aluno_Id" });
            DropIndex("dbo.AcessoArquivo", new[] { "Usuario_Id" });
            DropIndex("dbo.AcessoArquivo", new[] { "Arquivo_Id" });
            DropIndex("dbo.Arquivo", new[] { "Curso_Id" });
            DropIndex("dbo.Arquivo", new[] { "Aula_Id" });
            DropIndex("dbo.Curso", new[] { "ProfessorResponsavel_Id" });
            DropIndex("dbo.Curso", new[] { "Assunto_Id" });
            DropIndex("dbo.TurmaCurso", new[] { "IdTurma" });
            DropIndex("dbo.TurmaCurso", new[] { "IdCurso" });
            DropIndex("dbo.Aviso", new[] { "Usuario_Id" });
            DropIndex("dbo.Aviso", new[] { "Turma_Id" });
            DropIndex("dbo.UsuarioAviso", new[] { "IdAviso" });
            DropIndex("dbo.UsuarioAviso", new[] { "IdUsuario" });
            DropIndex("dbo.Log", new[] { "Usuario_Id" });
            DropTable("dbo.AlunoPlanejamento");
            DropTable("dbo.AreaConhecimento");
            DropTable("dbo.Assunto");
            DropTable("dbo.Planejamento");
            DropTable("dbo.AulaPlanejamento");
            DropTable("dbo.Comentario");
            DropTable("dbo.AcessoAula");
            DropTable("dbo.Aula");
            DropTable("dbo.AcessoArquivo");
            DropTable("dbo.Arquivo");
            DropTable("dbo.Curso");
            DropTable("dbo.TurmaCurso");
            DropTable("dbo.Turma");
            DropTable("dbo.Aviso");
            DropTable("dbo.UsuarioAviso");
            DropTable("dbo.Usuario");
            DropTable("dbo.Log");
            DropTable("dbo.Parametro");
        }
    }
}

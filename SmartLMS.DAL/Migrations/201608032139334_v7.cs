namespace SmartLMS.DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v7 : DbMigration
    {
        public override void Up()
        {
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
                        Aluno_Id = c.Guid(),
                        Turma_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Usuario", t => t.Aluno_Id)
                .ForeignKey("dbo.Turma", t => t.Turma_Id)
                .Index(t => t.Aluno_Id)
                .Index(t => t.Turma_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UsuarioAviso", "IdUsuario", "dbo.Usuario");
            DropForeignKey("dbo.UsuarioAviso", "IdAviso", "dbo.Aviso");
            DropForeignKey("dbo.Aviso", "Turma_Id", "dbo.Turma");
            DropForeignKey("dbo.Aviso", "Aluno_Id", "dbo.Usuario");
            DropIndex("dbo.Aviso", new[] { "Turma_Id" });
            DropIndex("dbo.Aviso", new[] { "Aluno_Id" });
            DropIndex("dbo.UsuarioAviso", new[] { "IdAviso" });
            DropIndex("dbo.UsuarioAviso", new[] { "IdUsuario" });
            DropTable("dbo.Aviso");
            DropTable("dbo.UsuarioAviso");
        }
    }
}

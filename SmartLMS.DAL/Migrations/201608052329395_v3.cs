namespace SmartLMS.DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v3 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.AcessoArquivo", "Aluno_Id", "dbo.Usuario");
            DropIndex("dbo.AcessoArquivo", new[] { "Aluno_Id" });
            AddColumn("dbo.AcessoArquivo", "Usuario_Id", c => c.Guid(nullable: false));
            AddColumn("dbo.Arquivo", "Curso_Id", c => c.Guid(nullable: false));
            AlterColumn("dbo.AcessoArquivo", "Aluno_Id", c => c.Guid());
            CreateIndex("dbo.AcessoArquivo", "Usuario_Id");
            CreateIndex("dbo.AcessoArquivo", "Aluno_Id");
            CreateIndex("dbo.Arquivo", "Curso_Id");
            AddForeignKey("dbo.Arquivo", "Curso_Id", "dbo.Curso", "Id", cascadeDelete: true);
            AddForeignKey("dbo.AcessoArquivo", "Usuario_Id", "dbo.Usuario", "Id", cascadeDelete: true);
            AddForeignKey("dbo.AcessoArquivo", "Aluno_Id", "dbo.Usuario", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AcessoArquivo", "Aluno_Id", "dbo.Usuario");
            DropForeignKey("dbo.AcessoArquivo", "Usuario_Id", "dbo.Usuario");
            DropForeignKey("dbo.Arquivo", "Curso_Id", "dbo.Curso");
            DropIndex("dbo.Arquivo", new[] { "Curso_Id" });
            DropIndex("dbo.AcessoArquivo", new[] { "Aluno_Id" });
            DropIndex("dbo.AcessoArquivo", new[] { "Usuario_Id" });
            AlterColumn("dbo.AcessoArquivo", "Aluno_Id", c => c.Guid(nullable: false));
            DropColumn("dbo.Arquivo", "Curso_Id");
            DropColumn("dbo.AcessoArquivo", "Usuario_Id");
            CreateIndex("dbo.AcessoArquivo", "Aluno_Id");
            AddForeignKey("dbo.AcessoArquivo", "Aluno_Id", "dbo.Usuario", "Id", cascadeDelete: true);
        }
    }
}

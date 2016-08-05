namespace SmartLMS.DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v2 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.AcessoAula", "Aluno_Id", "dbo.Usuario");
            DropIndex("dbo.AcessoAula", new[] { "Aluno_Id" });
            AddColumn("dbo.AcessoAula", "Segundos", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.AcessoAula", "Usuario_Id", c => c.Guid(nullable: false));
            AlterColumn("dbo.AcessoAula", "Aluno_Id", c => c.Guid());
            CreateIndex("dbo.AcessoAula", "Usuario_Id");
            CreateIndex("dbo.AcessoAula", "Aluno_Id");
            AddForeignKey("dbo.AcessoAula", "Usuario_Id", "dbo.Usuario", "Id", cascadeDelete: true);
            AddForeignKey("dbo.AcessoAula", "Aluno_Id", "dbo.Usuario", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AcessoAula", "Aluno_Id", "dbo.Usuario");
            DropForeignKey("dbo.AcessoAula", "Usuario_Id", "dbo.Usuario");
            DropIndex("dbo.AcessoAula", new[] { "Aluno_Id" });
            DropIndex("dbo.AcessoAula", new[] { "Usuario_Id" });
            AlterColumn("dbo.AcessoAula", "Aluno_Id", c => c.Guid(nullable: false));
            DropColumn("dbo.AcessoAula", "Usuario_Id");
            DropColumn("dbo.AcessoAula", "Segundos");
            CreateIndex("dbo.AcessoAula", "Aluno_Id");
            AddForeignKey("dbo.AcessoAula", "Aluno_Id", "dbo.Usuario", "Id", cascadeDelete: true);
        }
    }
}

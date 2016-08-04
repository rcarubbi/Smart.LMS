namespace SmartLMS.DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v8 : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.Aviso", name: "Aluno_Id", newName: "Usuario_Id");
            RenameIndex(table: "dbo.Aviso", name: "IX_Aluno_Id", newName: "IX_Usuario_Id");
            AlterColumn("dbo.UsuarioAviso", "DataVisualizacao", c => c.DateTime());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.UsuarioAviso", "DataVisualizacao", c => c.DateTime(nullable: false));
            RenameIndex(table: "dbo.Aviso", name: "IX_Usuario_Id", newName: "IX_Aluno_Id");
            RenameColumn(table: "dbo.Aviso", name: "Usuario_Id", newName: "Aluno_Id");
        }
    }
}

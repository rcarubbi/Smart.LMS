namespace SmartLMS.DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v2 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Aviso", "Turma_Id", "dbo.Turma");
            DropIndex("dbo.Aviso", new[] { "Turma_Id" });
            AddColumn("dbo.Aviso", "Planejamento_Id", c => c.Long());
            CreateIndex("dbo.Aviso", "Planejamento_Id");
            AddForeignKey("dbo.Aviso", "Planejamento_Id", "dbo.Planejamento", "Id");
            DropColumn("dbo.Aviso", "Turma_Id");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Aviso", "Turma_Id", c => c.Guid());
            DropForeignKey("dbo.Aviso", "Planejamento_Id", "dbo.Planejamento");
            DropIndex("dbo.Aviso", new[] { "Planejamento_Id" });
            DropColumn("dbo.Aviso", "Planejamento_Id");
            CreateIndex("dbo.Aviso", "Turma_Id");
            AddForeignKey("dbo.Aviso", "Turma_Id", "dbo.Turma", "Id");
        }
    }
}

namespace SmartLMS.DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v4 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Curso", "Ordem", c => c.Int(nullable: false));
            AddColumn("dbo.Assunto", "Ordem", c => c.Int(nullable: false));
            AddColumn("dbo.AreaConhecimento", "Ordem", c => c.Int(nullable: false));
            AddColumn("dbo.Aula", "Ordem", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Aula", "Ordem");
            DropColumn("dbo.AreaConhecimento", "Ordem");
            DropColumn("dbo.Assunto", "Ordem");
            DropColumn("dbo.Curso", "Ordem");
        }
    }
}

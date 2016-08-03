namespace SmartLMS.DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v5 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Curso", "Imagem", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Curso", "Imagem");
        }
    }
}

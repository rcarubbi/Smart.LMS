namespace SmartLMS.DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class V3 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AcessoAula", "Percentual", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.AcessoAula", "Percentual");
        }
    }
}

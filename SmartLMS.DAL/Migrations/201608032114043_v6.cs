namespace SmartLMS.DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v6 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Aula", "DataInclusao", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Aula", "DataInclusao");
        }
    }
}

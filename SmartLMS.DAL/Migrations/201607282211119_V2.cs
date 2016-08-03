namespace SmartLMS.DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class V2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Assunto", "Nome", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Assunto", "Nome");
        }
    }
}

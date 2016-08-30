namespace SmartLMS.DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class dataCriacaoUsuario : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Usuario", "DataCriacao", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Usuario", "DataCriacao");
        }
    }
}

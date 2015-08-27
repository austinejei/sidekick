namespace DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class App_Added_IsOAuth : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Apps", "IsOAuth", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Apps", "IsOAuth");
        }
    }
}

namespace DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class App_Added_AppUrl : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Apps", "AppUrl", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Apps", "AppUrl");
        }
    }
}

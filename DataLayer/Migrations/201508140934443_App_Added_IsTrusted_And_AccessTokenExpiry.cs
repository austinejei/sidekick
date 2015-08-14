namespace DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class App_Added_IsTrusted_And_AccessTokenExpiry : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Apps", "IsTrusted", c => c.Boolean(nullable: false));
            AddColumn("dbo.Apps", "AccessTokenExpiry", c => c.Time(nullable: false, precision: 7));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Apps", "AccessTokenExpiry");
            DropColumn("dbo.Apps", "IsTrusted");
        }
    }
}

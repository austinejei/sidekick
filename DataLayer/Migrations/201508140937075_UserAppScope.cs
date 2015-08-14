namespace DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UserAppScope : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.AppScopes", newName: "UserAppScopes");
            CreateTable(
                "dbo.AppScopes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AppId = c.Int(nullable: false),
                        OAuthScopeId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.AppId)
                .Index(t => t.OAuthScopeId);
            
            AlterColumn("dbo.UserAppScopes", "Enabled", c => c.Boolean(nullable: false));
            DropColumn("dbo.UserAppScopes", "Discriminator");
        }
        
        public override void Down()
        {
            AddColumn("dbo.UserAppScopes", "Discriminator", c => c.String(nullable: false, maxLength: 128));
            DropIndex("dbo.AppScopes", new[] { "OAuthScopeId" });
            DropIndex("dbo.AppScopes", new[] { "AppId" });
            AlterColumn("dbo.UserAppScopes", "Enabled", c => c.Boolean());
            DropTable("dbo.AppScopes");
            RenameTable(name: "dbo.UserAppScopes", newName: "AppScopes");
        }
    }
}

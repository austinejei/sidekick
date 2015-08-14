namespace DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class App_Added_SsoEncryptionKey : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Apps", "SsoEncryptionKey", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Apps", "SsoEncryptionKey");
        }
    }
}

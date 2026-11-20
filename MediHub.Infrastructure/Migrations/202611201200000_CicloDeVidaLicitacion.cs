namespace MediHub.Infrastructure.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CicloDeVidaLicitacion : DbMigration
    {
        public override void Up()
        {
            // Las licitaciones existentes ya reciben ofertas, por eso quedan como Publicada (2).
            AddColumn("dbo.Licitacion", "EstadoDelProceso", c => c.Int(nullable: false, defaultValue: 2));
            AddColumn("dbo.Licitacion", "IdOfertaAdjudicada", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Licitacion", "IdOfertaAdjudicada");
            DropColumn("dbo.Licitacion", "EstadoDelProceso");
        }
    }
}

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

            // Las que ya tienen ganador en el portal pasan a Adjudicada (4).
            Sql("UPDATE dbo.Licitacion SET EstadoDelProceso = 4 " +
                "WHERE IdLicitacion IN (SELECT IdLicitacion FROM dbo.Adjudicacion)");
        }
        
        public override void Down()
        {
            DropColumn("dbo.Licitacion", "EstadoDelProceso");
        }
    }
}

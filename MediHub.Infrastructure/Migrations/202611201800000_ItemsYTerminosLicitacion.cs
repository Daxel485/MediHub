namespace MediHub.Infrastructure.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class ItemsYTerminosLicitacion : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Licitacion", "RutaTerminos", c => c.String(maxLength: 500, unicode: false));

            CreateTable(
                "dbo.ItemLicitacion",
                c => new
                {
                    IdItemLicitacion = c.Int(nullable: false, identity: true),
                    IdLicitacion     = c.Int(nullable: false),
                    Descripcion      = c.String(nullable: false, maxLength: 500, unicode: false),
                    CantidadRequerida = c.Decimal(nullable: false, precision: 18, scale: 4),
                    Unidad           = c.String(nullable: false, maxLength: 50, unicode: false),
                    VidaUtilMeses    = c.Int(nullable: false),
                    RequiereCadenaFrio = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.IdItemLicitacion)
                .ForeignKey("dbo.Licitacion", t => t.IdLicitacion)
                .Index(t => t.IdLicitacion);
        }

        public override void Down()
        {
            DropIndex("dbo.ItemLicitacion", new[] { "IdLicitacion" });
            DropForeignKey("dbo.ItemLicitacion", "IdLicitacion", "dbo.Licitacion");
            DropTable("dbo.ItemLicitacion");
            DropColumn("dbo.Licitacion", "RutaTerminos");
        }
    }
}

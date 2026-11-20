namespace MediHub.Infrastructure.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class PortalOfertas : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Adjudicacion",
                c => new
                    {
                        IdLicitacion = c.Int(nullable: false),
                        IdOferta = c.Int(nullable: false),
                        IdComprador = c.Guid(nullable: false),
                        FechaDeAdjudicacion = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.IdLicitacion)
                .ForeignKey("dbo.Licitacion", t => t.IdLicitacion)
                .ForeignKey("dbo.Oferta", t => t.IdOferta)
                .Index(t => t.IdLicitacion)
                .Index(t => t.IdOferta);

            CreateTable(
                "dbo.DetalleOferta",
                c => new
                    {
                        IdDetalleOferta = c.Int(nullable: false, identity: true),
                        IdOferta = c.Int(nullable: false),
                        IdItemLicitacion = c.Int(),
                        DescripcionItem = c.String(nullable: false, maxLength: 500),
                        Cantidad = c.Decimal(nullable: false, precision: 18, scale: 4),
                        PrecioUnitario = c.Decimal(nullable: false, precision: 18, scale: 2),
                        FechaVencimientoLote = c.DateTime(nullable: false),
                        Puntaje = c.Decimal(precision: 7, scale: 4),
                    })
                .PrimaryKey(t => t.IdDetalleOferta)
                .ForeignKey("dbo.Oferta", t => t.IdOferta)
                .Index(t => t.IdOferta);

            AddColumn("dbo.Oferta", "PlazoEntregaDias", c => c.Int());

            // El índice protege los envíos del portal y conserva las ofertas históricas sin plazo.
            Sql(@"CREATE UNIQUE INDEX UX_Oferta_Licitacion_Proveedor_Portal
                  ON dbo.Oferta (IdLicitacion, IdProveedor)
                  WHERE PlazoEntregaDias IS NOT NULL");
        }

        public override void Down()
        {
            Sql("DROP INDEX UX_Oferta_Licitacion_Proveedor_Portal ON dbo.Oferta");
            DropForeignKey("dbo.Adjudicacion", "IdOferta", "dbo.Oferta");
            DropForeignKey("dbo.Adjudicacion", "IdLicitacion", "dbo.Licitacion");
            DropForeignKey("dbo.DetalleOferta", "IdOferta", "dbo.Oferta");
            DropIndex("dbo.DetalleOferta", new[] { "IdOferta" });
            DropIndex("dbo.Adjudicacion", new[] { "IdOferta" });
            DropIndex("dbo.Adjudicacion", new[] { "IdLicitacion" });
            DropColumn("dbo.Oferta", "PlazoEntregaDias");
            DropTable("dbo.DetalleOferta");
            DropTable("dbo.Adjudicacion");
        }
    }
}

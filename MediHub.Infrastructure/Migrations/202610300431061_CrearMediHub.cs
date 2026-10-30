namespace MediHub.Infrastructure.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CrearMediHub : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.BITACORA_EVENTOS",
                c => new
                    {
                        IdEvento = c.Int(nullable: false, identity: true),
                        TablaDeEvento = c.String(nullable: false, maxLength: 50, unicode: false),
                        TipoDeEvento = c.String(nullable: false, maxLength: 20, unicode: false),
                        FechaDeEvento = c.DateTime(nullable: false),
                        DescripcionDeEvento = c.String(nullable: false, unicode: false),
                        StackTrace = c.String(unicode: false),
                        DatosAnteriores = c.String(unicode: false),
                        DatosPosteriores = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.IdEvento);
            
            CreateTable(
                "dbo.ConfiguracionProveedor",
                c => new
                    {
                        IdConfiguracion = c.Int(nullable: false, identity: true),
                        IdProveedor = c.Int(nullable: false),
                        TipoConfiguracion = c.Int(nullable: false),
                        Comision = c.Int(nullable: false),
                        FechaDeRegistro = c.DateTime(nullable: false),
                        FechaDeModificacion = c.DateTime(),
                        Estado = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.IdConfiguracion)
                .ForeignKey("dbo.Proveedor", t => t.IdProveedor)
                .Index(t => t.IdProveedor, unique: true, name: "IX_ConfiguracionProveedor_IdProveedor");
            
            CreateTable(
                "dbo.Proveedor",
                c => new
                    {
                        IdProveedor = c.Int(nullable: false, identity: true),
                        Nombre = c.String(nullable: false, maxLength: 150, unicode: false),
                        Identificacion = c.String(nullable: false, maxLength: 12, unicode: false),
                        CorreoElectronico = c.String(nullable: false, maxLength: 200, unicode: false),
                        Telefono = c.String(nullable: false, maxLength: 8, unicode: false),
                        FechaDeRegistro = c.DateTime(nullable: false),
                        FechaDeModificacion = c.DateTime(),
                        Estado = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.IdProveedor)
                .Index(t => t.Identificacion, unique: true, name: "IX_Proveedor_Identificacion");
            
            CreateTable(
                "dbo.Oferta",
                c => new
                    {
                        IdOferta = c.Int(nullable: false, identity: true),
                        IdLicitacion = c.Int(nullable: false),
                        IdProveedor = c.Int(nullable: false),
                        Monto = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Descripcion = c.String(maxLength: 500, unicode: false),
                        FechaDeEnvio = c.DateTime(nullable: false),
                        Sincronizada = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.IdOferta)
                .ForeignKey("dbo.Licitacion", t => t.IdLicitacion)
                .ForeignKey("dbo.Proveedor", t => t.IdProveedor)
                .Index(t => t.IdLicitacion)
                .Index(t => t.IdProveedor);
            
            CreateTable(
                "dbo.Licitacion",
                c => new
                    {
                        IdLicitacion = c.Int(nullable: false, identity: true),
                        Titulo = c.String(nullable: false, maxLength: 150, unicode: false),
                        Descripcion = c.String(maxLength: 500, unicode: false),
                        FechaDeCierre = c.DateTime(nullable: false),
                        FechaDeRegistro = c.DateTime(nullable: false),
                        FechaDeModificacion = c.DateTime(),
                        Estado = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.IdLicitacion)
                .Index(t => t.Titulo, unique: true, name: "IX_Licitacion_Titulo");
            
            CreateTable(
                "dbo.Usuario",
                c => new
                    {
                        IdUsuario = c.Int(nullable: false, identity: true),
                        IdProveedor = c.Int(nullable: false),
                        IdNetUser = c.Guid(),
                        Nombres = c.String(nullable: false, maxLength: 100, unicode: false),
                        PrimerApellido = c.String(nullable: false, maxLength: 100, unicode: false),
                        SegundoApellido = c.String(nullable: false, maxLength: 100, unicode: false),
                        Identificacion = c.String(nullable: false, maxLength: 10, unicode: false),
                        CorreoElectronico = c.String(nullable: false, maxLength: 200, unicode: false),
                        FechaDeRegistro = c.DateTime(nullable: false),
                        FechaDeModificacion = c.DateTime(),
                        Estado = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.IdUsuario)
                .ForeignKey("dbo.Proveedor", t => t.IdProveedor)
                .Index(t => t.IdProveedor)
                .Index(t => t.Identificacion, unique: true, name: "IX_Usuario_Identificacion");
            
            CreateTable(
                "dbo.ReporteMensual",
                c => new
                    {
                        IdReporte = c.Int(nullable: false, identity: true),
                        IdProveedor = c.Int(nullable: false),
                        CantidadDeLicitaciones = c.Int(nullable: false),
                        MontoTotalOfertado = c.Decimal(nullable: false, precision: 18, scale: 2),
                        CantidadDeOfertas = c.Int(nullable: false),
                        MontoTotalComision = c.Decimal(nullable: false, precision: 18, scale: 2),
                        FechaDelReporte = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.IdReporte)
                .ForeignKey("dbo.Proveedor", t => t.IdProveedor)
                .Index(t => t.IdProveedor);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ReporteMensual", "IdProveedor", "dbo.Proveedor");
            DropForeignKey("dbo.ConfiguracionProveedor", "IdProveedor", "dbo.Proveedor");
            DropForeignKey("dbo.Usuario", "IdProveedor", "dbo.Proveedor");
            DropForeignKey("dbo.Oferta", "IdProveedor", "dbo.Proveedor");
            DropForeignKey("dbo.Oferta", "IdLicitacion", "dbo.Licitacion");
            DropIndex("dbo.ReporteMensual", new[] { "IdProveedor" });
            DropIndex("dbo.Usuario", "IX_Usuario_Identificacion");
            DropIndex("dbo.Usuario", new[] { "IdProveedor" });
            DropIndex("dbo.Licitacion", "IX_Licitacion_Titulo");
            DropIndex("dbo.Oferta", new[] { "IdProveedor" });
            DropIndex("dbo.Oferta", new[] { "IdLicitacion" });
            DropIndex("dbo.Proveedor", "IX_Proveedor_Identificacion");
            DropIndex("dbo.ConfiguracionProveedor", "IX_ConfiguracionProveedor_IdProveedor");
            DropTable("dbo.ReporteMensual");
            DropTable("dbo.Usuario");
            DropTable("dbo.Licitacion");
            DropTable("dbo.Oferta");
            DropTable("dbo.Proveedor");
            DropTable("dbo.ConfiguracionProveedor");
            DropTable("dbo.BITACORA_EVENTOS");
        }
    }
}

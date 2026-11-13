using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration.Conventions;
using MediHub.Domain.Entities;

namespace MediHub.Infrastructure.Data
{
    public class MediHubContext : DbContext
    {
        public MediHubContext() : base("name=MediHubConnection")
        {
        }

        public DbSet<Proveedor> Proveedores { get; set; }
        public DbSet<ConfiguracionProveedor> Configuraciones { get; set; }
        public DbSet<ReporteMensual> Reportes { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Licitacion> Licitaciones { get; set; }
        public DbSet<Oferta> Ofertas { get; set; }
        public DbSet<BitacoraEvento> Bitacora { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();

            ConfigurarProveedor(modelBuilder);
            ConfigurarConfiguracion(modelBuilder);
            ConfigurarReporte(modelBuilder);
            ConfigurarUsuario(modelBuilder);
            ConfigurarLicitacion(modelBuilder);
            ConfigurarOferta(modelBuilder);
            ConfigurarBitacora(modelBuilder);
        }

        private static IndexAnnotation IndiceUnico(string nombre)
        {
            return new IndexAnnotation(new IndexAttribute(nombre) { IsUnique = true });
        }

        private static void ConfigurarProveedor(DbModelBuilder mb)
        {
            var e = mb.Entity<Proveedor>();
            e.ToTable("Proveedor").HasKey(p => p.IdProveedor);
            e.Property(p => p.Nombre).IsRequired().HasMaxLength(150).IsUnicode(false);
            e.Property(p => p.Identificacion).IsRequired().HasMaxLength(12).IsUnicode(false)
                .HasColumnAnnotation(IndexAnnotation.AnnotationName, IndiceUnico("IX_Proveedor_Identificacion"));
            e.Property(p => p.CorreoElectronico).IsRequired().HasMaxLength(200).IsUnicode(false);
            e.Property(p => p.Telefono).IsRequired().HasMaxLength(8).IsUnicode(false);
        }

        private static void ConfigurarConfiguracion(DbModelBuilder mb)
        {
            var e = mb.Entity<ConfiguracionProveedor>();
            e.ToTable("ConfiguracionProveedor").HasKey(c => c.IdConfiguracion);
            e.Property(c => c.IdProveedor)
                .HasColumnAnnotation(IndexAnnotation.AnnotationName, IndiceUnico("IX_ConfiguracionProveedor_IdProveedor"));
            e.HasRequired(c => c.Proveedor).WithMany().HasForeignKey(c => c.IdProveedor);
        }

        private static void ConfigurarReporte(DbModelBuilder mb)
        {
            var e = mb.Entity<ReporteMensual>();
            e.ToTable("ReporteMensual").HasKey(r => r.IdReporte);
            e.Property(r => r.MontoTotalOfertado).HasPrecision(18, 2);
            e.Property(r => r.MontoTotalComision).HasPrecision(18, 2);
            e.HasRequired(r => r.Proveedor).WithMany().HasForeignKey(r => r.IdProveedor);
        }

        private static void ConfigurarUsuario(DbModelBuilder mb)
        {
            var e = mb.Entity<Usuario>();
            e.ToTable("Usuario").HasKey(u => u.IdUsuario);
            e.Property(u => u.Nombres).IsRequired().HasMaxLength(100).IsUnicode(false);
            e.Property(u => u.PrimerApellido).IsRequired().HasMaxLength(100).IsUnicode(false);
            e.Property(u => u.SegundoApellido).IsRequired().HasMaxLength(100).IsUnicode(false);
            e.Property(u => u.Identificacion).IsRequired().HasMaxLength(10).IsUnicode(false)
                .HasColumnAnnotation(IndexAnnotation.AnnotationName, IndiceUnico("IX_Usuario_Identificacion"));
            e.Property(u => u.CorreoElectronico).IsRequired().HasMaxLength(200).IsUnicode(false);
            e.HasRequired(u => u.Proveedor).WithMany(p => p.Usuarios).HasForeignKey(u => u.IdProveedor);
        }

        private static void ConfigurarLicitacion(DbModelBuilder mb)
        {
            var e = mb.Entity<Licitacion>();
            e.ToTable("Licitacion").HasKey(l => l.IdLicitacion);
            e.Property(l => l.Titulo).IsRequired().HasMaxLength(150).IsUnicode(false)
                .HasColumnAnnotation(IndexAnnotation.AnnotationName, IndiceUnico("IX_Licitacion_Titulo"));
            e.Property(l => l.Descripcion).HasMaxLength(500).IsUnicode(false);
        }

        private static void ConfigurarOferta(DbModelBuilder mb)
        {
            var e = mb.Entity<Oferta>();
            e.ToTable("Oferta").HasKey(o => o.IdOferta);
            e.Property(o => o.Monto).HasPrecision(18, 2);
            e.Property(o => o.Descripcion).HasMaxLength(500).IsUnicode(false);
            e.HasRequired(o => o.Licitacion).WithMany(l => l.Ofertas).HasForeignKey(o => o.IdLicitacion);
            e.HasRequired(o => o.Proveedor).WithMany(p => p.Ofertas).HasForeignKey(o => o.IdProveedor);
        }

        private static void ConfigurarBitacora(DbModelBuilder mb)
        {
            var e = mb.Entity<BitacoraEvento>();
            e.ToTable("BITACORA_EVENTOS").HasKey(b => b.IdEvento);
            e.Property(b => b.TablaDeEvento).IsRequired().HasMaxLength(50).IsUnicode(false);
            e.Property(b => b.TipoDeEvento).IsRequired().HasMaxLength(20).IsUnicode(false);
            e.Property(b => b.DescripcionDeEvento).IsRequired().IsUnicode(false);
            e.Property(b => b.StackTrace).IsUnicode(false);
            e.Property(b => b.DatosAnteriores).IsUnicode(false);
            e.Property(b => b.DatosPosteriores).IsUnicode(false);
        }
    }
}

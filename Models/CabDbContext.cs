using Microsoft.EntityFrameworkCore;

namespace CrewRedDemo.Models;

public partial class CabDbContext : DbContext
{
    public CabDbContext()
    {
    }

    public CabDbContext(DbContextOptions<CabDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<SampleCabDatum> SampleCabData { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=crewred_test;Trusted_Connection=True;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SampleCabDatum>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SampleCa__3214EC07ABCBA6C7");

            entity.HasIndex(e => e.PuLocationId, "IX_SampleCabData_PULocationID");

            entity.HasIndex(e => e.TripDurationSeconds, "IX_SampleCabData_TravelTime").IsDescending();

            entity.HasIndex(e => e.TripDistance, "IX_SampleCabData_TripDistance").IsDescending();

            entity.Property(e => e.StoreAndFwdFlag)
                .HasMaxLength(3)
                .IsFixedLength();
            entity.Property(e => e.TripDurationSeconds).HasComputedColumnSql("(datediff(second,[TpepPickupDatetime],[TpepDropoffDatetime]))", false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

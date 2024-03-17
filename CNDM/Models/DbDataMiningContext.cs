using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace CNDM.Models;

public partial class DbDataMiningContext : DbContext
{
    public DbDataMiningContext()
    {
    }

    public DbDataMiningContext(DbContextOptions<DbDataMiningContext> options)
        : base(options)
    {
    }

    public virtual DbSet<HoaDon> HoaDons { get; set; }

    public virtual DbSet<SanPham> SanPhams { get; set; }

    public virtual DbSet<Support> Supports { get; set; }

    public virtual DbSet<TanSuatHaiSanPham> TanSuatHaiSanPhams { get; set; }

    public virtual DbSet<TanSuatMotSanPham> TanSuatMotSanPhams { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("name=connectString");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<HoaDon>(entity =>
        {
            entity.HasKey(e => e.IdHoaDon).HasName("PK__HoaDon__4DD461C83D0D4565");
        });

        modelBuilder.Entity<SanPham>(entity =>
        {
            entity.HasKey(e => e.IdSanPham).HasName("PK__SanPham__5FFA2D42B39AE1B4");
        });

        modelBuilder.Entity<Support>(entity =>
        {
            entity.HasKey(e => e.Smin).HasName("PK__Support__A47A33145EA00775");
        });

        modelBuilder.Entity<TanSuatHaiSanPham>(entity =>
        {
            entity.HasKey(e => e.ThuTu).HasName("PK_TanSuatHaiSanPham_ThuTu");

            entity.Property(e => e.ThuTu).ValueGeneratedNever();
        });

        modelBuilder.Entity<TanSuatMotSanPham>(entity =>
        {
            entity.HasKey(e => e.ThuTu).HasName("PK_TanSuatMotSanPham_ThuTu");

            entity.Property(e => e.ThuTu).ValueGeneratedNever();
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

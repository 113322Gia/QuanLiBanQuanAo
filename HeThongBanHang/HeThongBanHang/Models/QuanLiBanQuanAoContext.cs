using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace HeThongBanHang.Models;

public partial class QuanLiBanQuanAoContext : DbContext
{
    public QuanLiBanQuanAoContext()
    {
    }

    public QuanLiBanQuanAoContext(DbContextOptions<QuanLiBanQuanAoContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Cart> Carts { get; set; }

    public virtual DbSet<CartItem> CartItems { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<OrderItem> OrderItems { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductVariant> ProductVariants { get; set; }

    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<Payment> Payment { get; set; }
    public virtual DbSet<HistoryInventory> HistoryInventory { get; set; }
    public virtual DbSet<CustomerEmployee> CustomerEmployee { get; set; }
    public virtual DbSet<Favorite> Favorite { get; set; }
    public virtual DbSet<InventoryImport> InventoryImports { get; set; }
    public virtual DbSet<ObjectType> ObjectType { get; set; }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer("Data Source=LAPTOP-78FQNANG\\LOCAL;Initial Catalog=QuanLiBanQuanAo;Integrated Security=True;Connect Timeout=30;Encrypt=True;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cart>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Carts__3214EC0720E73055");

            entity.HasIndex(e => e.UserId, "UQ__Carts__1788CC4DA9D4DDB1").IsUnique();

            entity.HasOne(d => d.User).WithOne(p => p.Cart)
                .HasForeignKey<Cart>(d => d.UserId)
                .HasConstraintName("FK__Carts__UserId__300424B4");
        });

        modelBuilder.Entity<Favorite>()
    .HasOne(f => f.User)
    .WithMany()  // Mối quan hệ giữa User và Favorites
    .HasForeignKey(f => f.UserId)
    .OnDelete(DeleteBehavior.Cascade);  // Khi xóa User, Favorites sẽ bị xóa theo

        modelBuilder.Entity<Favorite>()
            .HasOne(f => f.Product)
            .WithMany()  // Mối quan hệ giữa Product và Favorites
            .HasForeignKey(f => f.ProductId)
            .OnDelete(DeleteBehavior.Cascade);  // Khi xóa Product, Favorites sẽ bị xóa theo


        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CartItem__3214EC076E442244");

            entity.HasOne(d => d.Cart).WithMany(p => p.CartItems)
                .HasForeignKey(d => d.CartId)
                .HasConstraintName("FK__CartItems__CartI__32E0915F");

            entity.HasOne(d => d.Product).WithMany(p => p.CartItems)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK__CartItems__Produ__33D4B598");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Categori__3214EC07075D2EA1");

            entity.Property(e => e.Name).HasMaxLength(100);
        });
        modelBuilder.Entity<CustomerEmployee>()
        .HasOne(ce => ce.Employee)
        .WithOne(e => e.CustomerEmployee)
        .HasForeignKey<Employee>(e => e.CustomerEmployeeId)
        .OnDelete(DeleteBehavior.Cascade);  // Khi xóa Employee, xóa luôn CustomerEmployee


        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Employee__3214EC0745A716B9");

            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.PasswordHash).HasMaxLength(200);
            entity.Property(e => e.Role)
                .HasMaxLength(50)
                .HasDefaultValue("Staff");
            entity.Property(e => e.Username).HasMaxLength(100);
            entity.HasMany(e => e.Orders)
       .WithOne(o => o.Employee)
       .HasForeignKey(o => o.EmployeeId)
       .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Orders__3214EC07D9748E7B");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TotalPrice).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Employee).WithMany(p => p.Orders)
                .HasForeignKey(d => d.EmployeeId)
                .HasConstraintName("FK__Orders__Employee__38996AB5");

            entity.HasOne(d => d.User).WithMany(p => p.Orders)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Orders__UserId__37A5467C");
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__OrderIte__3214EC0737D9867A");

            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("FK__OrderItem__Order__3B75D760");

            entity.HasOne(d => d.Product).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK__OrderItem__Produ__3C69FB99");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Products__3214EC079EA80D52");

            entity.Property(e => e.ImageUrl).HasMaxLength(250);
            entity.Property(e => e.Name).HasMaxLength(100);

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.Restrict) // Không xóa Category khi xóa Product
                .HasConstraintName("FK__Products__Catego__2C3393D0");
        });
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.HasOne(e => e.ObjectType)
                .WithMany(ot => ot.Categories)
                .HasForeignKey(e => e.ObjectTypeId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);  // Cho phép ObjectTypeId có thể NULL
        });



        modelBuilder.Entity<ProductVariant>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ProductV__3214EC074471381A");

            entity.ToTable("ProductVariant");

            entity.Property(e => e.Size).HasMaxLength(10);

            entity.HasOne(d => d.Product).WithMany(p => p.ProductVariants)
                .HasForeignKey(d => d.ProductId)
                //.IsRequired()
                .OnDelete(DeleteBehavior.Cascade) // Xóa biến thể khi xóa sản phẩm
                .HasConstraintName("FK_ProductVariant_Product");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Users__3214EC07662D925A");

            entity.Property(e => e.Role)
                .HasMaxLength(50)
                .HasDefaultValue("User");
            entity.Property(e => e.Username).HasMaxLength(100);
        });
        // Quan hệ InventoryImport → ProductVariant (nhiều-nhiều)
        modelBuilder.Entity<InventoryImport>()
            .HasOne(ii => ii.ProductVariant)
            .WithMany()
            .HasForeignKey(ii => ii.ProductVariantId)
            .OnDelete(DeleteBehavior.Restrict); // hoặc Cascade tùy ý

        // Quan hệ InventoryImport → Employee (nhiều-nhiều)
        modelBuilder.Entity<InventoryImport>()
            .HasOne(ii => ii.Employee)
            .WithMany()
            .HasForeignKey(ii => ii.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict); // hoặc Cascade tùy ý


        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using webapp_demo.Models;

namespace webapp_demo.Data;

public class ApplicationDbContext : IdentityDbContext<AppUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Property> Properties => Set<Property>();
    public DbSet<PropertyType> PropertyTypes => Set<PropertyType>();
    public DbSet<PropertyImage> PropertyImages => Set<PropertyImage>();
    public DbSet<Favorite> Favorites => Set<Favorite>();
    public DbSet<ContactRequest> ContactRequests => Set<ContactRequest>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Property>().HasIndex(p => p.Status);
        builder.Entity<Property>().HasIndex(p => p.District);

        builder.Entity<Property>()
            .HasOne(p => p.PropertyType).WithMany(t => t.Properties)
            .HasForeignKey(p => p.PropertyTypeId).OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Property>()
            .HasOne(p => p.Owner).WithMany()
            .HasForeignKey(p => p.OwnerId).OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Property>()
            .HasCheckConstraint("CK_Property_Price_Positive", "[Price] > 0");
        builder.Entity<Property>()
            .HasCheckConstraint("CK_Property_Area_Positive", "[Area] > 0");

        builder.Entity<PropertyImage>()
            .HasOne(i => i.Property).WithMany(p => p.Images)
            .HasForeignKey(i => i.PropertyId).OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Favorite>()
            .HasIndex(f => new { f.UserId, f.PropertyId }).IsUnique();
        builder.Entity<Favorite>()
            .HasOne(f => f.Property).WithMany()
            .HasForeignKey(f => f.PropertyId).OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ContactRequest>()
            .HasOne(c => c.Property).WithMany()
            .HasForeignKey(c => c.PropertyId).OnDelete(DeleteBehavior.Cascade);
    }
}

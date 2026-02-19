using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<Mensagem> Mensagens { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Mensagem>()
            .HasOne(m => m.DeUser)
            .WithMany()
            .HasForeignKey(m => m.DeUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Mensagem>()
            .HasOne(m => m.ParaUser)
            .WithMany()
            .HasForeignKey(m => m.ParaUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Reswifty.API.Application.Abstractions.Persistence;
using Reswifty.API.Domain.Companies;
using Reswifty.API.Domain.Identity;

namespace Reswifty.API.Infrastructure.Persistence;

/// <remarks>
/// Add migrations using the following command inside the 'Reswifty.Infrastructure' project directory:
///
/// dotnet ef migrations add --startup-project Reswifty.API --context AppDbContext [migration-name] --output-dir Infrastructure/Migrations
/// </remarks>
public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<User, UserRole, int>(options), IAppDbContext
{
    public DbSet<Company> Companies => Set<Company>();

    public DbSet<UserInvite> UserInvites => Set<UserInvite>();

    public DbSet<AuthRefreshToken> RefreshTokens => Set<AuthRefreshToken>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(builder);

        builder.Entity<User>().ToTable(DatabaseConst.Tables.User, DatabaseConst.Schemas.User);
        builder.Entity<UserRole>().ToTable(DatabaseConst.Tables.Role, DatabaseConst.Schemas.User);
        builder.Entity<UserRoleClaim>().ToTable(DatabaseConst.Tables.UserRole, DatabaseConst.Schemas.User);
        builder.Entity<IdentityUserRole<int>>().ToTable(DatabaseConst.Tables.RoleClaim, DatabaseConst.Schemas.User);
        builder.Entity<IdentityUserClaim<int>>().ToTable(DatabaseConst.Tables.UserClaim, DatabaseConst.Schemas.User);
        builder.Entity<IdentityUserLogin<int>>().ToTable(DatabaseConst.Tables.UserLogin, DatabaseConst.Schemas.User);
        builder.Entity<IdentityUserToken<int>>().ToTable(DatabaseConst.Tables.UserToken, DatabaseConst.Schemas.User);

        builder.Entity<UserInvite>(b =>
        {
            b.ToTable(DatabaseConst.Tables.UserInvite);
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).ValueGeneratedOnAdd();

            b.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<AuthRefreshToken>(b =>
        {
            b.ToTable(DatabaseConst.Tables.RefreshToken);
            b.HasKey(x => x.Id);
            b.HasIndex(x => x.Hash).IsUnique();
            b.Property(x => x.Hash).IsRequired();
            b.Property(x => x.Salt).IsRequired();

            b.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    Task<int> IAppDbContext.SaveChangesAsync(CancellationToken ct) => base.SaveChangesAsync(ct);
}

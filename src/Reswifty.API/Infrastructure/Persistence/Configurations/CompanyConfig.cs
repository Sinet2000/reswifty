using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Reswifty.API.Domain.Companies;

namespace Reswifty.API.Infrastructure.Persistence.Configurations;

public sealed class CompanyConfig : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> b)
    {
        b.ToTable("companies");
        b.HasKey(x => x.Id);
        b.Property(x => x.Type).HasConversion<int>().IsRequired();

        b.Property(x => x.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        b.HasIndex(x => x.Name);
    }
}

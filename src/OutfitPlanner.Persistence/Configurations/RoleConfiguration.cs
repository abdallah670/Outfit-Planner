using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OutfitPlanner.Persistence.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<IdentityRole>
{
    public void Configure(EntityTypeBuilder<IdentityRole> builder)
    {
        builder.HasData(
            new IdentityRole
            {
                Id = "5765715a-93be-4628-86f7-b12e35a1a1f1",
                Name = "Admin",
                NormalizedName = "ADMIN",
                ConcurrencyStamp = "ece01a6a-4caf-4a95-a704-9f03712e7fbb"
            },
            new IdentityRole
            {
                Id = "76208571-0083-4a8b-9149-8d769c0d9c02",
                Name = "Planner",
                NormalizedName = "PLANNER",
                ConcurrencyStamp = "bd9a512a-b188-467d-9fd9-875f09673ac3"
            }
        );
    }
}

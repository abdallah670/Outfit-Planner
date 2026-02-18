using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Persistence.Configurations;

public class ValidationPollConfiguration : IEntityTypeConfiguration<ValidationPoll>
{
    public void Configure(EntityTypeBuilder<ValidationPoll> builder)
    {
        builder.Property(p => p.Question)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(p => p.Status)
            .HasConversion<string>();

        builder.HasMany(p => p.Options)
            .WithOne(o => o.Poll)
            .HasForeignKey(o => o.PollId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Votes)
            .WithOne()
            .HasForeignKey(v => v.PollId)
            .OnDelete(DeleteBehavior.NoAction); // Avoid multiple cascade paths
    }
}

public class PollOptionConfiguration : IEntityTypeConfiguration<PollOption>
{
    public void Configure(EntityTypeBuilder<PollOption> builder)
    {
        builder.Property(o => o.Description)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasMany(o => o.Votes)
            .WithOne(v => v.Option)
            .HasForeignKey(v => v.OptionId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne(o => o.Outfit)
            .WithMany(outfit => outfit.PollOptions)
            .HasForeignKey(o => o.OutfitId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}

public class VoteConfiguration : IEntityTypeConfiguration<Vote>
{
    public void Configure(EntityTypeBuilder<Vote> builder)
    {
        builder.Property(v => v.Comment)
            .HasMaxLength(1000);

        builder.HasOne(v => v.Voter)
            .WithMany()
            .HasForeignKey(v => v.VoterId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}

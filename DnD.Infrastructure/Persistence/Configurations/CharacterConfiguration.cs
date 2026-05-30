using DnD.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DnD.Infrastructure.Persistence.Configurations;

public class CharcterConfiguration : IEntityTypeConfiguration<Character>
{
    public void Configure(EntityTypeBuilder<Character> builder)
    {
        //  1. Primmary key to field Id
        builder.HasKey(c => c.Id);

        builder
            .HasOne(c => c.User)
            .WithMany(u => u.Characters)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // 2.
        builder.Property(c => c.Name).IsRequired().HasMaxLength(128);
        builder.Property(c => c.ImageUrl).HasMaxLength(512);
        builder.Property(c => c.Race).HasMaxLength(64);
        builder.Property(c => c.Class).HasMaxLength(64);
        builder.Property(c => c.Background).HasMaxLength(64);
        builder.Property(c => c.Alignment).HasMaxLength(64);

        builder.Property(c => c.CreatedAt).IsRequired();
        builder.Property(c => c.LastModifiedAt).IsRequired();

        //3. Set Enum for skills
        // save as INT in DB

        builder.Property(c => c.Athletics).HasConversion<int>();
        builder.Property(c => c.Acrobatics).HasConversion<int>();
        builder.Property(c => c.SleightOfHand).HasConversion<int>();
        builder.Property(c => c.Stealth).HasConversion<int>();
        builder.Property(c => c.Arcana).HasConversion<int>();
        builder.Property(c => c.History).HasConversion<int>();
        builder.Property(c => c.Investigation).HasConversion<int>();
        builder.Property(c => c.Nature).HasConversion<int>();
        builder.Property(c => c.Religion).HasConversion<int>();
        builder.Property(c => c.AnimalHandling).HasConversion<int>();
        builder.Property(c => c.Insight).HasConversion<int>();
        builder.Property(c => c.Medicine).HasConversion<int>();
        builder.Property(c => c.Perception).HasConversion<int>();
        builder.Property(c => c.Survival).HasConversion<int>();
        builder.Property(c => c.Deception).HasConversion<int>();
        builder.Property(c => c.Intimidation).HasConversion<int>();
        builder.Property(c => c.Performance).HasConversion<int>();
        builder.Property(c => c.Persuasion).HasConversion<int>();

        // 4. Arrays ()

        builder.Property(c => c.Inventory).HasColumnType("text[]");
        builder.Property(c => c.Spells).HasColumnType("text[]");
        builder.Property(c => c.Feats).HasColumnType("text[]");
        builder.Property(c => c.ClassFeatures).HasColumnType("text[]");
        builder.Property(c => c.RacialTraits).HasColumnType("text[]");
    }
}

using System.Reflection;
using DnD.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DnD.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Character> Characters => Set<Character>();

    // ДОДАНО: Таблиця для трекерів
    public DbSet<ResourceTracker> ResourceTrackers => Set<ResourceTracker>();
    public DbSet<Spell> Spells => Set<Spell>();
    public DbSet<SpellSlot> SpellSlots => Set<SpellSlot>();
    public DbSet<AttackAction> AttackActions => Set<AttackAction>();
    public DbSet<AttackDamage> AttackDamages => Set<AttackDamage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // --- Campaign to GM (User) relationship ---
        modelBuilder.Entity<Campaign>()
            .HasOne(c => c.GM)
            .WithMany() // A user can be a GM of many campaigns
            .HasForeignKey(c => c.GmUserId)
            .OnDelete(DeleteBehavior.Restrict); // Protects a User from being deleted if they are a GM

        // --- Campaign Member (Композитний ключ) ---
        modelBuilder.Entity<CampaignMember>()
            .HasKey(cm => new { cm.CampaignId, cm.UserId });

        modelBuilder.Entity<CampaignMember>()
            .HasOne(cm => cm.User)
            .WithMany()
            .HasForeignKey(cm => cm.UserId)
            .OnDelete(DeleteBehavior.Cascade); // Видалили юзера - видалився запис в кампанії

        modelBuilder.Entity<CampaignMember>()
            .HasOne(cm => cm.Campaign)
            .WithMany(c => c.Members)
            .HasForeignKey(cm => cm.CampaignId)
            .OnDelete(DeleteBehavior.Cascade); // Видалили кампанію - видалились учасники

        // --- Campaign Character (Композитний ключ) ---
        modelBuilder.Entity<CampaignCharacter>()
            .HasKey(cc => new { cc.CampaignId, cc.CharacterId });

        modelBuilder.Entity<CampaignCharacter>()
            .HasOne(cc => cc.Character)
            .WithMany()
            .HasForeignKey(cc => cc.CharacterId)
            .OnDelete(DeleteBehavior.Restrict); // ЖОРСТКИЙ ЗАХИСТ: не можна видалити кампанію і випадково стерти лист персонажа гравця!

        modelBuilder.Entity<CampaignCharacter>()
            .HasOne(cc => cc.Campaign)
            .WithMany(c => c.Characters)
            .HasForeignKey(cc => cc.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);

        // --- Character to SpellSlot (1-to-Many, Cascade) ---
        modelBuilder.Entity<Character>()
            .HasMany(c => c.SpellSlots)
            .WithOne(s => s.Character)
            .HasForeignKey(s => s.CharacterId)
            .OnDelete(DeleteBehavior.Cascade);

        // --- AttackAction to AttackDamage (1-to-Many, Cascade) ---
        modelBuilder.Entity<AttackAction>()
            .HasMany(a => a.Damages)
            .WithOne(d => d.AttackAction)
            .HasForeignKey(d => d.AttackActionId)
            .OnDelete(DeleteBehavior.Cascade);

        // --- Spell to AttackAction (1-to-Many, SetNull) ---
        // CRITICAL: A character deleting their action should NEVER delete the dictionary Spell!
        modelBuilder.Entity<AttackAction>()
            .HasOne(a => a.Spell)
            .WithMany()
            .HasForeignKey(a => a.SpellId)
            .OnDelete(DeleteBehavior.SetNull);

        var seedTime = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        modelBuilder.Entity<Spell>().HasData(
            new Spell
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Name = "Fireball",
                Level = 3,
                RequiresSave = true,
                SaveStat = DnD.Domain.Enums.StatType.Dexterity,
                DamageDice = "8d6",
                DamageType = "Fire",
                Description = "A bright streak flashes from your pointing finger to a point you choose within range then blossoms with a low roar into an explosion of flame.",
                BuffDebuffNotes = "",
                CreatedAt = seedTime,
                LastModifiedAt = seedTime
            },
            new Spell
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Name = "Mage Hand",
                Level = 0,
                RequiresSave = false,
                SaveStat = null,
                DamageDice = "",
                DamageType = "",
                Description = "A spectral, floating hand appears at a point you choose within range. The hand lasts for the duration or until you dismiss it as an action.",
                BuffDebuffNotes = "",
                CreatedAt = seedTime,
                LastModifiedAt = seedTime
            },
            new Spell
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Name = "Ray of Frost",
                Level = 0,
                RequiresSave = false,
                SaveStat = null,
                DamageDice = "1d8",
                DamageType = "Cold",
                Description = "A frigid beam of blue-white light streaks toward a creature within range. Make a ranged spell attack against the target. On a hit, it takes 1d8 cold damage, and its speed is reduced by 10 feet until the start of your next turn.",
                BuffDebuffNotes = "Speed reduced by 10ft",
                CreatedAt = seedTime,
                LastModifiedAt = seedTime
            }
        );

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    public DbSet<Campaign> Campaigns { get; set; }
    public DbSet<CampaignMember> CampaignMembers { get; set; }
    public DbSet<CampaignFolder> CampaignFolders { get; set; }
    public DbSet<CampaignCharacter> CampaignCharacters { get; set; }
    public DbSet<Encounter> Encounters { get; set; }
    public DbSet<EncounterParticipant> EncounterParticipants { get; set; }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker
            .Entries()
            .Where(e =>
                // ЗМІНЕНО: Тепер ми ловимо всі сутності, що наслідують BaseEntity
                e.Entity is BaseEntity
                && (e.State == EntityState.Added || e.State == EntityState.Modified)
            );

        foreach (var entityEntry in entries)
        {
            // ЗМІНЕНО: Кастуємо до BaseEntity, а не до Character
            var entity = (BaseEntity)entityEntry.Entity;
            entity.LastModifiedAt = DateTime.UtcNow;

            if (entityEntry.State == EntityState.Added)
            {
                entity.CreatedAt = DateTime.UtcNow;
            }
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}

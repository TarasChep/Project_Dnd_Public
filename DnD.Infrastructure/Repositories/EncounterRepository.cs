using DnD.Domain.Entities;
using DnD.Domain.Interfaces;
using DnD.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DnD.Infrastructure.Repositories;

public class EncounterRepository : IEncounterRepository
{
    private readonly ApplicationDbContext _dbContext;

    public EncounterRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<Encounter>> GetAllAsync()
    {
        return await _dbContext.Encounters
            .Include(e => e.Participants) // Підтягуємо, щоб порахувати кількість
            .ToListAsync();
    }

    public async Task<Encounter?> GetByIdWithDetailsAsync(Guid id)
    {
        return await _dbContext.Encounters
            .Include(e => e.Participants)
                .ThenInclude(p => p.Character)
                    .ThenInclude(c => c!.Attacks)
                        .ThenInclude(a => a.Damages)
            .AsSplitQuery()
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<Encounter?> GetByIdWithParticipantsAsync(Guid id)
    {
        return await _dbContext.Encounters
            .Include(e => e.Participants)
                .ThenInclude(p => p.Character)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<EncounterParticipant?> GetParticipantByIdAsync(Guid id)
    {
        return await _dbContext.EncounterParticipants.FindAsync(id);
    }

    public async Task AddAsync(Encounter encounter)
    {
        await _dbContext.Encounters.AddAsync(encounter);
    }

    public async Task AddParticipantAsync(EncounterParticipant participant)
    {
        await _dbContext.AddAsync(participant);
    }

    public async Task AddParticipantsAsync(IEnumerable<EncounterParticipant> participants)
    {
        await _dbContext.AddRangeAsync(participants);
    }

    public async Task<int> GetParticipantCountAsync(Guid encounterId, Guid characterId)
    {
        return await _dbContext.EncounterParticipants
            .CountAsync(p => p.EncounterId == encounterId && p.CharacterId == characterId);
    }

    public void Update(Encounter encounter)
    {
        _dbContext.Encounters.Update(encounter);
    }

    public void Update(EncounterParticipant participant)
    {
        _dbContext.EncounterParticipants.Update(participant);
    }

    public void RemoveParticipant(EncounterParticipant participant)
    {
        _dbContext.Remove(participant);
    }

    public async Task SaveChangesAsync()
    {
        await _dbContext.SaveChangesAsync();
    }

    public void Remove(Encounter encounter)
    {
        _dbContext.Encounters.Remove(encounter);
    }
}
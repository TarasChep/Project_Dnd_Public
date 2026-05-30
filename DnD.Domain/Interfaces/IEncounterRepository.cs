using DnD.Domain.Entities;

namespace DnD.Domain.Interfaces;

public interface IEncounterRepository
{
    Task<IEnumerable<Encounter>> GetAllAsync();
    Task<Encounter?> GetByIdWithDetailsAsync(Guid id);
    Task<Encounter?> GetByIdWithParticipantsAsync(Guid id);
    Task<EncounterParticipant?> GetParticipantByIdAsync(Guid id);
    Task AddAsync(Encounter encounter);
    Task AddParticipantAsync(EncounterParticipant participant);
    Task AddParticipantsAsync(IEnumerable<EncounterParticipant> participants);
    Task<int> GetParticipantCountAsync(Guid encounterId, Guid characterId);
    void Update(Encounter encounter);
    void Update(EncounterParticipant participant);
    void RemoveParticipant(EncounterParticipant participant);
    Task SaveChangesAsync();
    void Remove(Encounter encounter);
}

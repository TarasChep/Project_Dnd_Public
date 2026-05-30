using DnD.Application.DTOs;

namespace DnD.Application.Interfaces;

public interface IEncounterAnalyzerService
{
    Task<IEnumerable<EncounterBriefDto>> GetAllEncountersAsync();
    Task<EncounterDetailDto?> GetEncounterByIdAsync(Guid id, Guid currentUserId, bool isSystemAdmin);
    Task<EncounterAnalysisDto> AnalyzeEncounterAsync(Guid encounterId);
    Task<Guid> CreateEncounterAsync(CreateEncounterDto dto);
    Task<bool> AddParticipantAsync(Guid encounterId, AddParticipantDto dto);
    Task<bool> RemoveParticipantAsync(Guid encounterId, Guid participantId);
    Task<bool> UpdateParticipantAsync(Guid encounterId, Guid participantId, UpdateParticipantDto dto);
}
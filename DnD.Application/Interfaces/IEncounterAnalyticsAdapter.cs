using DnD.Application.DTOs;

namespace DnD.Application.Interfaces;

public interface IEncounterAnalyticsAdapter
{
    Task<EncounterAnalyticsDto> GetAnalyticsPayloadAsync(Guid encounterId);
}
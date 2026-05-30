using DnD.Domain.Entities;

namespace DnD.Domain.Interfaces;

public interface ICharacterRepository
{
    Task<IEnumerable<Character>> GetByUserIdAsync(Guid userId);
    Task<Character?> GetByIdAsync(Guid id);

    // ОЦЕЙ РЯДОК - НАЙГОЛОВНІШИЙ ЗАРАЗ. Це метод, який тягне і трекери, і атаки.
    Task<Character?> GetByIdWithDetailsAsync(Guid id);

    Task AddAsync(Character character);
    void Update(Character character);
    void Delete(Character character);

    // ПЕРЕВІР, ЩОБ ЦЕЙ РЯДОК БУВ ТУТ! Компілятор скаржиться на його відсутність.
    Task SaveChangesAsync();
}

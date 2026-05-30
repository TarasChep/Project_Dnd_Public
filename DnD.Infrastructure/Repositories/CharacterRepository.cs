using DnD.Domain.Entities;
using DnD.Domain.Interfaces;
using DnD.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DnD.Infrastructure.Repositories;

public class CharacterRepository : ICharacterRepository
{
    private readonly ApplicationDbContext _context;

    public CharacterRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Character>> GetByUserIdAsync(Guid userId)
    {
        return await _context
            .Characters.Where(c => c.UserId == userId)
            .OrderByDescending(c => c.LastModifiedAt)
            .ToListAsync();
    }

    public async Task<Character?> GetByIdAsync(Guid id)
    {
        return await _context.Characters.FindAsync(id);
    }

    public async Task AddAsync(Character character)
    {
        await _context.Characters.AddAsync(character);
    }

    public void Update(Character character)
    {
        _context.Characters.Update(character);
    }

    public void Delete(Character character)
    {
        _context.Characters.Remove(character);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<Character?> GetByIdWithDetailsAsync(Guid id)
    {
        return await _context
            .Characters
            .Include(c => c.ResourceTrackers)
            .Include(c => c.Attacks)
                .ThenInclude(a => a.Damages)
            .Include(c => c.Attacks)
                .ThenInclude(a => a.Spell)
            .Include(c => c.SpellSlots)
            .AsSplitQuery()
            .FirstOrDefaultAsync(c => c.Id == id);
    }
}

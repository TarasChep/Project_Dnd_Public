using DnD.Application.DTOs;
using DnD.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DnD.Api.Controllers;

[Authorize]
[Route("api/[controller]")]
public class SpellsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public SpellsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<SpellDto>>> GetAllSpells()
    {
        var spells = await _context.Spells.OrderBy(s => s.Level).ThenBy(s => s.Name).ToListAsync();
        var dtos = spells.Select(s => new SpellDto
        {
            Id = s.Id,
            Name = s.Name,
            Level = s.Level,
            RequiresSave = s.RequiresSave,
            SaveStat = s.SaveStat?.ToString(),
            DamageDice = s.DamageDice,
            DamageType = s.DamageType,
            Description = s.Description,
            BuffDebuffNotes = s.BuffDebuffNotes
        }).ToList();

        return Ok(dtos);
    }
}
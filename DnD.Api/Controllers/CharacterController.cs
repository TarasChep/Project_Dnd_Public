using DnD.Application.DTOs;
using DnD.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DnD.Api.Controllers;

[Authorize]
[Route("api/characters")]
public class CharacterController : BaseApiController
{
    private readonly ICharacterService _characterService;

    public CharacterController(ICharacterService characterService)
    {
        _characterService = characterService;
    }

    /// <summary>
    /// Отримати список персонажів поточного користувача.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CharacterBriefDto>>> GetMyCharacters()
    {
        var userId = GetCurrentUserId();
        var characters = await _characterService.GetMyCharactersAsync(userId, IsAdmin);
        return Ok(characters);
    }

    /// <summary>
    /// Отримати повну анкету персонажа за ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<CharacterFullDto>> GetById(Guid id)
    {
        var userId = GetCurrentUserId();
        var character = await _characterService.GetByIdAsync(id, userId, IsAdmin);

        if (character == null)
        {
            return NotFound(new { message = "Character not found or access denied." });
        }

        return Ok(character);
    }

    /// <summary>
    /// Створити нового персонажа.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<CharacterFullDto>> Create([FromBody] CharacterCreateDto dto)
    {
        var userId = GetCurrentUserId();
        var result = await _characterService.CreateAsync(dto, userId);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Видалити персонажа.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = GetCurrentUserId();
        var success = await _characterService.DeleteAsync(id, userId, IsAdmin);

        if (!success)
        {
            return NotFound(
                new { message = "Failed to delete: character not found or insufficient permissions." }
            );
        }

        return NoContent();
    }

    /// <summary>
    /// Універсальний генератор кидків кубиків (Атаки, Скіли, Кастом).
    /// </summary>
    [HttpPost("{id}/roll")]
    public async Task<ActionResult<RollResponseDto>> PerformRoll(
        Guid id,
        [FromBody] UniversalRollRequestDto dto
    )
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _characterService.PerformUniversalRollAsync(
                id,
                userId,
                IsAdmin,
                dto
            );
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Витратити кістки здоров'я (Hit Dice) для лікування.
    /// </summary>
    [HttpPost("{id}/hitdice/spend")]
    public async Task<ActionResult<HitDiceResultDto>> SpendHitDice(
        Guid id,
        [FromBody] SpendHitDiceDto dto
    )
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _characterService.SpendHitDiceAsync(id, userId, IsAdmin, dto);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Оновити прогресію персонажа (Level, Class, etc).
    /// </summary>
    [HttpPut("{id}/progression")]
    public async Task<ActionResult<CharacterFullDto>> UpdateProgression(
        Guid id,
        [FromBody] UpdateProgressionDto dto
    )
    {
        try
        {
            var result = await _characterService.UpdateProgressionAsync(
                id,
                GetCurrentUserId(),
                IsAdmin,
                dto
            );
            return Ok(result);
        }
        catch (Exception ex) when (ex.Message == "Not Found")
        {
            return NotFound(new { message = "Character not found." });
        }
    }

    /// <summary>
    /// Швидке коригування HP (Damage/Heal).
    /// </summary>
    [HttpPatch("{id}/health")]
    public async Task<ActionResult<CharacterFullDto>> UpdateHealth(
        Guid id,
        [FromBody] UpdateValueDto dto
    )
    {
        try
        {
            var result = await _characterService.UpdateHealthAsync(
                id,
                GetCurrentUserId(),
                IsAdmin,
                dto
            );
            return Ok(result);
        }
        catch (Exception ex) when (ex.Message == "Not Found")
        {
            return NotFound(new { message = "Character not found." });
        }
    }

    /// <summary>
    /// Оновити макс. HP, тимчасові HP або додати досвід.
    /// </summary>
    [HttpPatch("{id}/vitals")]
    public async Task<ActionResult<CharacterFullDto>> UpdateVitals(
        Guid id,
        [FromBody] UpdateVitalsDto dto
    )
    {
        try
        {
            var result = await _characterService.UpdateVitalsAsync(
                id,
                GetCurrentUserId(),
                IsAdmin,
                dto
            );
            return Ok(result);
        }
        catch (Exception ex) when (ex.Message == "Not Found")
        {
            return NotFound(new { message = "Character not found." });
        }
    }

    /// <summary>
    /// Оновити гаманець за Мідним Стандартом (Підтримує прибутки та витрати).
    /// </summary>
    [HttpPatch("{id}/wallet")]
    public async Task<ActionResult<CharacterFullDto>> UpdateWallet(
        Guid id,
        [FromBody] UpdateWalletDto dto
    )
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _characterService.UpdateWalletAsync(id, userId, IsAdmin, dto);
            return Ok(result);
        }
        catch (Exception ex) when (ex.Message == "Insufficient funds")
        {
            return BadRequest(new { message = "Insufficient funds for this transaction." });
        }
        catch (Exception ex) when (ex.Message == "Not Found")
        {
            return NotFound(new { message = "Character not found." });
        }
    }

    /// <summary>
    /// Виконати короткий або довгий відпочинок.
    /// </summary>
    [HttpPost("{id}/rest")]
    public async Task<ActionResult<CharacterFullDto>> PerformRest(
        Guid id,
        [FromBody] PerformRestDto dto
    )
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _characterService.PerformRestAsync(id, userId, IsAdmin, dto);
            return Ok(result);
        }
        catch (Exception ex) when (ex.Message == "Not Found")
        {
            return NotFound(new { message = "Character not found." });
        }
    }

    /// <summary>
    /// Створити новий трекер ресурсу (Стріли, Одиниці люті тощо).
    /// </summary>
    [HttpPost("{id}/trackers")]
    public async Task<ActionResult<CharacterFullDto>> AddTracker(
        Guid id,
        [FromBody] CreateTrackerDto dto
    )
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _characterService.AddTrackerAsync(id, userId, IsAdmin, dto);
            return Ok(result);
        }
        catch (Exception ex) when (ex.Message == "Not Found")
        {
            return NotFound(new { message = "Character not found." });
        }
    }

    /// <summary>
    /// Додає нову зброю або атаку в арсенал персонажа
    /// </summary>
    [HttpPost("{id:guid}/attacks")]
    public async Task<IActionResult> AddAttack(Guid id, [FromBody] CreateAttackDto dto)
    {
        try
        {
            // Використовуємо твої реальні методи з BaseApiController
            var userId = GetCurrentUserId();

            // Зверни увагу: IsAdmin передається без дужок
            var updatedCharacter = await _characterService.AddAttackAsync(id, userId, IsAdmin, dto);

            return Ok(updatedCharacter);
        }
        catch (Exception ex)
        {
            if (ex.Message == "Not Found")
                return NotFound(new { error = "Character not found or access denied." });
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Кидок на попадання (To-Hit) для конкретної атаки
    /// </summary>
    [HttpPost("{id:guid}/attacks/{attackId:guid}/roll-hit")]
    public async Task<IActionResult> RollAttackHit(Guid id, Guid attackId)
    {
        try
        {
            // ТИМЧАСОВА ЗАГЛУШКА (Поки ти не налаштував авторизацію)
            Guid userId = Guid.Empty; // Заміни на GetCurrentUserId()
            bool isAdmin = true;

            var result = await _characterService.RollAttackHitAsync(id, attackId, userId, isAdmin);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Кидок шкоди (Damage) для конкретної атаки
    /// </summary>
    [HttpPost("{id:guid}/attacks/{attackId:guid}/roll-damage")]
    public async Task<IActionResult> RollAttackDamage(
        Guid id,
        Guid attackId,
        [FromQuery] bool isCritical = false
    )
    {
        try
        {
            // ТИМЧАСОВА ЗАГЛУШКА
            Guid userId = Guid.Empty; // Заміни на GetCurrentUserId()
            bool isAdmin = true;

            var result = await _characterService.RollAttackDamageAsync(
                id,
                attackId,
                userId,
                isAdmin,
                isCritical
            );
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Оновити існуючу атаку та її параметри шкоди.
    /// </summary>
    [HttpPut("{id:guid}/attacks/{attackId:guid}")]
    public async Task<IActionResult> UpdateAttack(
        Guid id,
        Guid attackId,
        [FromBody] CreateAttackDto dto
    )
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _characterService.UpdateAttackAsync(
                id,
                attackId,
                userId,
                IsAdmin,
                dto
            );
            return Ok(result);
        }
        catch (Exception ex)
        {
            if (ex.Message == "Not Found")
                return NotFound(new { error = "Character not found or access denied." });
            if (ex.Message == "Attack Not Found")
                return NotFound(new { error = "Attack not found." });
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPatch("{id:guid}/trackers/{trackerId:guid}")]
    public async Task<IActionResult> UpdateTracker(
        Guid id,
        Guid trackerId,
        [FromBody] UpdateTrackerDto dto
    )
    {
        try
        {
            var result = await _characterService.UpdateTrackerAsync(
                id,
                trackerId,
                GetCurrentUserId(),
                IsAdmin,
                dto
            );
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // [DELETE] Видалити трекер
    [HttpDelete("{id:guid}/trackers/{trackerId:guid}")]
    public async Task<IActionResult> DeleteTracker(Guid id, Guid trackerId)
    {
        try
        {
            var result = await _characterService.DeleteTrackerAsync(
                id,
                trackerId,
                GetCurrentUserId(),
                IsAdmin
            );
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // [DELETE] Видалити атаку
    [HttpDelete("{id:guid}/attacks/{attackId:guid}")]
    public async Task<IActionResult> DeleteAttack(Guid id, Guid attackId)
    {
        try
        {
            var result = await _characterService.DeleteAttackAsync(
                id,
                attackId,
                GetCurrentUserId(),
                IsAdmin
            );
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{id:guid}/spellslots")]
    public async Task<IActionResult> AddSpellSlot(Guid id, [FromBody] CreateSpellSlotDto dto)
    {
        try
        {
            var service = (DnD.Application.Services.CharacterService)_characterService;
            var result = await service.AddSpellSlotAsync(id, GetCurrentUserId(), IsAdmin, dto);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPatch("{id:guid}/spellslots/{slotId:guid}")]
    public async Task<IActionResult> UpdateSpellSlot(Guid id, Guid slotId, [FromBody] UpdateSpellSlotDto dto)
    {
        try
        {
            var service = (DnD.Application.Services.CharacterService)_characterService;
            var result = await service.UpdateSpellSlotAsync(id, slotId, GetCurrentUserId(), IsAdmin, dto);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{id:guid}/spellslots/{slotId:guid}")]
    public async Task<IActionResult> DeleteSpellSlot(Guid id, Guid slotId)
    {
        try
        {
            var service = (DnD.Application.Services.CharacterService)_characterService;
            var result = await service.DeleteSpellSlotAsync(id, slotId, GetCurrentUserId(), IsAdmin);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Оновити URL зображення персонажа.
    /// </summary>
    [HttpPatch("{id}/image")]
    public async Task<ActionResult> UpdateCharacterImage(
        Guid id,
        [FromBody] UpdateCharacterImageDto dto
    )
    {
        try
        {
            var userId = GetCurrentUserId();
            var success = await _characterService.UpdateImageUrlAsync(id, userId, dto.ImageUrl);

            if (!success)
            {
                return NotFound(
                    new { message = "Character not found or access denied." }
                );
            }

            return Ok(new { message = "Character image updated successfully." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPatch("{id}/integrations")]
    // [Authorize] // Розкоментуй, якщо в тебе вже налаштований JWT
    public async Task<ActionResult<CharacterFullDto>> UpdateIntegrations(
        Guid id,
        [FromBody] UpdateIntegrationsDto dto
    )
    {
        try
        {
            // У тебе може бути інший спосіб отримання UserId з токена. Використовуй свій, якщо цей не підходить.
            // Guid userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            // bool isAdmin = User.IsInRole("Admin");

            // ТИМЧАСОВА ЗАГЛУШКА (Поки ти не налаштував авторизацію повністю):
            Guid userId = Guid.Empty; // Заміни на реальне отримання з User
            bool isAdmin = true; // Заміни на реальну перевірку ролі

            var result = await _characterService.UpdateIntegrationsAsync(id, userId, isAdmin, dto);
            return Ok(result);
        }
        catch (Exception ex)
        {
            // У реальному житті тут не можна повертати текст ексепшена напряму, але для MVP зійде
            return BadRequest(new { Message = ex.Message });
        }
    }
}

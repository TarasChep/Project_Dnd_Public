namespace DnD.Application.DTOs;

public class UpdateValueDto
{
    /// <summary>
    /// The amount to change. Can be positive (heal/add XP) or negative (damage/remove XP).
    /// </summary>
    public int Amount { get; set; }
}

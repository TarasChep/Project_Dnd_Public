namespace DnD.Domain.Entities;

/// <summary>
/// Базовий клас для всіх сутностей бази даних.
/// Містить поля, які повинні бути у кожної таблиці.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastModifiedAt { get; set; }
}

using DnD.Domain.Enums; // КРИТИЧНО

namespace DnD.Application.DTOs;

public class PerformRestDto
{
    // Фронтенд буде надсилати сюди 1 (Short) або 2 (Long)
    public RestType RestType { get; set; }
}

using System.ComponentModel.DataAnnotations;

namespace PrinzipPriceMonitor.Models.DTOs;

/// <summary>
/// DTO для запроса на обновление настроек сервиса.
/// </summary>
public class UpdateSettingsRequest
{
    /// <summary>
    /// Новый интервал проверки цен в секундах.
    /// Должен быть в диапазоне от 5 до 86400 секунд (24 часа).
    /// </summary>
    /// <example>60</example>
    [Required]
    [Range(5, 86400, ErrorMessage = "Интервал должен быть в диапазоне от 5 до 86400 секунд.")]
    public int CheckIntervalSeconds { get; set; }
}
namespace PrinzipPriceMonitor.Models.DTOs;

/// <summary>
/// DTO для возврата текущих настроек сервиса.
/// </summary>
public class SettingsResponse
{
    /// <summary>
    /// Текущий интервал проверки цен в секундах.
    /// </summary>
    public int CheckIntervalSeconds { get; set; }

    /// <summary>
    /// Минимально допустимый интервал проверки в секундах.
    /// </summary>
    public int MinIntervalSeconds { get; set; }

    /// <summary>
    /// Максимально допустимый интервал проверки в секундах.
    /// </summary>
    public int MaxIntervalSeconds { get; set; }
}
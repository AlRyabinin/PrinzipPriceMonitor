namespace PrinzipPriceMonitor.Services;

/// <summary>
/// Сервис для управления настройками приложения.
/// Реализован как Singleton, чтобы все компоненты использовали единое состояние.
/// Потокобезопасен благодаря использованию lock.
/// </summary>
public class SettingsService : ISettingsService
{
    private readonly object _lock = new();
    private int _checkIntervalSeconds;

    /// <summary>
    /// Минимально допустимый интервал проверки (в секундах).
    /// Ограничение введено для предотвращения блокировки со стороны prinzip.su (DDoS).
    /// </summary>
    public const int MinIntervalSeconds = 5;

    /// <summary>
    /// Максимально допустимый интервал проверки (в секундах).
    /// 86400 секунд = 24 часа.
    /// </summary>
    public const int MaxIntervalSeconds = 86400;

    /// <summary>
    /// Инициализирует сервис настроек с интервалом по умолчанию (300 секунд = 5 минут).
    /// </summary>
    public SettingsService()
    {
        _checkIntervalSeconds = 300;
    }

    /// <inheritdoc/>
    public int GetCheckIntervalSeconds()
    {
        lock(_lock)
        {
            return _checkIntervalSeconds;
        }
    }

    /// <inheritdoc/>
    public void SetCheckIntervalSeconds(int seconds)
    {
        if(seconds < MinIntervalSeconds || seconds > MaxIntervalSeconds)
        {
            throw new ArgumentOutOfRangeException(
                nameof(seconds),
                seconds,
                $"Интервал должен быть в диапазоне от {MinIntervalSeconds} до {MaxIntervalSeconds} секунд.");
        }

        lock(_lock)
        {
            _checkIntervalSeconds = seconds;
        }
    }

    /// <inheritdoc/>
    public TimeSpan GetCheckInterval()
    {
        return TimeSpan.FromSeconds(GetCheckIntervalSeconds());
    }
}
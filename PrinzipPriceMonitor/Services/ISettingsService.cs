namespace PrinzipPriceMonitor.Services;

/// <summary>
/// Интерфейс сервиса для управления настройками приложения.
/// Позволяет динамически изменять параметры работы без перезапуска сервиса.
/// </summary>
public interface ISettingsService
{
    /// <summary>
    /// Получает текущий интервал проверки цен в секундах.
    /// </summary>
    /// <returns>Интервал проверки в секундах.</returns>
    int GetCheckIntervalSeconds();

    /// <summary>
    /// Устанавливает новый интервал проверки цен.
    /// </summary>
    /// <param name="seconds">Новый интервал в секундах. Должен быть больше 0.</param>
    /// <exception cref="ArgumentOutOfRangeException">Выбрасывается, если значение вне допустимого диапазона.</exception>
    void SetCheckIntervalSeconds(int seconds);

    /// <summary>
    /// Получает текущий интервал проверки цен в виде TimeSpan.
    /// </summary>
    /// <returns>Интервал проверки как TimeSpan.</returns>
    TimeSpan GetCheckInterval();
}
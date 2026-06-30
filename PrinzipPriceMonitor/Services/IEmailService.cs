namespace PrinzipPriceMonitor.Services;

/// <summary>
/// Абстракция сервиса отправки email-уведомлений.
/// Позволяет подменять транспорт (SMTP, SendGrid, Mailgun) и упрощает тестирование.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Отправляет получателю письмо с уведомлением об изменении цены квартиры.
    /// </summary>
    /// <param name="to">Email-адрес получателя уведомления.</param>
    /// <param name="url">URL объявления, цена которого изменилась.</param>
    /// <param name="oldPrice">Предыдущее значение цены в рублях.</param>
    /// <param name="newPrice">Новое значение цены в рублях.</param>
    /// <returns>Задача, представляющая асинхронную операцию отправки.</returns>
    /// <remarks>
    /// Метод не генерирует исключений при ошибках отправки — они логируются внутри реализации.
    /// </remarks>
    Task SendPriceChangeNotificationAsync(string to, string url, decimal oldPrice, decimal newPrice);
}
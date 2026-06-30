namespace PrinzipPriceMonitor.Services;

/// <summary>
/// Абстракция парсера цены со страницы объявления.
/// Позволяет подменять реализацию (HTML-парсинг, API мобильного сайта и т.д.)
/// и упрощает модульное тестирование.
/// </summary>
public interface IPriceParser
{
    /// <summary>
    /// Асинхронно извлекает цену квартиры со страницы объявления по указанному URL.
    /// </summary>
    /// <param name="url">Абсолютный URL страницы объявления на prinzip.su.</param>
    /// <param name="cancellationToken">Токен отмены асинхронной операции.</param>
    /// <returns>
    /// Цена в рублях, если удалось извлечь значение; <c>null</c> — если страница недоступна,
    /// структура изменилась или цена не найдена.
    /// </returns>
    Task<decimal?> ParsePriceAsync(string url, CancellationToken cancellationToken = default);
}
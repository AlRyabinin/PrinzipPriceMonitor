using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace PrinzipPriceMonitor.Services;

/// <summary>
/// Сервис для парсинга цен квартир с сайта prinzip.su.
/// Извлекает текущую цену со скидкой из блока "Полная оплата".
/// </summary>
public class PrinzipPriceParser : IPriceParser
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PrinzipPriceParser> _logger;

    /// <summary>
    /// Инициализирует новый экземпляр парсера цен.
    /// </summary>
    /// <param name="httpClient">HTTP-клиент для загрузки страниц.</param>
    /// <param name="logger">Логгер для записи событий.</param>
    public PrinzipPriceParser(HttpClient httpClient, ILogger<PrinzipPriceParser> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// Парсит текущую цену квартиры со скидкой с указанной страницы prinzip.su.
    /// Ищет цену в блоке с классом WdgtPaymentMethods...promo_price.
    /// </summary>
    /// <param name="url">URL страницы объявления.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Текущая цена квартиры в рублях, или null, если цена не найдена.</returns>
    public async Task<decimal?> ParsePriceAsync(string url, CancellationToken cancellationToken = default)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");

            _logger.LogInformation("Загрузка страницы {Url}", url);
            var html = await _httpClient.GetStringAsync(url, cancellationToken);

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var priceText = ExtractPromoPrice(doc);

            if(string.IsNullOrWhiteSpace(priceText))
            {
                _logger.LogWarning("Не удалось найти цену со скидкой на странице {Url}", url);
                return null;
            }

            _logger.LogDebug("Найден текст с ценой: {PriceText}", priceText);

            var price = ParsePriceFromText(priceText);

            if(price.HasValue)
            {
                _logger.LogInformation("Извлечена цена {Price:N0} ₽ с {Url}", price.Value, url);
            }
            else
            {
                _logger.LogWarning("Не удалось распарсить цену из текста: {PriceText}", priceText);
            }

            return price;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Ошибка при парсинге страницы {Url}", url);
            return null;
        }
    }

    /// <summary>
    /// Извлекает цену со скидкой из HTML-документа.
    /// Ищет span с классом, содержащим "promo_price".
    /// </summary>
    /// <param name="doc">HTML-документ.</param>
    /// <returns>Текст с ценой, или null, если не найден.</returns>
    private string? ExtractPromoPrice(HtmlDocument doc)
    {
        var promoPriceNode = doc.DocumentNode.SelectSingleNode(
            "//span[contains(@class, 'promo_price')]"
        );

        if(promoPriceNode != null)
        {
            var text = promoPriceNode.InnerText.Trim();
            _logger.LogDebug("Найден promo_price узел: {Text}", text);

            var priceMatch = Regex.Match(text, @"([\d\s]+)");
            if(priceMatch.Success)
            {
                return priceMatch.Groups[1].Value.Trim();
            }

            return text;
        }

        _logger.LogDebug("promo_price не найден, ищем обычную цену");
        return ExtractRegularPrice(doc);
    }

    /// <summary>
    /// Извлекает обычную цену (без скидки) из HTML-документа.
    /// Ищет span с классом, содержащим "full_price".
    /// </summary>
    /// <param name="doc">HTML-документ.</param>
    /// <returns>Текст с ценой, или null, если не найден.</returns>
    private string? ExtractRegularPrice(HtmlDocument doc)
    {
        var fullPriceNode = doc.DocumentNode.SelectSingleNode(
            "//span[contains(@class, 'full_price')]"
        );

        if(fullPriceNode != null)
        {
            var text = fullPriceNode.InnerText.Trim();
            _logger.LogDebug("Найден full_price узел: {Text}", text);

            var priceMatch = Regex.Match(text, @"([\d\s]+)");
            if(priceMatch.Success)
            {
                return priceMatch.Groups[1].Value.Trim();
            }

            return text;
        }

        return null;
    }

    /// <summary>
    /// Извлекает числовое значение цены из текста.
    /// Удаляет пробелы и преобразует в decimal.
    /// </summary>
    /// <param name="priceText">Текст с ценой (например, "10 887 975").</param>
    /// <returns>Числовое значение цены, или null, если не удалось извлечь.</returns>
    private decimal? ParsePriceFromText(string priceText)
    {
        // Удаляем все пробелы из числа
        var cleanText = Regex.Replace(priceText, @"\s+", "");

        if(string.IsNullOrWhiteSpace(cleanText))
        {
            return null;
        }

        if(decimal.TryParse(cleanText, out var price))
        {
            if(price >= 100_000 && price <= 1_000_000_000)
            {
                return price;
            }
            else
            {
                _logger.LogWarning("Цена вне разумного диапазона: {Price}", price);
            }
        }

        return null;
    }
}
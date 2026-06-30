namespace PrinzipPriceMonitor.Models.DTOs;

/// <summary>
/// DTO ответа со сведениями о подписке и актуальной информацией о цене.
/// Возвращается методами GET <c>/api/subscriptions</c> и GET <c>/api/subscriptions/{id}</c>.
/// </summary>
public class SubscriptionResponse
{
    /// <summary>Идентификатор подписки.</summary>
    public int Id { get; set; }

    /// <summary>URL отслеживаемого объявления.</summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>Email получателя уведомлений.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Последняя известная цена квартиры. <c>null</c>, если цена ещё не была получена.
    /// </summary>
    public decimal? CurrentPrice { get; set; }

    /// <summary>
    /// Предыдущая цена (до последнего изменения). <c>null</c>, если это первая зафиксированная цена.
    /// </summary>
    public decimal? PreviousPrice { get; set; }

    /// <summary>
    /// Флаг, указывающий, что цена изменилась по сравнению с предыдущей проверкой.
    /// </summary>
    public bool PriceChanged { get; set; }

    /// <summary>
    /// Дата и время последней проверки цены в формате UTC.
    /// <c>null</c>, если проверка ещё не выполнялась.
    /// </summary>
    public DateTime? LastChecked { get; set; }

    /// <summary>Дата и время создания подписки в формате UTC.</summary>
    public DateTime CreatedAt { get; set; }
}
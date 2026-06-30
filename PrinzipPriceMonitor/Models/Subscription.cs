using System.ComponentModel.DataAnnotations;

namespace PrinzipPriceMonitor.Models;

/// <summary>
/// Сущность подписки на отслеживание изменения цены квартиры.
/// Каждая подписка привязана к конкретному URL объявления и email получателя уведомлений.
/// </summary>
/// <remarks>
/// Подписки хранятся в БД и используются фоновым сервисом <see cref="Services.PriceCheckerHostedService"/>
/// для периодической проверки цен.
/// </remarks>
public class Subscription
{
    /// <summary>Уникальный идентификатор подписки (первичный ключ).</summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// URL объявления на сайте prinzip.su, за ценой которого ведётся наблюдение.
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Email-адрес получателя уведомлений об изменении цены.
    /// Должен соответствовать формату RFC 5322.
    /// </summary>
    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Флаг активности подписки. Если <c>false</c>, подписка игнорируется
    /// фоновым сервисом при проверке цен (мягкое удаление).
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Дата и время создания подписки в формате UTC.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Навигационное свойство: история изменения цен по данной подписке.
    /// </summary>
    public ICollection<PriceHistory> PriceHistories { get; set; } = new List<PriceHistory>();
}
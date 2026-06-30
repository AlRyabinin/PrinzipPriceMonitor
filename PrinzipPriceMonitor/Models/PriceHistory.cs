using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrinzipPriceMonitor.Models;

/// <summary>
/// Запись истории изменения цены квартиры.
/// Создаётся каждый раз, когда фоновый сервис обнаруживает изменение цены
/// по сравнению с предыдущим известным значением.
/// </summary>
public class PriceHistory
{
    /// <summary>Уникальный идентификатор записи истории (первичный ключ).</summary>
    [Key]
    public int Id { get; set; }

    /// <summary>Внешний ключ на связанную подписку.</summary>
    [Required]
    public int SubscriptionId { get; set; }

    /// <summary>Навигационное свойство: связанная подписка.</summary>
    [ForeignKey("SubscriptionId")]
    public Subscription Subscription { get; set; } = null!;

    /// <summary>
    /// Зафиксированная цена квартиры в рублях на момент проверки.
    /// </summary>
    [Required]
    public decimal Price { get; set; }

    /// <summary>
    /// Дата и время проверки цены в формате UTC.
    /// Используется для сортировки истории и определения актуальной цены.
    /// </summary>
    [Required]
    public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
}
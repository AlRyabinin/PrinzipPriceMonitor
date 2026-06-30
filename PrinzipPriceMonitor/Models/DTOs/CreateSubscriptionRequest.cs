using System.ComponentModel.DataAnnotations;

namespace PrinzipPriceMonitor.Models.DTOs;

/// <summary>
/// DTO для создания новой подписки на отслеживание цены.
/// Передаётся в теле POST-запроса к <c>/api/subscriptions</c>.
/// </summary>
public class CreateSubscriptionRequest
{
    /// <summary>
    /// URL объявления на сайте prinzip.su.
    /// Должен быть валидным абсолютным URL.
    /// </summary>
    [Required]
    [Url]
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Email получателя уведомлений. Должен быть валидным адресом электронной почты.
    /// </summary>
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}
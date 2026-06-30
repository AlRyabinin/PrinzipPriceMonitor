using Microsoft.AspNetCore.Mvc;
using PrinzipPriceMonitor.Services;

namespace PrinzipPriceMonitor.Controllers;

/// <summary>
/// Тестовый контроллер для проверки отправки email-уведомлений.
/// Доступен только в Development среде.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TestEmailController : ControllerBase
{
    private readonly IEmailService _emailService;
    private readonly ILogger<TestEmailController> _logger;

    /// <summary>
    /// Инициализирует новый экземпляр тестового контроллера email.
    /// </summary>
    /// <param name="emailService">Сервис отправки email.</param>
    /// <param name="logger">Логгер для записи событий.</param>
    public TestEmailController(IEmailService emailService, ILogger<TestEmailController> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    /// <summary>
    /// Отправляет тестовое email-уведомление об изменении цены.
    /// </summary>
    /// <param name="email">Email получателя (по умолчанию test@example.com).</param>
    /// <returns>Результат отправки.</returns>
    /// <response code="200">Email успешно отправлен.</response>
    /// <response code="500">Ошибка при отправке email.</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SendTestEmail([FromQuery] string email = "test@example.com")
    {
        try
        {
            _logger.LogInformation("Отправка тестового email на {Email}", email);

            await _emailService.SendPriceChangeNotificationAsync(
                email,
                "https://prinzip.su/test/123/",
                5_000_000,
                4_500_000);

            _logger.LogInformation("Тестовое email успешно отправлено на {Email}", email);
            return Ok(new { message = $"Тестовое email отправлено на {email}. Проверьте MailHog на http://localhost:8025" });
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Ошибка при отправке тестового email на {Email}", email);
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
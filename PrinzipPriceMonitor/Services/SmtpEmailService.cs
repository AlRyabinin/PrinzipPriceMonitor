using MailKit.Net.Smtp;
using MimeKit;

namespace PrinzipPriceMonitor.Services;

/// <summary>
/// Сервис для отправки email-уведомлений через SMTP.
/// Поддерживает как защищённое соединение (StartTls), так и незащищённое (для тестирования с MailHog).
/// </summary>
public class SmtpEmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SmtpEmailService> _logger;

    /// <summary>
    /// Инициализирует новый экземпляр сервиса отправки email.
    /// </summary>
    /// <param name="configuration">Конфигурация приложения.</param>
    /// <param name="logger">Логгер для записи событий.</param>
    public SmtpEmailService(IConfiguration configuration, ILogger<SmtpEmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Отправляет email-уведомление об изменении цены квартиры.
    /// </summary>
    /// <param name="to">Email получателя.</param>
    /// <param name="url">URL объявления с квартирой.</param>
    /// <param name="oldPrice">Старая цена квартиры.</param>
    /// <param name="newPrice">Новая цена квартиры.</param>
    /// <returns>Задача, представляющая асинхронную операцию отправки.</returns>
    public async Task SendPriceChangeNotificationAsync(string to, string url, decimal oldPrice, decimal newPrice)
    {
        var smtpHost = _configuration["Smtp:Host"] ?? "localhost";
        var smtpPort = int.Parse(_configuration["Smtp:Port"] ?? "587");
        var smtpUser = _configuration["Smtp:User"] ?? "";
        var smtpPass = _configuration["Smtp:Password"] ?? "";
        var fromEmail = _configuration["Smtp:FromEmail"] ?? "noreply@prinzip-monitor.local";
        var useSsl = bool.Parse(_configuration["Smtp:UseSsl"] ?? "false");

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Prinzip Price Monitor", fromEmail));
        message.To.Add(new MailboxAddress("", to));
        message.Subject = "Цена квартиры изменилась!";

        var changePercent = ((newPrice - oldPrice) / oldPrice) * 100;
        var changeDirection = newPrice < oldPrice ? "снизилась" : "выросла";

        message.Body = new TextPart("html")
        {
            Text = $@"
                <html>
                <body>
                    <h2>Уведомление об изменении цены</h2>
                    <p>Цена квартиры по адресу <a href='{url}'>{url}</a> {changeDirection}.</p>
                    <ul>
                        <li><strong>Старая цена:</strong> {oldPrice:N0} ₽</li>
                        <li><strong>Новая цена:</strong> {newPrice:N0} ₽</li>
                        <li><strong>Изменение:</strong> {changePercent:F2}%</li>
                    </ul>
                    <p>С уважением,<br/>Prinzip Price Monitor</p>
                </body>
                </html>"
        };

        try
        {
            using var client = new SmtpClient();

            var secureSocketOptions = useSsl
                ? MailKit.Security.SecureSocketOptions.StartTls
                : MailKit.Security.SecureSocketOptions.None;

            _logger.LogDebug("Подключение к SMTP {Host}:{Port} с опциями {Options}",
                smtpHost, smtpPort, secureSocketOptions);

            await client.ConnectAsync(smtpHost, smtpPort, secureSocketOptions);

            if(!string.IsNullOrEmpty(smtpUser))
            {
                await client.AuthenticateAsync(smtpUser, smtpPass);
            }

            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Email успешно отправлен на {Email}", to);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Ошибка при отправке email на {Email}", to);
            throw;
        }
    }
}
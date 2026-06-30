using Microsoft.EntityFrameworkCore;
using PrinzipPriceMonitor.Data;

namespace PrinzipPriceMonitor.Services;

/// <summary>
/// Фоновый сервис для периодической проверки цен квартир.
/// Наследуется от <see cref="BackgroundService"/> и выполняется в течение всего времени работы приложения.
/// Интервал проверки динамически читается из <see cref="ISettingsService"/> перед каждой итерацией.
/// </summary>
public class PriceCheckerHostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IPriceParser _priceParser;
    private readonly IEmailService _emailService;
    private readonly ISettingsService _settingsService;
    private readonly ILogger<PriceCheckerHostedService> _logger;

    /// <summary>
    /// Инициализирует новый экземпляр фонового сервиса проверки цен.
    /// </summary>
    /// <param name="serviceProvider">Провайдер сервисов для создания scoped-контекстов.</param>
    /// <param name="priceParser">Сервис парсинга цен с сайта prinzip.su.</param>
    /// <param name="emailService">Сервис отправки email-уведомлений.</param>
    /// <param name="settingsService">Сервис управления настройками (включая интервал проверки).</param>
    /// <param name="logger">Логгер для записи событий.</param>
    public PriceCheckerHostedService(
        IServiceProvider serviceProvider,
        IPriceParser priceParser,
        IEmailService emailService,
        ISettingsService settingsService,
        ILogger<PriceCheckerHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _priceParser = priceParser;
        _emailService = emailService;
        _settingsService = settingsService;
        _logger = logger;
    }

    /// <summary>
    /// Основной цикл выполнения фонового сервиса.
    /// Выполняется до отмены через <paramref name="stoppingToken"/>.
    /// На каждой итерации проверяет цены всех активных подписок,
    /// затем засыпает на интервал, полученный из <see cref="ISettingsService"/>.
    /// </summary>
    /// <param name="stoppingToken">Токен отмены, сигнализирующий о необходимости остановки сервиса.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Price Checker Service запущен");

        while(!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckPricesAsync(stoppingToken);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Ошибка при проверке цен");
            }

            // Динамически читаем интервал перед каждой паузой
            var interval = _settingsService.GetCheckInterval();
            _logger.LogDebug("Следующая проверка через {Interval}", interval);

            await Task.Delay(interval, stoppingToken);
        }
    }

    /// <summary>
    /// Выполняет проверку цен для всех активных подписок.
    /// Для каждой подписки:
    /// <list type="number">
    ///   <item><description>Парсит актуальную цену с сайта prinzip.su.</description></item>
    ///   <item><description>Сравнивает с последней сохранённой ценой.</description></item>
    ///   <item><description>При изменении цены сохраняет новую запись в историю и отправляет email.</description></item>
    /// </list>
    /// </summary>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Задача, представляющая асинхронную операцию проверки.</returns>
    private async Task CheckPricesAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var subscriptions = await dbContext.Subscriptions
            .Where(s => s.IsActive)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Проверка {Count} подписок", subscriptions.Count);

        foreach(var subscription in subscriptions)
        {
            if(cancellationToken.IsCancellationRequested) break;

            try
            {
                var newPrice = await _priceParser.ParsePriceAsync(subscription.Url, cancellationToken);

                if(newPrice == null)
                {
                    _logger.LogWarning("Не удалось получить цену для {Url}", subscription.Url);
                    continue;
                }

                var lastPrice = await dbContext.PriceHistories
                    .Where(p => p.SubscriptionId == subscription.Id)
                    .OrderByDescending(p => p.CheckedAt)
                    .FirstOrDefaultAsync(cancellationToken);

                if(lastPrice == null || lastPrice.Price != newPrice.Value)
                {
                    var priceHistory = new Models.PriceHistory
                    {
                        SubscriptionId = subscription.Id,
                        Price = newPrice.Value,
                        CheckedAt = DateTime.UtcNow
                    };

                    dbContext.PriceHistories.Add(priceHistory);
                    await dbContext.SaveChangesAsync(cancellationToken);

                    _logger.LogInformation(
                        "Цена изменилась для {Url}: {OldPrice} -> {NewPrice}",
                        subscription.Url,
                        lastPrice?.Price ?? 0,
                        newPrice.Value);

                    if(lastPrice != null)
                    {
                        await _emailService.SendPriceChangeNotificationAsync(
                            subscription.Email,
                            subscription.Url,
                            lastPrice.Price,
                            newPrice.Value);
                    }
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обработке подписки {Id}", subscription.Id);
            }
        }
    }
}
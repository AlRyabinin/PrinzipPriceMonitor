using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrinzipPriceMonitor.Data;
using PrinzipPriceMonitor.Models;
using PrinzipPriceMonitor.Models.DTOs;

namespace PrinzipPriceMonitor.Controllers;

/// <summary>
/// REST-контроллер для управления подписками на отслеживание цен квартир.
/// Предоставляет endpoints для создания, получения и удаления подписок.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SubscriptionsController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<SubscriptionsController> _logger;

    /// <summary>
    /// Создаёт экземпляр контроллера.
    /// </summary>
    /// <param name="dbContext">Контекст базы данных приложения.</param>
    /// <param name="logger">Логгер для записи событий контроллера.</param>
    public SubscriptionsController(AppDbContext dbContext, ILogger<SubscriptionsController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// Создаёт новую подписку на отслеживание цены квартиры по указанному URL.
    /// </summary>
    /// <param name="request">Данные подписки (URL объявления и email получателя).</param>
    /// <returns>
    /// <para><see cref="CreatedAtActionResult"/> (HTTP 201) с данными созданной подписки — при успехе.</para>
    /// <para><see cref="BadRequestObjectResult"/> (HTTP 400) — при невалидных входных данных.</para>
    /// <para><see cref="ConflictObjectResult"/> (HTTP 409) — если подписка на этот URL с этим email уже существует.</para>
    /// </returns>
    /// <response code="201">Подписка успешно создана.</response>
    /// <response code="400">Невалидный URL или email в теле запроса.</response>
    /// <response code="409">Подписка с такой парой (URL, Email) уже существует.</response>
    [HttpPost]
    public async Task<IActionResult> CreateSubscription([FromBody] CreateSubscriptionRequest request)
    {
        if(!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var existing = await _dbContext.Subscriptions
            .FirstOrDefaultAsync(s => s.Url == request.Url && s.Email == request.Email);

        if(existing != null)
        {
            return Conflict(new { message = "Подписка уже существует" });
        }

        var subscription = new Subscription
        {
            Url = request.Url,
            Email = request.Email,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Subscriptions.Add(subscription);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Создана подписка {Id} на {Url}", subscription.Id, subscription.Url);

        return CreatedAtAction(
            nameof(GetSubscription),
            new { id = subscription.Id },
            new
            {
                subscription.Id,
                subscription.Url,
                subscription.Email,
                subscription.CreatedAt
            });
    }

    /// <summary>
    /// Возвращает список всех подписок с актуальными ценами и информацией о последнем изменении.
    /// </summary>
    /// <returns>
    /// <see cref="OkObjectResult"/> (HTTP 200) с массивом <see cref="SubscriptionResponse"/>.
    /// Для каждой подписки включены текущая и предыдущая цена, флаг изменения и дата последней проверки.
    /// </returns>
    /// <response code="200">Список подписок успешно получен.</response>
    [HttpGet]
    public async Task<IActionResult> GetSubscriptions()
    {
        var subscriptions = await _dbContext.Subscriptions
            .Where(s => s.IsActive)
            .Include(s => s.PriceHistories.OrderByDescending(p => p.CheckedAt).Take(2))
            .ToListAsync();

        var response = subscriptions.Select(s =>
        {
            var prices = s.PriceHistories.OrderByDescending(p => p.CheckedAt).ToList();
            var currentPrice = prices.FirstOrDefault();
            var previousPrice = prices.Skip(1).FirstOrDefault();

            return new SubscriptionResponse
            {
                Id = s.Id,
                Url = s.Url,
                Email = s.Email,
                CurrentPrice = currentPrice?.Price,
                PreviousPrice = previousPrice?.Price,
                PriceChanged = previousPrice != null && currentPrice?.Price != previousPrice.Price,
                LastChecked = currentPrice?.CheckedAt,
                CreatedAt = s.CreatedAt
            };
        });

        return Ok(response);
    }

    /// <summary>
    /// Возвращает данные конкретной подписки с актуальной информацией о цене.
    /// </summary>
    /// <param name="id">Идентификатор подписки.</param>
    /// <returns>
    /// <para><see cref="OkObjectResult"/> (HTTP 200) с данными подписки — если найдена.</para>
    /// <para><see cref="NotFoundResult"/> (HTTP 404) — если подписка с указанным ID не существует.</para>
    /// </returns>
    /// <response code="200">Подписка найдена и возвращена.</response>
    /// <response code="404">Подписка с указанным идентификатором не найдена.</response>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetSubscription(int id)
    {
        var subscription = await _dbContext.Subscriptions
            .Where(s => s.IsActive)
            .Include(s => s.PriceHistories.OrderByDescending(p => p.CheckedAt).Take(2))
            .FirstOrDefaultAsync(s => s.Id == id);

        if(subscription == null)
        {
            return NotFound();
        }

        var prices = subscription.PriceHistories.OrderByDescending(p => p.CheckedAt).ToList();
        var currentPrice = prices.FirstOrDefault();
        var previousPrice = prices.Skip(1).FirstOrDefault();

        return Ok(new SubscriptionResponse
        {
            Id = subscription.Id,
            Url = subscription.Url,
            Email = subscription.Email,
            CurrentPrice = currentPrice?.Price,
            PreviousPrice = previousPrice?.Price,
            PriceChanged = previousPrice != null && currentPrice?.Price != previousPrice.Price,
            LastChecked = currentPrice?.CheckedAt,
            CreatedAt = subscription.CreatedAt
        });
    }

    /// <summary>
    /// Деактивирует подписку (мягкое удаление). Подписка перестаёт обрабатываться фоновым сервисом.
    /// </summary>
    /// <param name="id">Идентификатор подписки для удаления.</param>
    /// <returns>
    /// <para><see cref="NoContentResult"/> (HTTP 204) — при успешной деактивации.</para>
    /// <para><see cref="NotFoundResult"/> (HTTP 404) — если подписка не найдена.</para>
    /// </returns>
    /// <response code="204">Подписка успешно деактивирована.</response>
    /// <response code="404">Подписка с указанным идентификатором не найдена.</response>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSubscription(int id)
    {
        var subscription = await _dbContext.Subscriptions.FindAsync(id);

        if(subscription == null)
        {
            return NotFound();
        }

        subscription.IsActive = false;
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Подписка {Id} деактивирована", id);

        return NoContent();
    }
}
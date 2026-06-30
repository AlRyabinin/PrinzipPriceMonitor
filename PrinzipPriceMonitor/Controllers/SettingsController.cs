using Microsoft.AspNetCore.Mvc;
using PrinzipPriceMonitor.Models.DTOs;
using PrinzipPriceMonitor.Services;

namespace PrinzipPriceMonitor.Controllers;

/// <summary>
/// Контроллер для управления настройками сервиса мониторинга цен.
/// Позволяет просматривать и изменять параметры работы фоновых задач.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class SettingsController : ControllerBase
{
    private readonly ISettingsService _settingsService;
    private readonly ILogger<SettingsController> _logger;

    /// <summary>
    /// Инициализирует новый экземпляр контроллера настроек.
    /// </summary>
    /// <param name="settingsService">Сервис управления настройками.</param>
    /// <param name="logger">Логгер для записи событий.</param>
    public SettingsController(ISettingsService settingsService, ILogger<SettingsController> logger)
    {
        _settingsService = settingsService;
        _logger = logger;
    }

    /// <summary>
    /// Получает текущие настройки сервиса.
    /// </summary>
    /// <returns>Объект с текущими настройками, включая интервал проверки и допустимые границы.</returns>
    /// <response code="200">Настройки успешно получены.</response>
    [HttpGet]
    [ProducesResponseType(typeof(SettingsResponse), StatusCodes.Status200OK)]
    public IActionResult GetSettings()
    {
        var response = new SettingsResponse
        {
            CheckIntervalSeconds = _settingsService.GetCheckIntervalSeconds(),
            MinIntervalSeconds = SettingsService.MinIntervalSeconds,
            MaxIntervalSeconds = SettingsService.MaxIntervalSeconds
        };

        return Ok(response);
    }

    /// <summary>
    /// Обновляет настройки сервиса.
    /// </summary>
    /// <param name="request">Новые значения настроек.</param>
    /// <returns>Обновлённые настройки сервиса.</returns>
    /// <response code="200">Настройки успешно обновлены.</response>
    /// <response code="400">Переданы невалидные данные (например, интервал вне допустимого диапазона).</response>
    [HttpPut]
    [ProducesResponseType(typeof(SettingsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult UpdateSettings([FromBody] UpdateSettingsRequest request)
    {
        if(!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            _settingsService.SetCheckIntervalSeconds(request.CheckIntervalSeconds);

            _logger.LogInformation(
                "Интервал проверки изменён на {Interval} секунд",
                request.CheckIntervalSeconds);

            return Ok(new SettingsResponse
            {
                CheckIntervalSeconds = _settingsService.GetCheckIntervalSeconds(),
                MinIntervalSeconds = SettingsService.MinIntervalSeconds,
                MaxIntervalSeconds = SettingsService.MaxIntervalSeconds
            });
        }
        catch(ArgumentOutOfRangeException ex)
        {
            _logger.LogWarning(ex, "Попытка установить невалидный интервал: {Value}", request.CheckIntervalSeconds);
            return BadRequest(new { message = ex.Message });
        }
    }
}
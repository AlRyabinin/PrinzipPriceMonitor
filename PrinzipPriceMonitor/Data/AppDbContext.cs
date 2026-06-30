using Microsoft.EntityFrameworkCore;
using PrinzipPriceMonitor.Models;

namespace PrinzipPriceMonitor.Data;

/// <summary>
/// Контекст базы данных приложения.
/// Предоставляет доступ к таблицам подписок и истории цен через Entity Framework Core.
/// </summary>
/// <remarks>
/// В качестве провайдера используется SQLite (файл <c>prinzip_monitor.db</c>).
/// При необходимости может быть переключён на MS SQL/PostgreSQL заменой провайдера.
/// </remarks>
public class AppDbContext : DbContext
{
    /// <summary>
    /// Создаёт новый экземпляр контекста с заданными параметрами.
    /// </summary>
    /// <param name="options">Параметры конфигурации контекста (провайдер, connection string и т.д.).</param>
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    /// <summary>Таблица активных и деактивированных подписок.</summary>
    public DbSet<Subscription> Subscriptions { get; set; }

    /// <summary>Таблица истории изменения цен по подпискам.</summary>
    public DbSet<PriceHistory> PriceHistories { get; set; }

    /// <inheritdoc />
    /// <remarks>
    /// Настраивает уникальные индексы и индексы для оптимизации запросов:
    /// <list type="bullet">
    ///   <item><description>Уникальный индекс по паре (Url, Email) для предотвращения дубликатов подписок.</description></item>
    ///   <item><description>Составной индекс по (SubscriptionId, CheckedAt) для быстрого поиска последней цены.</description></item>
    /// </list>
    /// </remarks>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Subscription>()
            .HasIndex(s => new { s.Url, s.Email })
            .IsUnique();

        modelBuilder.Entity<PriceHistory>()
            .HasIndex(p => new { p.SubscriptionId, p.CheckedAt });
    }
}
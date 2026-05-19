using Data.ApplicationDbContext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

public abstract class UtilitiesController : ControllerBase
{
    protected readonly AppDbContext _context;
    protected readonly IDbContextFactory<AppDbContext> _contextFactory;
    protected readonly IMemoryCache _cache;

    protected UtilitiesController(AppDbContext context, IDbContextFactory<AppDbContext> contextFactory, IMemoryCache cache)
    {
        _context = context;
        _contextFactory = contextFactory;
        _cache = cache;
    }

    protected async Task<ActionResult<T>> SingleTask<T>(SingleTaskOptions<T> options)
    {
        try
        {
            T task = await options.Task();

            return Ok(task);
        }
        catch (Exception ex)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                $"{options.ErrorMessage}: {ex.Message}"
            );
        }
    }

    protected async Task<ActionResult<TResult>> MultiTask<T1, T2, TResult>(MultiTaskOptions<T1, T2, TResult> options)
    {
        try
        {
            Task<T1> task1 = options.Task1();
            Task<T2> task2 = options.Task2();

            await Task.WhenAll(task1, task2);

            return options.ResultFactory(
                task1.Result,
                task2.Result
            );
        }
        catch (Exception ex)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                $"{options.ErrorMessage}: {ex.Message}"
            );
        }
    }

    protected async Task<ActionResult> SqlFunc(SqlTaskOptions options)
    {
        try
        {
            await options.Sql();

            return Ok(new { message = options.SuccessMessage });
        }
        catch (Exception ex)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                $"{options.ErrorMessage}: {ex.Message}"
            );
        }
    }

    protected async Task<T> CacheFunc<T>(CacheOptions<T> options)
    {
        return (await _cache.GetOrCreateAsync(options.NomeCache, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = options.DurataCache;
            entry.SetPriority(CacheItemPriority.Normal);

            return await options.Task();
        }))!;
    }
}

public class SingleTaskOptions<T>
{
    public required Func<Task<T>> Task { get; set; }
    public string ErrorMessage { get; set; } = "Errore interno del server";
}

public class MultiTaskOptions<T1, T2, TResult>
{
    public required Func<Task<T1>> Task1 { get; set; }
    public required Func<Task<T2>> Task2 { get; set; }
    public required Func<T1, T2, TResult> ResultFactory { get; set; }
    public string ErrorMessage { get; set; } = "Errore interno del server";
}

public class SqlTaskOptions
{
    public required Func<Task> Sql { get; set; }
    public string SuccessMessage { get; set; } = "Opeazione compleatata";
    public string ErrorMessage { get; set; } = "Errore interno del server";
}

public class CacheOptions<T>
{
    public required Func<Task<T>> Task { get; set; }
    public required string NomeCache { get; set; }
    public required TimeSpan DurataCache { get; set; }
}